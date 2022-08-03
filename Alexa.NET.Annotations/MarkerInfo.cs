using Alexa.NET.Annotations.Markers;
using Alexa.NET.Request.Type;
using Alexa.NET.RequestHandlers.Handlers;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Alexa.NET.Annotations;

public class MarkerInfo
{
    public MarkerInfo(string baseClassName, string requestType, Func<AttributeSyntax, ConstructorInitializerSyntax>? constructor = null)
    {
        BaseType = _ => SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName(baseClassName));
        RequestType = SyntaxFactory.IdentifierName(requestType);
        Constructor = constructor;
    }

    public Func<AttributeSyntax,BaseTypeSyntax> BaseType { get; }
    public IdentifierNameSyntax RequestType { get; }
    public Func<AttributeSyntax, ConstructorInitializerSyntax>? Constructor { get; }

    public static readonly string[] List =
    {
        nameof(LaunchAttribute).NameOnly(),
        nameof(IntentAttribute).NameOnly()
    };

    public static Dictionary<string, MarkerInfo> MarkerTypeInfo = new()
    {
        { nameof(LaunchAttribute).NameOnly(), new(nameof(LaunchRequestHandler), nameof(LaunchRequest)) },
        { nameof(IntentAttribute).NameOnly(), new(nameof(IntentNameRequestHandler), nameof(IntentRequest), MarkerHelper.IntentConstructor) }
    };
}