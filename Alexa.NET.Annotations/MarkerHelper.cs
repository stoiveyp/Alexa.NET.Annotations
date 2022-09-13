using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

    internal static AttributeSyntax? HandlerAttribute(this MethodDeclarationSyntax method)
    {
        return method.AttributeLists.SelectMany(a => a.Attributes)
            .FirstOrDefault(a => HandlerMarkerInfo.Info.Keys.Contains(a.MarkerName()));
    }

    internal static AttributeSyntax? InterceptorAttribute(this MethodDeclarationSyntax method)
    {
        return method.AttributeLists.SelectMany(a => a.Attributes)
            .FirstOrDefault(a => InterceptorMarkerInfo.Info.Keys.Contains(a.MarkerName()));
    }

    internal static bool HasHandlerAttribute(this MethodDeclarationSyntax method) => HandlerAttribute(method) != default;
    internal static bool HasInterceptorAttribute(this MethodDeclarationSyntax method) => InterceptorAttribute(method) != default;
}