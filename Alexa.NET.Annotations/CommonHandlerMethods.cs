using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Alexa.NET.Annotations
{
    internal static class CommonHandlerMethods
    {
        private const string WrapperPropertyName = "Wrapper";

        public static ClassDeclarationSyntax AddWrapperConstructor(this ClassDeclarationSyntax skillClass, ClassDeclarationSyntax wrapperClass)
        {
            var handlerParameter = SF.Parameter(SF.Identifier("wrapper"))
                .WithType(SF.IdentifierName(wrapperClass.Identifier.Text));

            var constructor = SF.ConstructorDeclaration(skillClass.Identifier.Text)
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PrivateKeyword)))
                .WithParameterList(SF.ParameterList(SF.SingletonSeparatedList(handlerParameter)))
                .WithBody(SF.Block(SF.ExpressionStatement(SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,SF.IdentifierName(WrapperPropertyName),SF.IdentifierName("wrapper")))));
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

        public static ClassDeclarationSyntax AddExecuteMethod(this ClassDeclarationSyntax skillClass, MethodDeclarationSyntax method)
        {
            return skillClass;
        }

    }
}
