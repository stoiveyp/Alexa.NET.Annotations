using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Alexa.NET.Annotations
{
    internal static class PipelineBuilder
    {
        private const string PipelineClass = "AlexaRequestPipeline";
        private const string PipelineFieldName = "_pipeline";

        public static CompilationUnitSyntax BuildSkillClasses(ClassDeclarationSyntax cls)
        {
            var usings = SF.List(new[]
            {
                SF.UsingDirective(Utility.BuildName("System")!),
                SF.UsingDirective(Utility.BuildName("Alexa","NET","Request")!),
                SF.UsingDirective(Utility.BuildName("Alexa","NET","Response")!),
                SF.UsingDirective(Utility.BuildName("Alexa","NET","Request","Type")!),
                SF.UsingDirective(Utility.BuildName("Alexa","NET","RequestHandlers")!),
                SF.UsingDirective(Utility.BuildName("Alexa","NET","RequestHandlers","Handlers")!),
                SF.UsingDirective(Utility.BuildName("System","Threading","Tasks")!),
            }.Distinct());

            var initialSetup = SF.CompilationUnit().WithUsings(usings);

            var nsName = Utility.FindNamespace(cls);

            var skillClass = SF.ClassDeclaration(cls.Identifier.Text)
                .WithModifiers(SF.TokenList(
                    SF.Token(SyntaxKind.PublicKeyword),
                    SF.Token(SyntaxKind.PartialKeyword)));


            if (nsName != null)
            {
                return initialSetup.AddMembers(SF.NamespaceDeclaration(nsName).AddMembers(skillClass.BuildSkill(cls)));
            }

            return initialSetup.AddMembers(skillClass.BuildSkill(cls));
        }

        public static ClassDeclarationSyntax BuildSkill(this ClassDeclarationSyntax skillClass, ClassDeclarationSyntax cls)
        {
            var handlers = cls.Members.OfType<MethodDeclarationSyntax>()
                .Where(MarkerHelper.HasMarkerAttribute).Select(m => MethodToPipelineClass(m, m.MarkerAttribute()!, cls));

            return skillClass
                .AddPipelineField()
                .AddExecuteMethod()
                .AddInitialization(handlers);

        }

        public static ClassDeclarationSyntax AddPipelineField(this ClassDeclarationSyntax skillClass)
        {
            var field = SF.FieldDeclaration(SF.VariableDeclaration(SF.IdentifierName("AlexaRequestPipeline")))
                .AddDeclarationVariables(SF.VariableDeclarator(PipelineFieldName))
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PrivateKeyword)));
            return skillClass.AddMembers(field);
        }

        public static ClassDeclarationSyntax AddInitialization(this ClassDeclarationSyntax skillClass,
            IEnumerable<ClassDeclarationSyntax> handlers)
        {
            var newPipeline = SF.ObjectCreationExpression(SF.IdentifierName(PipelineClass))
                .WithArgumentList(SF.ArgumentList(SF.SeparatedList(new[]{SF.Argument(
                SF.ArrayCreationExpression(SF.ArrayType(SF.IdentifierName("IAlexaRequestHandler<SkillRequest>[]")),
                    SF.InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                        SF.SeparatedList<ExpressionSyntax>(
                        handlers.Select(h =>
                            SF.ObjectCreationExpression(SF.IdentifierName(h.Identifier.Text)).WithArgumentList(
                                SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(SF.ThisExpression())))))))))})));

            var initializeMethod =
                SF.MethodDeclaration(SF.PredefinedType(SF.Token(SyntaxKind.VoidKeyword)), "Initialize")
                    .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword)))
                    .AddBodyStatements(
                        SF.ExpressionStatement(SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SF.IdentifierName(PipelineFieldName), newPipeline)));
            return skillClass.AddMembers(initializeMethod).AddMembers(handlers.ToArray());
        }

        public static ClassDeclarationSyntax AddExecuteMethod(this ClassDeclarationSyntax skillClass)
        {
            const string skillRequestParameterName = "skillRequest";
            var invokePipeline = SF.InvocationExpression(
                SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName(PipelineFieldName), SF.IdentifierName("Process")))
                .WithArgumentList(SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(SF.IdentifierName(skillRequestParameterName)))));

            var executeMethod = SF.MethodDeclaration(
                    SF.GenericName(SF.Identifier(nameof(Task)),
                        SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName("SkillResponse")))),
                    "Execute")
                .WithParameterList(SF.ParameterList(SF.SingletonSeparatedList(SF.Parameter(SF.Identifier("skillRequest")).WithType(SF.IdentifierName("SkillRequest")))))
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.VirtualKeyword)))
                .WithExpressionBody(SF.ArrowExpressionClause(invokePipeline)).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken));

            return skillClass.AddMembers(executeMethod);
        }

        private static ClassDeclarationSyntax MethodToPipelineClass(MethodDeclarationSyntax method, AttributeSyntax marker, ClassDeclarationSyntax containerClass)
        {
            if (marker == null) throw new ArgumentNullException(nameof(marker));
            var info = MarkerInfo.MarkerTypeInfo[marker.MarkerName()!];
            return SF.ClassDeclaration(method.Identifier.Text + "Handler")
                .WithBaseList(SF.BaseList(SF.SingletonSeparatedList(info.BaseType(marker))))
                .WithModifiers(SF.TokenList(
                    SF.Token(SyntaxKind.PrivateKeyword)))
                .AddWrapperField(containerClass)
                .AddWrapperConstructor(containerClass, info.Constructor?.Invoke(marker))
                .AddExecuteMethod(method, info);
        }
    }
}
