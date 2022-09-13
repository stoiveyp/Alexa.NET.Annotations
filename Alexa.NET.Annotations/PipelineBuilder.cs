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
            var skillClass = SF.ClassDeclaration(cls.Identifier.Text)
                .WithModifiers(SF.TokenList(
                    SF.Token(SyntaxKind.PublicKeyword),
                    SF.Token(SyntaxKind.PartialKeyword)));


            var skillInfo = skillClass.BuildSkill(cls, reportDiagnostic);

            var usings = SF.List(new[]
            {
                Strings.Usings.System(),
                Strings.Usings.AlexaNetRequest(),
                Strings.Usings.AlexaNetResponse(),
                Strings.Usings.AlexaNetResponseType(),
                Strings.Usings.RequestHandlers(),
                Strings.Usings.RequestHandlerTypes(),
                Strings.Usings.Tasks(),
                skillInfo.HasInterceptors ? Strings.Usings.Interceptors() : null
            }.Where(u => u != null).Select(SF.UsingDirective!));

            var nsName = NamespaceHelper.Find(cls);
            var initialSetup = SF.CompilationUnit().WithUsings(usings);

            if (nsName != null)
            {
                return initialSetup.AddMembers(SF.NamespaceDeclaration(nsName).AddMembers(skillInfo.SkillClass!));
            }

            return initialSetup.AddMembers(skillInfo.SkillClass!);
        }

        public static SkillInformation BuildSkill(this ClassDeclarationSyntax skillClass, ClassDeclarationSyntax cls, Action<Diagnostic> reportDiagnostic)
        {
            var requestType = (cls.RequestType() as IdentifierNameSyntax)?.Identifier.Text ?? Strings.Types.SkillRequest;
            var info = SkillInformation.GenerateFrom(cls, requestType, reportDiagnostic);

            info.SetBuiltSkill(skillClass
                .AddPipelineField(requestType)
                .AddExecuteMethod(requestType)
                .AddInitialization(info, requestType));
            return info;

        }

        public static ClassDeclarationSyntax AddPipelineField(this ClassDeclarationSyntax skillClass, string requestType)
        {
            var field = SF.FieldDeclaration(SF.VariableDeclaration(PipelineType(requestType)))
                .AddDeclarationVariables(SF.VariableDeclarator(Strings.Names.PipelineField))
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PrivateKeyword)));
            return skillClass.AddMembers(field);
        }

        public static ClassDeclarationSyntax AddInitialization(this ClassDeclarationSyntax skillClass, SkillInformation information, string requestType)
        {
            var argumentList = new List<ArgumentSyntax> { information.HandlerArray() };

            if (information.HasInterceptors)
            {
                //Error Handlers - not yet supported
                argumentList.Add(SF.Argument(SF.LiteralExpression(SyntaxKind.NullLiteralExpression)));
                argumentList.Add(information.InterceptorArray());
                argumentList.Add(SF.Argument(SF.LiteralExpression(SyntaxKind.NullLiteralExpression)));
            }

            var newPipeline = SF.ObjectCreationExpression(PipelineType(requestType))
                .WithArgumentList(SF.ArgumentList(SF.SeparatedList(argumentList)));

            var initializeMethod =
                SF.MethodDeclaration(SF.PredefinedType(SF.Token(SyntaxKind.VoidKeyword)), Strings.Names.InitializeMethod)
                    .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword)))
                    .AddBodyStatements(
                        SF.ExpressionStatement(SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SF.IdentifierName(Strings.Names.PipelineField), newPipeline)));
            return skillClass.AddMembers(initializeMethod).AddMembers(information.Handlers).AddMembers(information.Interceptors);
        }

        private static TypeSyntax PipelineType(string requestType)
        {
            if (requestType == Strings.Types.SkillRequest || requestType == Strings.Types.FullSkillRequest)
            {
                return SF.IdentifierName(Strings.Types.PipelineClass);
            }
            return SF
                .GenericName(SF.Identifier(Strings.Types.PipelineClass)).WithTypeArgumentList(
                    SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(requestType))));
        }

        private static ArgumentSyntax HandlerArray(this SkillInformation information)
        {
            var arrayType = SF.ArrayType(SF.GenericName(
                        SF.Identifier(Strings.Types.RequestHandlerInterface))
                    .WithTypeArgumentList(SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(information.SkillRequestType)))))
                .WithRankSpecifiers(SF.SingletonList(SF.ArrayRankSpecifier(SF.SingletonSeparatedList<ExpressionSyntax>(SF.OmittedArraySizeExpression()))));

            return SF.Argument(
                SF.ArrayCreationExpression(arrayType,
                    SF.InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                        SF.SeparatedList<ExpressionSyntax>(
                            information.Handlers.Select(h =>
                                SF.ObjectCreationExpression(SF.IdentifierName(h.Identifier.Text)).WithArgumentList(
                                    SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(SF.ThisExpression())))))))));
        }

        private static ArgumentSyntax InterceptorArray(this SkillInformation information)
        {
            var arrayType = SF.ArrayType(SF.GenericName(
                        SF.Identifier(Strings.Types.RequestInterceptorInterface))
                    .WithTypeArgumentList(SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(information.SkillRequestType)))))
                .WithRankSpecifiers(SF.SingletonList(SF.ArrayRankSpecifier(SF.SingletonSeparatedList<ExpressionSyntax>(SF.OmittedArraySizeExpression()))));

            return SF.Argument(
                SF.ArrayCreationExpression(arrayType,
                    SF.InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                        SF.SeparatedList<ExpressionSyntax>(
                            information.Interceptors.Select(h =>
                                SF.ObjectCreationExpression(SF.IdentifierName(h.Identifier.Text)).WithArgumentList(
                                    SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(SF.ThisExpression())))))))));
        }

        public static ClassDeclarationSyntax AddExecuteMethod(this ClassDeclarationSyntax skillClass, string requestType)
        {
            var invokePipeline = SF.InvocationExpression(
                SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName(Strings.Names.PipelineField), SF.IdentifierName(Strings.Names.ProcessMethod)))
                .WithArgumentList(SF.ArgumentList(SF.SeparatedList(new []
                {
                    SF.Argument(SF.IdentifierName(Strings.Names.SkillRequestParameter)),
                    SF.Argument(SF.IdentifierName(Strings.Names.ContextParameter))
                })));

            var executeMethod = SF.MethodDeclaration(
                    SF.GenericName(SF.Identifier(nameof(Task)),
                        SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(Strings.Types.SkillResponse)))),
                    Strings.Names.ExecuteMethod)
                .WithParameterList(
                    SF.ParameterList(SF.SeparatedList(new []
                    {
                        SF.Parameter(SF.Identifier(Strings.Names.SkillRequestParameter)).WithType(SF.IdentifierName(requestType)),
                        SF.Parameter(SF.Identifier(Strings.Names.ContextParameter))
                            .WithType(SF.PredefinedType(SF.Token(SyntaxKind.ObjectKeyword)))
                            .WithDefault(SF.EqualsValueClause(SF.LiteralExpression(SyntaxKind.NullLiteralExpression)))
                    })))
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.VirtualKeyword)))
                .WithExpressionBody(SF.ArrowExpressionClause(invokePipeline)).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken));

            return skillClass.AddMembers(executeMethod);
        }
    }
}
