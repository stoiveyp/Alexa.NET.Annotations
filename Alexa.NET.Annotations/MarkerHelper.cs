using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Alexa.NET.Annotations;

public static class MarkerHelper
{
    public static ConstructorInitializerSyntax IntentConstructor(AttributeSyntax attribute)
    {
        return SF.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer,
            SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(attribute.ArgumentList!.Arguments.First().Expression))));
    }

    internal static string? MarkerName(this AttributeSyntax attribute) =>
        attribute.Name is IdentifierNameSyntax id ? id.Identifier.Text : null;

    internal static AttributeSyntax? MarkerAttribute(this MethodDeclarationSyntax method)
    {
        return method.AttributeLists.SelectMany(a => a.Attributes)
            .FirstOrDefault(a => MarkerInfo.List.Contains(a.MarkerName()));
    }

    internal static bool HasMarkerAttribute(this MethodDeclarationSyntax method)
    {
        return MarkerAttribute(method) != default;
    }
}