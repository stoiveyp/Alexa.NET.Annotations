using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Alexa.NET.Annotations
{
    internal static class HandlerFactory
    {
        public static ClassDeclarationSyntax ToPipelineHandler(this MethodDeclarationSyntax method, AttributeSyntax marker, ClassDeclarationSyntax containerClass)
        {
            if (marker == null) throw new ArgumentNullException(nameof(marker));
            var info = MarkerInfo.MarkerTypeInfo[marker.MarkerName()!];
            return SF.ClassDeclaration(method.Identifier.Text + "Handler")
                .WithBaseList(SF.BaseList(SF.SingletonSeparatedList(info.BaseType(marker))))
                .WithModifiers(SF.TokenList(
                    SF.Token(SyntaxKind.PrivateKeyword)))
                .AddWrapperField(containerClass)
                .AddWrapperConstructor(containerClass, info.Constructor?.Invoke(marker))
                .AddExecuteMethod(method, info);
        }

        public static ClassDeclarationSyntax AddWrapperConstructor(this ClassDeclarationSyntax skillClass, ClassDeclarationSyntax wrapperClass, ConstructorInitializerSyntax? initializer)
        {
            var handlerParameter = SF.Parameter(SF.Identifier("wrapper"))
                .WithType(SF.IdentifierName(wrapperClass.Identifier.Text));

            var constructor = SF.ConstructorDeclaration(skillClass.Identifier.Text)
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.InternalKeyword)))
                .WithParameterList(SF.ParameterList(SF.SingletonSeparatedList(handlerParameter)))
                .WithBody(SF.Block(SF.ExpressionStatement(SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    SF.IdentifierName(WrapperPropertyName), SF.IdentifierName("wrapper")))));

            if (initializer != null)
            {
                constructor = constructor.WithInitializer(initializer);
            }

            return skillClass.AddMembers(constructor);
        }

        public static ClassDeclarationSyntax AddWrapperField(this ClassDeclarationSyntax skillClass,
            ClassDeclarationSyntax wrapperClass)
        {
            var handlerField = SF
                .PropertyDeclaration(SF.IdentifierName(wrapperClass.Identifier.Text), WrapperPropertyName)
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PrivateKeyword)))
                .WithAccessorList(SF.AccessorList(SF.SingletonList(SF.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken)))));
            return skillClass.AddMembers(handlerField);
        }

        public static ClassDeclarationSyntax AddExecuteMethod(this ClassDeclarationSyntax skillClass, MethodDeclarationSyntax method, MarkerInfo info)
        {
            var returnType = method.ReturnsTask()
                ? method.ReturnType
                : SF.GenericName("Task").WithTypeArgumentList(
                    SF.TypeArgumentList(SF.SingletonSeparatedList(method.ReturnType)));

            var argumentMapping = ArgumentFactory.FromParameters(method.ParameterList.Parameters.ToArray(), info);

            var newMethod = SF.MethodDeclaration(returnType, HandlerMethodName)
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.OverrideKeyword)))
                .WithParameterList(SF.ParameterList(SF.SingletonSeparatedList(
                    SF.Parameter(SF.Identifier("information")).WithType(
                        SF.GenericName(SF.Identifier("AlexaRequestInformation"),
                            SF.TypeArgumentList(SF.SingletonSeparatedList(SF.ParseTypeName("SkillRequest")))))
                )));

            if (argumentMapping.InlineOnly)
            {
                newMethod = newMethod.WithExpressionBody(SF.ArrowExpressionClause(RunWrapper(method, argumentMapping).WrapIfAsync(method))).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken));
            }

            return skillClass.AddMembers(newMethod);
        }

        private static InvocationExpressionSyntax RunWrapper(MethodDeclarationSyntax method, ParameterPrep prep)
        {
            SeparatedSyntaxList<ArgumentSyntax> arguments = SF.SeparatedList<ArgumentSyntax>();

            if (prep.Arguments.Count == 1)
            {
                arguments = SF.SingletonSeparatedList(SF.Argument(prep.Arguments[0].Expression));
            }
            else
            {
                arguments = SF.SeparatedList(prep.Arguments.Select(a => SF.Argument(a.Expression)));
            }

            return SF.InvocationExpression(
                SF.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SF.IdentifierName(WrapperPropertyName),
                    SF.IdentifierName(method.Identifier.Text)),
                SF.ArgumentList(arguments));
        }

        private const string WrapperPropertyName = "Wrapper";
        private const string HandlerMethodName = "Handle";
    }
}
