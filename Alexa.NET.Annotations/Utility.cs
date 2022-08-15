using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Alexa.NET.Annotations
{
    internal class Utility
    {
        public static ThrowStatementSyntax NotImplemented() => SF.ThrowStatement(
            SF.ObjectCreationExpression(SF.IdentifierName(nameof(NotImplementedException)),
                SF.ArgumentList(), null));

        public static NameSyntax? BuildName(params string[] pieces) => pieces.Aggregate<string?, NameSyntax?>(null, (current, piece) => current == null
            ? SF.IdentifierName(piece)
            : SF.QualifiedName(current, SF.IdentifierName(piece)));

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
