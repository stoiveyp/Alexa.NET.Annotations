using Alexa.NET.RequestHandlers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Alexa.NET.Annotations;

internal static class InterceptorFactory
{
    private static readonly BaseTypeSyntax RequestInterceptorBaseType = SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName(nameof(IAlexaRequestInterceptor)));

    public static ClassDeclarationSyntax? ToInterceptor(this MethodDeclarationSyntax method,
        AttributeSyntax marker, ClassDeclarationSyntax containerClass, Action<Diagnostic> reportDiagnostic)
    {
        if (marker == null) throw new ArgumentNullException(nameof(marker));
        var info = InterceptorMarkerInfo.Info[marker.MarkerName()!];

        var returnsVoid = method.ReturnsVoid();

        if (method.ReturnsVoid() || method.IsTask())
        {
            return ReturnClass(containerClass, method, info, reportDiagnostic);
        }

        if (HandlerFactory.AssertReturnType(method))
        {
            return ReturnClass(containerClass, method, info, reportDiagnostic);
        }

        reportDiagnostic(Diagnostic.Create(Rules.InvalidInterceptorReturnTypeRule, method.GetLocation(), method.Identifier.Text));
        return null;
    }

    private static ClassDeclarationSyntax ReturnClass(ClassDeclarationSyntax containerClass, MethodDeclarationSyntax method,
        InterceptorMarkerInfo info, System.Action<Diagnostic> reportDiagnostic)
    {
        return method.GenerateWrapperClass(containerClass, RequestInterceptorBaseType, null)
            .AddExecuteMethod(method, info, reportDiagnostic);
    }

    private static ClassDeclarationSyntax AddExecuteMethod(this ClassDeclarationSyntax skillClass,
        MethodDeclarationSyntax method, InterceptorMarkerInfo info, Action<Diagnostic> reportDiagnostic)
    {
        var returnType = InnerClassHelper.SkillResponseTask();

        var newMethod = SF.MethodDeclaration(returnType, Strings.HandlerMethodName)
            .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.OverrideKeyword)))
            .WithParameterList(SF.ParameterList(SF.SeparatedList(
                new []{
                    SF.Parameter(SF.Identifier(Strings.Names.HandlerInformationProperty)).WithType(InnerClassHelper.TypedSkillInformation()),
                    SF.Parameter(SF.Identifier(Strings.Names.NextCallProperty)).WithType(InnerClassHelper.NextCall())
                    }
            )));

        var argumentMapping = method.FromInterceptorParameters(info, reportDiagnostic);

        var wrapperExpression = InnerClassHelper.RunWrapper(method, argumentMapping).WrapIfNotAsync(method);

        if (argumentMapping.InlineOnly)
        {
            newMethod = newMethod.WithExpressionBody(SF.ArrowExpressionClause(wrapperExpression.WrapIfNotAsync(method))).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken));
        }
        else
        {
            newMethod = newMethod.WithBody(SF.Block(
                argumentMapping.CommonStatements
                    .Concat(argumentMapping.Arguments.SelectMany(a => a.ArgumentSetup)
                        .Concat(new[] { SF.ReturnStatement(wrapperExpression) }))));
        }

        return skillClass.AddMembers(newMethod);
    }
}