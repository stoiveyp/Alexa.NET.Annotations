using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Alexa.NET.Annotations;

internal static class ArgumentFactory
{
    public static ParameterPrep FromParameters(ParameterSyntax[] parameters, MarkerInfo info)
    {
        var parameterPrep = new ParameterPrep();

        if (!parameters.Any())
        {
            return parameterPrep;
        }

        parameterPrep.Arguments.AddRange(parameters.Select(p => p.ToArgumentDetail(info, parameters.Length > 1)).Where(ad => ad != null)!);

        if (parameterPrep.RequiresRequest)
        {
            var requestTypeAssignment = SF.VariableDeclaration(SF.IdentifierName(SF.Token(SyntaxKind.VarKeyword))).WithVariables(SF.SeparatedList(new[] {
                    SF.VariableDeclarator(SF.Identifier(Strings.TypedRequestObjectIdentifier)).WithInitializer(SF.EqualsValueClause(TypedRequest(info)))
                }));
            parameterPrep.CommonStatements.Add(SF.LocalDeclarationStatement(requestTypeAssignment));
        }
        return parameterPrep;
    }

    private static CastExpressionSyntax TypedRequest(MarkerInfo info) => SF.CastExpression(info.RequestType,
        SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
            SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                SF.IdentifierName(Strings.HandlerInformationName),
                SF.IdentifierName(Strings.Types.SkillRequest)),
            SF.IdentifierName(Strings.RequestProperty)
        ));

    private static string TypeName(this ParameterSyntax parameter) => ((IdentifierNameSyntax)parameter.Type!).Identifier.Text;

    private static ArgumentDetail? ToArgumentDetail(this ParameterSyntax syntax, MarkerInfo info, bool singleParam)
    {
        if (syntax.TypeName() == info.RequestType.Identifier.Text)
        {
            return new ArgumentDetail(singleParam ? TypedRequest(info) : SF.IdentifierName(Strings.TypedRequestObjectIdentifier));
        }

        //TODO: Add Analyzer warning - unable to map type for method.
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
    public ArgumentDetail(ExpressionSyntax expression)
    {
        Expression = expression;
    }

    public ArgumentDetail(ExpressionSyntax expression, params StatementSyntax[] setup) : this(expression)
    {
        ArgumentSetup.AddRange(setup);
    }

    public List<StatementSyntax> ArgumentSetup = new();
    public ExpressionSyntax Expression { get; set; }

    public bool RequiresRequest { get; set; }
    public bool IsInline => ArgumentSetup.Count == 0;
}