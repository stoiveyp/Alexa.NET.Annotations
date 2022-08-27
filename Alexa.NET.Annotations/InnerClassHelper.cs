using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Alexa.NET.Annotations
{
    internal static class InnerClassHelper
    {
        internal static InvocationExpressionSyntax RunWrapper(MethodDeclarationSyntax method, ParameterPrep prep)
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
                    SF.IdentifierName(Strings.WrapperPropertyName),
                    SF.IdentifierName(method.Identifier.Text)),
                SF.ArgumentList(arguments));
        }

        internal static ClassDeclarationSyntax GenerateWrapperClass(this MethodDeclarationSyntax method,
            ClassDeclarationSyntax containerClass, BaseTypeSyntax baseType,
            ConstructorInitializerSyntax? constructorInitializer) => SF
            .ClassDeclaration(method.Identifier.Text + Strings.HandlerSuffix)
            .WithBaseList(SF.BaseList(SF.SingletonSeparatedList(baseType)))
            .WithModifiers(SF.TokenList(
                SF.Token(SyntaxKind.PrivateKeyword)))
            .AddWrapperField(containerClass)
            .AddWrapperConstructor(containerClass, constructorInitializer);

        private static ClassDeclarationSyntax AddWrapperConstructor(this ClassDeclarationSyntax skillClass, ClassDeclarationSyntax wrapperClass, ConstructorInitializerSyntax? initializer)
        {
            var handlerParameter = SF.Parameter(SF.Identifier(Strings.WrapperVarName))
                .WithType(SF.IdentifierName(wrapperClass.Identifier.Text));

            var constructor = SF.ConstructorDeclaration(skillClass.Identifier.Text)
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.InternalKeyword)))
                .WithParameterList(SF.ParameterList(SF.SingletonSeparatedList(handlerParameter)))
                .WithBody(SF.Block(SF.ExpressionStatement(SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    SF.IdentifierName(Strings.WrapperPropertyName), SF.IdentifierName(Strings.WrapperVarName)))));

            if (initializer != null)
            {
                constructor = constructor.WithInitializer(initializer);
            }

            return skillClass.AddMembers(constructor);
        }

        private static ClassDeclarationSyntax AddWrapperField(this ClassDeclarationSyntax skillClass,
            ClassDeclarationSyntax wrapperClass)
        {
            var handlerField = SF
                .PropertyDeclaration(SF.IdentifierName(wrapperClass.Identifier.Text), Strings.WrapperPropertyName)
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PrivateKeyword)))
                .WithAccessorList(SF.AccessorList(SF.SingletonList(SF.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken)))));
            return skillClass.AddMembers(handlerField);
        }

        internal static bool ReturnsVoid(this MethodDeclarationSyntax method) => method.ReturnType is PredefinedTypeSyntax pre && pre.Keyword.IsKind(SyntaxKind.VoidKeyword);

        public static TypeSyntax SkillResponseTask() => SF.GenericName(Strings.Types.Task).WithTypeArgumentList(
            SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(Strings.Types.SkillResponse))));

        public static TypeSyntax TypedSkillInformation() => SF.GenericName(
            SF.Identifier(Strings.Types.HandlerInformation),
            SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(Strings.Types.SkillRequest))));

        public static TypeSyntax NextCall() => SF.GenericName(
            SF.Identifier(Strings.Types.NextDelegate),
            SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(Strings.Types.SkillRequest))));
    }
}
