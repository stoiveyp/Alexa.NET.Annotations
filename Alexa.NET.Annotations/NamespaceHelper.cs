using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Alexa.NET.Annotations;

internal static class NamespaceHelper{
    //{
    //    public static ThrowStatementSyntax NotImplemented() => SF.ThrowStatement(
    //        SF.ObjectCreationExpression(SF.IdentifierName(nameof(NotImplementedException)),
    //            SF.ArgumentList(), null));

    public static NameSyntax? Build(params string[] pieces) => pieces.Aggregate<string?, NameSyntax?>(null, (current, piece) => current == null
        ? SyntaxFactory.IdentifierName(piece!)
        : SyntaxFactory.QualifiedName(current, SyntaxFactory.IdentifierName(piece!)));

    public static NameSyntax? Find(ClassDeclarationSyntax cls)
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