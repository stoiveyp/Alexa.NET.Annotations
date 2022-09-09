using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Alexa.NET.Annotations;

internal static class ArgumentFactory
{
    public static ParameterPrep FromHandlerParameters(this MethodDeclarationSyntax method, string requestType, HandlerMarkerInfo info,
        Action<Diagnostic> reportDiagnostic)
    {
        var parameters = method.ParameterList.Parameters.ToArray();
        var parameterPrep = new ParameterPrep();

        if (!parameters.Any())
        {
            return parameterPrep;
        }

        parameterPrep.Arguments.AddRange(parameters.Select(p => p.ToHandlerArgumentDetail(method.Identifier.Text,requestType, info, parameters.Length == 1, reportDiagnostic)).Where(ad => ad != null)!);

        if (parameterPrep.RequiresRequest)
        {
            var requestTypeAssignment = SF.VariableDeclaration(SF.IdentifierName(Strings.Types.Var)).WithVariables(SF.SeparatedList(new[] {
                    SF.VariableDeclarator(SF.Identifier(Strings.Names.TypedRequestObject)).WithInitializer(SF.EqualsValueClause(TypedRequest(info.RequestType)))
                }));
            parameterPrep.CommonStatements.Add(SF.LocalDeclarationStatement(requestTypeAssignment));
        }
        return parameterPrep;
    }

    public static ParameterPrep FromInterceptorParameters(this MethodDeclarationSyntax method, InterceptorMarkerInfo info,
        Action<Diagnostic> reportDiagnostic)
    {
        var parameters = method.ParameterList.Parameters.ToArray();
        var parameterPrep = new ParameterPrep();

        if (!parameters.Any())
        {
            return parameterPrep;
        }

        parameterPrep.Arguments.AddRange(parameters.Select(p => p.ToInterceptorArgumentDetail(method.Identifier.Text, info,  reportDiagnostic)).Where(ad => ad != null)!);

        return parameterPrep;
    }

    private static CastExpressionSyntax TypedRequest(IdentifierNameSyntax requestType) => SF.CastExpression(requestType,
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

    private static ArgumentDetail? ToInterceptorArgumentDetail(this ParameterSyntax syntax, string methodName,
        InterceptorMarkerInfo info, Action<Diagnostic> reportDiagnostic)
    {
        var typeName = syntax.TypeName();

        if (typeName == Strings.Types.AlexaRequestInformation)
        {
            return new ArgumentDetail(SF.IdentifierName(Strings.Names.HandlerInformationProperty));
        }

        if (typeName == Strings.Types.NextDelegate)
        {
            return new ArgumentDetail(SF.IdentifierName(Strings.Names.NextCallProperty));
        }

        if (info.CanAccessResponse && typeName is Strings.Types.SkillResponse or Strings.Types.FullSkillResponse)
        {
            return new ArgumentDetail(SF.IdentifierName(Strings.Names.Response));
        }

        reportDiagnostic(Diagnostic.Create(Rules.InvalidParameterRule, syntax.GetLocation(), syntax.TypeName(), methodName));
        return null;
    }

    private static ArgumentDetail? ToHandlerArgumentDetail(this ParameterSyntax syntax, string methodName, string requestType, HandlerMarkerInfo info, bool singleParam,
        Action<Diagnostic> reportDiagnostic)
    {
        var typeName = syntax.TypeName();


        if (typeName == info.RequestType.Identifier.Text)
        {
            return new ArgumentDetail(singleParam ? TypedRequest(info.RequestType) : SF.IdentifierName(Strings.Names.TypedRequestObject)){RequiresRequest = !singleParam};
        }

        if (typeName == Strings.Types.AlexaRequestInformation)
        {
            return new ArgumentDetail(SF.IdentifierName(Strings.Names.HandlerInformationProperty));
        }

        if (typeName is Strings.Types.SkillRequest or Strings.Types.FullSkillRequest || typeName == requestType)
        {
            return new ArgumentDetail(SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                SF.IdentifierName(Strings.HandlerInformationName),
                SF.IdentifierName(Strings.Types.SkillRequest)));
        }

        if (info.RequestType.Identifier.Text == Strings.Types.IntentRequest)
        {
            if (syntax.IntentArgumentParsing(out var intentDetail))
            {
                return intentDetail;
            }
        }

        reportDiagnostic(Diagnostic.Create(Rules.InvalidParameterRule,syntax.GetLocation(),syntax.TypeName(),methodName));
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