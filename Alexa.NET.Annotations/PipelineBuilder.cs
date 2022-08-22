using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Alexa.NET.Annotations
{
    internal static class PipelineBuilder
    {
        public static CompilationUnitSyntax BuildPipelineClasses(ClassDeclarationSyntax cls, Action<Diagnostic> reportDiagnostic)
        {
            var usings = SF.List(new[]
            {
                SF.UsingDirective(Strings.Usings.System()),
                SF.UsingDirective(Strings.Usings.AlexaNetRequest()),
                SF.UsingDirective(Strings.Usings.AlexaNetResponse()),
                SF.UsingDirective(Strings.Usings.AlexaNetResponseType()),
                SF.UsingDirective(Strings.Usings.RequestHandlers()),
                SF.UsingDirective(Strings.Usings.RequestHandlerTypes()),
                SF.UsingDirective(Strings.Usings.Tasks()),
            }.Distinct());

            var initialSetup = SF.CompilationUnit().WithUsings(usings);

            var nsName = Utility.FindNamespace(cls);

            var skillClass = SF.ClassDeclaration(cls.Identifier.Text)
                .WithModifiers(SF.TokenList(
                    SF.Token(SyntaxKind.PublicKeyword),
                    SF.Token(SyntaxKind.PartialKeyword)));


            var skill = skillClass.BuildSkill(cls, reportDiagnostic);

            if (nsName != null)
            {
                return initialSetup.AddMembers(SF.NamespaceDeclaration(nsName).AddMembers(skill));
            }

            return initialSetup.AddMembers(skill);
        }

        public static ClassDeclarationSyntax BuildSkill(this ClassDeclarationSyntax skillClass, ClassDeclarationSyntax cls, Action<Diagnostic> reportDiagnostic)
        {
            var handlers = cls.Members.OfType<MethodDeclarationSyntax>()
                .Where(MarkerHelper.HasMarkerAttribute).Select(m => m.ToPipelineHandler(m.MarkerAttribute()!, cls, reportDiagnostic));

            return skillClass
                .AddPipelineField()
                .AddExecuteMethod()
                .AddInitialization(handlers);

        }

        public static ClassDeclarationSyntax AddPipelineField(this ClassDeclarationSyntax skillClass)
        {
            var field = SF.FieldDeclaration(SF.VariableDeclaration(SF.IdentifierName(Strings.Types.PipelineClass)))
                .AddDeclarationVariables(SF.VariableDeclarator(Strings.Names.PipelineField))
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PrivateKeyword)));
            return skillClass.AddMembers(field);
        }

        public static ClassDeclarationSyntax AddInitialization(this ClassDeclarationSyntax skillClass,
            IEnumerable<ClassDeclarationSyntax> handlers)
        {
            var arrayType = SF.ArrayType(SF.GenericName(
                    SF.Identifier(Strings.Types.RequestHandlerInterface))
                .WithTypeArgumentList(SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(Strings.Types.SkillRequest)))))
                .WithRankSpecifiers(SF.SingletonList<ArrayRankSpecifierSyntax>(SF.ArrayRankSpecifier(SF.SingletonSeparatedList<ExpressionSyntax>(SF.OmittedArraySizeExpression()))));

            var newPipeline = SF.ObjectCreationExpression(SF.IdentifierName(Strings.Types.PipelineClass))
                .WithArgumentList(SF.ArgumentList(SF.SeparatedList(new[]{SF.Argument(
                SF.ArrayCreationExpression(arrayType,
                    SF.InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                        SF.SeparatedList<ExpressionSyntax>(
                        handlers.Select(h =>
                            SF.ObjectCreationExpression(SF.IdentifierName(h.Identifier.Text)).WithArgumentList(
                                SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(SF.ThisExpression())))))))))})));

            var initializeMethod =
                SF.MethodDeclaration(SF.PredefinedType(SF.Token(SyntaxKind.VoidKeyword)), Strings.Names.InitializeMethod)
                    .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword)))
                    .AddBodyStatements(
                        SF.ExpressionStatement(SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SF.IdentifierName(Strings.Names.PipelineField), newPipeline)));
            return skillClass.AddMembers(initializeMethod).AddMembers(handlers.ToArray());
        }

        public static ClassDeclarationSyntax AddExecuteMethod(this ClassDeclarationSyntax skillClass)
        {
            var invokePipeline = SF.InvocationExpression(
                SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName(Strings.Names.PipelineField), SF.IdentifierName(Strings.Names.ProcessMethod)))
                .WithArgumentList(SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(SF.IdentifierName(Strings.Names.SkillRequestParameter)))));

            var executeMethod = SF.MethodDeclaration(
                    SF.GenericName(SF.Identifier(nameof(Task)),
                        SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(Strings.Types.SkillResponse)))),
                    Strings.Names.ExecuteMethod)
                .WithParameterList(SF.ParameterList(SF.SingletonSeparatedList(SF.Parameter(SF.Identifier(Strings.Names.SkillRequestParameter)).WithType(SF.IdentifierName(Strings.Types.SkillRequest)))))
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.VirtualKeyword)))
                .WithExpressionBody(SF.ArrowExpressionClause(invokePipeline)).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken));

            return skillClass.AddMembers(executeMethod);
        }
    }
}
