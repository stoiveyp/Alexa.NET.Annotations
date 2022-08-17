using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Alexa.NET.Annotations;

internal static class ArgumentFactory
{
    private const string HandlerInformationName = "information";

    public static ParameterPrep FromParameters(ParameterSyntax[] parameters, MarkerInfo info)
    {
        var parameterPrep = new ParameterPrep();

        if (!parameters.Any())
        {
            return parameterPrep;
        }

        parameterPrep.Arguments.AddRange(parameters.Select(p => p.ToArgumentDetail(info, parameters.Length > 1)).Where(ad => ad != null));

        if (parameterPrep.RequiresRequest)
        {
            parameterPrep.CommonStatements.Add(SF.ExpressionStatement(TypedRequest(info)));
        }

        return parameterPrep;
    }

    private static CastExpressionSyntax TypedRequest(MarkerInfo info) => SF.CastExpression(info.RequestType,
        SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
            SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                SF.IdentifierName(HandlerInformationName),
                SF.IdentifierName("SkillRequest")),
            SF.IdentifierName("Request")
        ));

    private static string TypeName(this ParameterSyntax parameter) => ((IdentifierNameSyntax)parameter.Type!).Identifier.Text;

    private static ArgumentDetail? ToArgumentDetail(this ParameterSyntax syntax, MarkerInfo info, bool singleParam)
    {
        if (syntax.TypeName() == info.RequestType.Identifier.Text)
        {
            return new ArgumentDetail { Expression = SF.IdentifierName() };
        }

        return null;
    }
}

internal class ParameterPrep
{
    public List<ArgumentDetail> Arguments = new();
    public List<StatementSyntax> CommonStatements = new();
    public bool RequiresRequest => Arguments.Any(a => a.RequiresRequest);
    public bool InlineOnly => !CommonStatements.Any() && Arguments.All(a => a.IsInline);
}

internal class ArgumentDetail
{
    public List<StatementSyntax> ArgumentSetup = new();
    public ExpressionSyntax Expression { get; set; }

    public bool RequiresRequest { get; set; }
    public bool IsInline => ArgumentSetup.Count == 0;
}