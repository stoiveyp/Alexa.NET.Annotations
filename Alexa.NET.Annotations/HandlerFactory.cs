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
            return SF.ClassDeclaration(method.Identifier.Text + Strings.HandlerSuffix)
                .WithBaseList(SF.BaseList(SF.SingletonSeparatedList(info.BaseType(marker))))
                .WithModifiers(SF.TokenList(
                    SF.Token(SyntaxKind.PrivateKeyword)))
                .AddWrapperField(containerClass)
                .AddWrapperConstructor(containerClass, info.Constructor?.Invoke(marker))
                .AddExecuteMethod(method, info);
        }

        public static ClassDeclarationSyntax AddWrapperConstructor(this ClassDeclarationSyntax skillClass, ClassDeclarationSyntax wrapperClass, ConstructorInitializerSyntax? initializer)
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

        public static ClassDeclarationSyntax AddWrapperField(this ClassDeclarationSyntax skillClass,
            ClassDeclarationSyntax wrapperClass)
        {
            var handlerField = SF
                .PropertyDeclaration(SF.IdentifierName(wrapperClass.Identifier.Text), Strings.WrapperPropertyName)
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PrivateKeyword)))
                .WithAccessorList(SF.AccessorList(SF.SingletonList(SF.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken)))));
            return skillClass.AddMembers(handlerField);
        }

        public static ClassDeclarationSyntax AddExecuteMethod(this ClassDeclarationSyntax skillClass, MethodDeclarationSyntax method, MarkerInfo info)
        {
            var returnType = method.ReturnsTask()
                ? method.ReturnType
                : SF.GenericName(Strings.TypeTask).WithTypeArgumentList(
                    SF.TypeArgumentList(SF.SingletonSeparatedList(method.ReturnType)));

            var newMethod = SF.MethodDeclaration(returnType, Strings.HandlerMethodName)
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.OverrideKeyword)))
                .WithParameterList(SF.ParameterList(SF.SingletonSeparatedList(
                    SF.Parameter(SF.Identifier(Strings.HandlerInformationPropertyName)).WithType(
                        SF.GenericName(SF.Identifier(Strings.TypeHandlerInformation),
                            SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(Strings.Types.SkillRequest)))))
                )));

            var argumentMapping = ArgumentFactory.FromParameters(method.ParameterList.Parameters.ToArray(), info);

            var wrapperExpression = RunWrapper(method, argumentMapping);

            if (argumentMapping.InlineOnly)
            {
                newMethod = newMethod.WithExpressionBody(SF.ArrowExpressionClause(wrapperExpression.WrapIfNotAsync(method))).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken));
            }
            else
            {
                newMethod = newMethod.WithBody(SF.Block(
                    argumentMapping.CommonStatements
                    .Concat(argumentMapping.Arguments.SelectMany(a => a.ArgumentSetup)
                        .Concat(new []{SF.ReturnStatement(wrapperExpression)}))));
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
                    SF.IdentifierName(Strings.WrapperPropertyName),
                    SF.IdentifierName(method.Identifier.Text)),
                SF.ArgumentList(arguments));
        }
    }
}
