using Alexa.NET.Annotations.Markers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Alexa.NET.Annotations;

public class InterceptorMarkerInfo
{
    public static Dictionary<string, InterceptorMarkerInfo> Info = new()
    {
        { nameof(BeforeExecutionAttribute).NameOnly(), new(false) },
        { nameof(AfterExecutionAttribute).NameOnly(), new(true) }
    };

    public InterceptorMarkerInfo(bool canAccessResponse)
    {
        CanAccessResponse = canAccessResponse;
    }

    public bool CanAccessResponse { get; }
}