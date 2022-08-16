using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Alexa.NET.Annotations
{
    internal static class Utility
    {
        public static ThrowStatementSyntax NotImplemented() => SF.ThrowStatement(
            SF.ObjectCreationExpression(SF.IdentifierName(nameof(NotImplementedException)),
                SF.ArgumentList(), null));

        public static NameSyntax? BuildName(params string[] pieces) => pieces.Aggregate<string?, NameSyntax?>(null, (current, piece) => current == null
            ? SF.IdentifierName(piece)
            : SF.QualifiedName(current, SF.IdentifierName(piece)));

        public static bool ReturnsTask(this MethodDeclarationSyntax method) =>
            method.ReturnType is GenericNameSyntax gen && gen.Identifier.Text == "Task";

        public static InvocationExpressionSyntax WrapIfAsync(this InvocationExpressionSyntax expression, MethodDeclarationSyntax method)
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

        public static NameSyntax? FindNamespace(ClassDeclarationSyntax cls)
        {
            var containerNs = cls.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            if (containerNs != null)
            {
                return containerNs.Name;
            }

            var unit = cls.Ancestors().OfType<CompilationUnitSyntax>().FirstOrDefault();
            var fileScope = unit?.Members.OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault();
            return fileScope?.Name;
        }
    }
}
