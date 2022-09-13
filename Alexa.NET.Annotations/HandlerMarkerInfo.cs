using Alexa.NET.Annotations.Markers;
using Alexa.NET.Request.Type;
using Alexa.NET.RequestHandlers.Handlers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Alexa.NET.Annotations;

public class HandlerMarkerInfo
{
    public HandlerMarkerInfo(string baseClassName, string requestType, Func<AttributeSyntax, ConstructorInitializerSyntax>? constructor = null)
    {
        BaseType = SF.Identifier(baseClassName);
        RequestType = SF.IdentifierName(requestType);
        Constructor = constructor;
    }

    public SyntaxToken BaseType { get; }
    public IdentifierNameSyntax RequestType { get; }
    public Func<AttributeSyntax, ConstructorInitializerSyntax>? Constructor { get; }

    public static readonly Dictionary<string, HandlerMarkerInfo> Info = new()
    {
        { nameof(LaunchAttribute).NameOnly(), new(nameof(LaunchRequestHandler), nameof(LaunchRequest)) },
        { nameof(IntentAttribute).NameOnly(), new(nameof(IntentNameRequestHandler), nameof(IntentRequest), MarkerHelper.IntentConstructor) }
    };

    public BaseTypeSyntax GenericBase(string requestType)
    {
        if (requestType is Strings.Types.SkillRequest or Strings.Types.FullSkillRequest)
        {
            return SF.SimpleBaseType(SF.IdentifierName(BaseType));
        }

        return SF.SimpleBaseType(SF.GenericName(BaseType).WithTypeArgumentList(
            SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(requestType)))));
    }
}