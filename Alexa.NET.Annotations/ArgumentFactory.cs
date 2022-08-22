using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Alexa.NET.Annotations;

internal static class ArgumentFactory
{
    public static ParameterPrep FromParameters(this MethodDeclarationSyntax method, MarkerInfo info,
        Action<Diagnostic> reportDiagnostic)
    {
        var parameters = method.ParameterList.Parameters.ToArray();
        var parameterPrep = new ParameterPrep();

        if (!parameters.Any())
        {
            return parameterPrep;
        }

        parameterPrep.Arguments.AddRange(parameters.Select(p => p.ToArgumentDetail(method.Identifier.Text,info, parameters.Length == 1, reportDiagnostic)).Where(ad => ad != null)!);

        if (parameterPrep.RequiresRequest)
        {
            var requestTypeAssignment = SF.VariableDeclaration(SF.IdentifierName(Strings.Types.Var)).WithVariables(SF.SeparatedList(new[] {
                    SF.VariableDeclarator(SF.Identifier(Strings.Names.TypedRequestObject)).WithInitializer(SF.EqualsValueClause(TypedRequest(info)))
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

    private static string? TypeName(this ParameterSyntax parameter) => parameter.Type switch
    {
        IdentifierNameSyntax id => id.Identifier.Text,
        PredefinedTypeSyntax predef => predef.Keyword.Text,
        GenericNameSyntax generic => generic.Identifier.Text,
        _ => parameter.Type?.ToString()
    };

    private static ElementAccessExpressionSyntax AccessSlot(IdentifierNameSyntax localParam)
    {
        var accessIntent = SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
            SF.IdentifierName(Strings.Names.TypedRequestObject),
            SF.IdentifierName(Strings.Names.IntentProperty));

        var slotsProperty = SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
            accessIntent, SF.IdentifierName(Strings.Names.SlotsProperty));

        return SF.ElementAccessExpression(slotsProperty)
            .WithArgumentList(SF.BracketedArgumentList(SF.SingletonSeparatedList(
                SF.Argument(SF.LiteralExpression(SyntaxKind.StringLiteralExpression,
                    SF.Literal(localParam.Identifier.Text))))));
    }

    private static bool IntentArgumentParsing(this ParameterSyntax syntax, out ArgumentDetail? argumentDetail)
    {
        var localParam = SF.IdentifierName(syntax.Identifier.Text);
        var typeName = syntax.TypeName();
        if (typeName == Strings.Types.String)
        {
            argumentDetail = new ArgumentDetail(localParam)
            {
                RequiresRequest = true
            };

            var slotValue = SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, AccessSlot(localParam),
                SF.IdentifierName(Strings.Names.SlotValueProperty));

            var value = SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                slotValue, SF.IdentifierName(Strings.Names.SlotValueValueProperty));

            argumentDetail.ArgumentSetup.Add(SF.LocalDeclarationStatement(
                SF.VariableDeclaration(SF.IdentifierName(Strings.Types.Var),
                    SF.SingletonSeparatedList(SF.VariableDeclarator(localParam.Identifier.Text).WithInitializer(SF.EqualsValueClause(value))))));
            return true;
        }

        if (typeName is Strings.Types.Slot or Strings.Types.FullSlot)
        {
            argumentDetail = new ArgumentDetail(localParam)
            {
                RequiresRequest = true
            };

            argumentDetail.ArgumentSetup.Add(SF.LocalDeclarationStatement(
                SF.VariableDeclaration(SF.IdentifierName(Strings.Types.Var),
                    SF.SingletonSeparatedList(SF.VariableDeclarator(localParam.Identifier.Text).WithInitializer(SF.EqualsValueClause(AccessSlot(localParam)))))));
            return true;
        }

        argumentDetail = null;
        return false;
    }

    private static ArgumentDetail? ToArgumentDetail(this ParameterSyntax syntax, string methodName, MarkerInfo info, bool singleParam,
        Action<Diagnostic> reportDiagnostic)
    {
        var typeName = syntax.TypeName();


        if (typeName == info.RequestType.Identifier.Text)
        {
            return new ArgumentDetail(singleParam ? TypedRequest(info) : SF.IdentifierName(Strings.Names.TypedRequestObject)){RequiresRequest = !singleParam};
        }

        if (typeName == Strings.Types.AlexaRequestInformation)
        {
            return new ArgumentDetail(SF.IdentifierName(Strings.Names.HandlerInformationProperty));
        }

        if (info.RequestType.Identifier.Text == Strings.Types.IntentRequest)
        {
            if (syntax.IntentArgumentParsing(out var intentDetail))
            {
                return intentDetail;
            }
        }

        reportDiagnostic(Diagnostic.Create(Rules.InvalidParameterRule,syntax.GetLocation(),syntax.TypeName(),methodName));
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