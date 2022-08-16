using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Alexa.NET.Annotations;

internal static class ArgumentFactory
{
    public static ParameterPrep FromParameters(ParameterSyntax[] parameters, MarkerInfo info)
    {
        var parameterPrep = new ParameterPrep();
        parameterPrep.Arguments.Add(new ArgumentDetail
        {
            Expression = SF.CastExpression(info.RequestType,
                SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName("information"),
                        SF.IdentifierName("SkillRequest")),
                    SF.IdentifierName("Request")
                ))
        });
        return parameterPrep;
    }
}

internal class ParameterPrep
{
    public List<ArgumentDetail> Arguments = new();
    public List<StatementSyntax> CommonStatements = new();

    public bool InlineOnly => CommonStatements.Count == 0 && Arguments.All(a => a.IsInline);
}

internal class ArgumentDetail
{
    public List<StatementSyntax> ArgumentSetup = new();
    public ExpressionSyntax Expression { get; set; }

    public bool IsInline => ArgumentSetup.Count == 0;
}