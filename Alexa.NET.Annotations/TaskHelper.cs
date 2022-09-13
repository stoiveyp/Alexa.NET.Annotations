using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Alexa.NET.Annotations
{
    internal static class TaskHelper
    {
        public static bool ReturnsTask(this MethodDeclarationSyntax method) =>
            method.IsTask() || method.ReturnType is GenericNameSyntax { Identifier.Text: "Task" };

        public static bool IsTask(this MethodDeclarationSyntax method) => method.ReturnType is IdentifierNameSyntax
        {
            Identifier.Text: "Task"
        };

        public static InvocationExpressionSyntax WrapIfNotAsync(this InvocationExpressionSyntax expression, MethodDeclarationSyntax method)
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
    }
}
