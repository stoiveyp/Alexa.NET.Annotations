using Alexa.NET.RequestHandlers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Alexa.NET.Annotations;

internal static class InterceptorFactory
{
    private static readonly BaseTypeSyntax RequestInterceptorBaseType = SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName(nameof(IAlexaRequestInterceptor)));

    public static ClassDeclarationSyntax? ToInterceptor(this MethodDeclarationSyntax method,
        string requestType, AttributeSyntax marker, ClassDeclarationSyntax containerClass, Action<Diagnostic> reportDiagnostic)
    {
        if (marker == null) throw new ArgumentNullException(nameof(marker));
        var info = InterceptorMarkerInfo.Info[marker.MarkerName()!];

        var returnsVoid = method.ReturnsVoid();

        if (returnsVoid || method.IsTask())
        {
            return ReturnClass(containerClass, method, requestType, info, reportDiagnostic);
        }

        if (HandlerFactory.AssertReturnType(method))
        {
            return ReturnClass(containerClass, method, requestType, info, reportDiagnostic);
        }

        reportDiagnostic(Diagnostic.Create(Rules.InvalidInterceptorReturnTypeRule, method.GetLocation(), method.Identifier.Text));
        return null;
    }

    private static ClassDeclarationSyntax ReturnClass(ClassDeclarationSyntax containerClass, MethodDeclarationSyntax method,
        string requestType, InterceptorMarkerInfo info, System.Action<Diagnostic> reportDiagnostic)
    {
        return method.GenerateInterceptorClass(containerClass, RequestInterceptorBaseType, null)
            .AddExecuteMethod(method, requestType, info, reportDiagnostic);
    }

    private static ClassDeclarationSyntax AddExecuteMethod(this ClassDeclarationSyntax skillClass,
        MethodDeclarationSyntax method, string requestType, InterceptorMarkerInfo info, Action<Diagnostic> reportDiagnostic)
    {
        var returnType = InnerClassHelper.SkillResponseTask();

        var tokens = SF.TokenList(SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.OverrideKeyword));
        if (method.ReturnsTask() || info.CanAccessResponse)
        {
            tokens = tokens.Add(SF.Token(SyntaxKind.AsyncKeyword));
        }

        var newMethod = SF.MethodDeclaration(returnType, Strings.HandlerMethodName)
            .WithModifiers(tokens)
            .WithParameterList(SF.ParameterList(SF.SeparatedList(
                new[]{
                    SF.Parameter(SF.Identifier(Strings.Names.HandlerInformationProperty)).WithType(InnerClassHelper.TypedSkillInformation(requestType)),
                    SF.Parameter(SF.Identifier(Strings.Names.NextCallProperty)).WithType(InnerClassHelper.NextCall(requestType))
                    }
            )));

        var argumentMapping = method.FromInterceptorParameters(info, reportDiagnostic);

        var statements = argumentMapping.CommonStatements
            .Concat(argumentMapping.Arguments.SelectMany(a => a.ArgumentSetup)).ToList();

        ExpressionSyntax nextExpression = SF.InvocationExpression(SF.IdentifierName(Strings.Names.NextCallProperty))
            .WithArgumentList(SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(SF.IdentifierName(Strings.Names.HandlerInformationProperty)))));

        ExpressionSyntax wrapperExpression = InnerClassHelper.RunWrapper(method, argumentMapping);

        if (method.ReturnsTask())
        {
            wrapperExpression = SF.AwaitExpression(wrapperExpression);
        }

        if (method.ReturnsTask() || info.CanAccessResponse)
        {
            nextExpression = SF.AwaitExpression(nextExpression);
        }

        StatementSyntax? ifStatement = null;
        StatementSyntax? finalWrapper = null;
        if (method.ReturnsSkillResponse())
        {
            var interceptorVar = SF.VariableDeclaration(SF.IdentifierName(Strings.Types.Var))
                .WithVariables(SF.SingletonSeparatedList(SF.VariableDeclarator(SF.Identifier(Strings.Names.InterceptorResponse)).WithInitializer(SF.EqualsValueClause(wrapperExpression))));
            finalWrapper = SF.LocalDeclarationStatement(interceptorVar);
            var condition = SF.BinaryExpression(SyntaxKind.NotEqualsExpression,
                SF.IdentifierName(Strings.Names.InterceptorResponse),
                SF.LiteralExpression(SyntaxKind.NullLiteralExpression));
            ifStatement = SF.IfStatement(condition,
                SF.Block(SF.ReturnStatement(SF.IdentifierName(Strings.Names.InterceptorResponse))));
        }
        else
        {
            finalWrapper = SF.ExpressionStatement(wrapperExpression);
        }



        if (info.CanAccessResponse)
        {
            var initialResponse = SF.VariableDeclaration(SF.IdentifierName(Strings.Types.Var))
                .WithVariables(SF.SingletonSeparatedList(
                    SF.VariableDeclarator(SF.Identifier(Strings.Names.Response))
                        .WithInitializer(SF.EqualsValueClause(nextExpression))
                    ));
            statements.Add(SF.LocalDeclarationStatement(initialResponse));
            statements.Add(finalWrapper);
            if (ifStatement != null)
            {
                statements.Add(ifStatement);
            }
            //IF handler returns SkillResponse - return interceptor, otherwise return response
            statements.Add(SF.ReturnStatement(SF.IdentifierName(Strings.Names.Response)));
        }
        else
        {
            statements.Add(finalWrapper);
            if (ifStatement != null)
            {
                statements.Add(ifStatement);
            }
            statements.Add(SF.ReturnStatement(nextExpression));
        }

        newMethod = newMethod.WithBody(SF.Block(statements));

        return skillClass.AddMembers(newMethod);
    }
}