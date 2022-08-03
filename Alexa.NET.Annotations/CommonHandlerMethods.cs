using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Alexa.NET.Annotations
{
    internal static class CommonHandlerMethods
    {
        private const string WrapperPropertyName = "Wrapper";
        private const string HandlerMethodName = "Handle";

        public static ClassDeclarationSyntax AddWrapperConstructor(this ClassDeclarationSyntax skillClass, ClassDeclarationSyntax wrapperClass, ConstructorInitializerSyntax? initializer)
        {
            var handlerParameter = SF.Parameter(SF.Identifier("wrapper"))
                .WithType(SF.IdentifierName(wrapperClass.Identifier.Text));

            var constructor = SF.ConstructorDeclaration(skillClass.Identifier.Text)
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.InternalKeyword)))
                .WithParameterList(SF.ParameterList(SF.SingletonSeparatedList(handlerParameter)))
                .WithBody(SF.Block(SF.ExpressionStatement(SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    SF.IdentifierName(WrapperPropertyName), SF.IdentifierName("wrapper")))));

            if (initializer != null)
            {
                constructor = constructor.WithInitializer(initializer);
            }

            return skillClass.AddMembers(constructor);
        }

        public static ClassDeclarationSyntax AddWrapperField(this ClassDeclarationSyntax skillClass,
            ClassDeclarationSyntax wrapperClass)
        {
            var handlerField = SF
                .PropertyDeclaration(SF.IdentifierName(wrapperClass.Identifier.Text), WrapperPropertyName)
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PrivateKeyword)))
                .WithAccessorList(SF.AccessorList(SF.SingletonList(SF.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken)))));
            return skillClass.AddMembers(handlerField);
        }

        public static ClassDeclarationSyntax AddExecuteMethod(this ClassDeclarationSyntax skillClass, MethodDeclarationSyntax method, MarkerInfo info)
        {
            var returnType = method.ReturnsTask()
                ? method.ReturnType
                : SF.GenericName("Task").WithTypeArgumentList(
                    SF.TypeArgumentList(SF.SingletonSeparatedList(method.ReturnType)));

            var newMethod = SF.MethodDeclaration(returnType, HandlerMethodName)
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.OverrideKeyword)))
                .WithParameterList(SF.ParameterList(SF.SingletonSeparatedList(
                    SF.Parameter(SF.Identifier("information")).WithType(
                        SF.GenericName(SF.Identifier("AlexaRequestInformation"),
                            SF.TypeArgumentList(SF.SingletonSeparatedList(SF.ParseTypeName("SkillRequest")))))
                )))
                .WithExpressionBody(SF.ArrowExpressionClause(WrapInTask(method,RunWrapper(method, info)))).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken));
            return skillClass.AddMembers(newMethod);
        }

        private static bool ReturnsTask(this MethodDeclarationSyntax method) =>
            method.ReturnType is GenericNameSyntax gen && gen.Identifier.Text == "Task";

        private static InvocationExpressionSyntax WrapInTask(MethodDeclarationSyntax method, InvocationExpressionSyntax expression)
        {
            if (method.ReturnsTask())
            {
                return expression;
            }

            return SF.InvocationExpression(
                SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    SF.IdentifierName("Task"),
                    SF.IdentifierName("FromResult"))).WithArgumentList(
                SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(expression))));
        }

        private static InvocationExpressionSyntax RunWrapper(MethodDeclarationSyntax method, MarkerInfo info)
        {
            return SF.InvocationExpression(
                SF.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SF.IdentifierName(WrapperPropertyName),
                    SF.IdentifierName(method.Identifier.Text)),
                SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(SF.CastExpression(info.RequestType,
                    SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,SF.IdentifierName("information"),SF.IdentifierName("SkillRequest")),
                        SF.IdentifierName("Request")
                        ))))));
        }
    }
}
