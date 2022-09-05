using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Alexa.NET.Annotations
{
    internal static class HandlerFactory
    {
        public static ClassDeclarationSyntax? ToHandler(this MethodDeclarationSyntax method, AttributeSyntax marker, ClassDeclarationSyntax containerClass, Action<Diagnostic> reportDiagnostic)
        {
            if (!AssertReturnType(method, reportDiagnostic))
            {
                return null;
            }

            if (marker == null) throw new ArgumentNullException(nameof(marker));
            var info = HandlerMarkerInfo.Info[marker.MarkerName()!];
            return method.GenerateHandlerClass(containerClass, info.BaseType, info.Constructor?.Invoke(marker))
                .AddExecuteMethod(method, info, reportDiagnostic);
        }

        internal static bool AssertReturnType(MethodDeclarationSyntax method, Action<Diagnostic>? reportDiagnostic = null)
        {
            if(method.ReturnsSkillResponse())
            {
                return true;
            }

            reportDiagnostic?.Invoke(Diagnostic.Create(Rules.InvalidHandlerReturnTypeRule, method.GetLocation(),
                method.Identifier.Text));

            return false;
        }

        private static ClassDeclarationSyntax AddExecuteMethod(this ClassDeclarationSyntax skillClass,
            MethodDeclarationSyntax method, HandlerMarkerInfo info, Action<Diagnostic> reportDiagnostic)
        {
            var returnType = SF.GenericName(Strings.Types.Task).WithTypeArgumentList(
                    SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(Strings.Types.SkillResponse))));

            var newMethod = SF.MethodDeclaration(returnType, Strings.HandlerMethodName)
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.OverrideKeyword)))
                .WithParameterList(SF.ParameterList(SF.SingletonSeparatedList(
                    SF.Parameter(SF.Identifier(Strings.Names.HandlerInformationProperty)).WithType(InnerClassHelper.TypedSkillInformation())
                )));

            var argumentMapping = method.FromHandlerParameters(info, reportDiagnostic);

            var wrapperExpression = InnerClassHelper.RunWrapper(method, argumentMapping).WrapIfNotAsync(method);

            if (argumentMapping.InlineOnly)
            {
                newMethod = newMethod.WithExpressionBody(SF.ArrowExpressionClause(wrapperExpression)).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken));
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
}
