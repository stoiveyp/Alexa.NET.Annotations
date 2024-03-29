﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Alexa.NET.Annotations
{
    internal class LambdaBuilder
    {
        public static CompilationUnitSyntax BuildLambdaClass(ClassDeclarationSyntax cls)
        {
            var requestType = cls.RequestType();
            var usings = SF.List(new[]
            {
                Strings.Usings.System(),
                Strings.Usings.StaticCode(),
                Strings.Usings.AlexaNetRequest()
            }.Select(SF.UsingDirective));

            var namespaceName = NamespaceHelper.Find(cls);
            if (namespaceName != null)
            {
                usings = usings.Add(SF.UsingDirective(namespaceName));
            }

            var initialSetup = SF.CompilationUnit().WithUsings(usings);

            var skillClass = MakeSkillISkillLambda(cls, requestType);

            initialSetup = namespaceName != null ? initialSetup.AddMembers(SF.NamespaceDeclaration(namespaceName).AddMembers(skillClass)) : initialSetup.AddMembers(skillClass);

            var pipelineInvocation = PipelineInvocation(cls, requestType);

            var main = MainTask(pipelineInvocation);

            var staticClass = SF.ClassDeclaration(Strings.ProgramClassName).WithModifiers(SF.TokenList(SF.Token(SyntaxKind.StaticKeyword))).AddMembers(main);

            return initialSetup.AddMembers(staticClass);
        }

        private static MemberDeclarationSyntax MainTask(InvocationExpressionSyntax pipelineInvocation)
        {
            return SF.MethodDeclaration(SF.IdentifierName(nameof(Task)), Strings.MainMethod)
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.StaticKeyword)))
                .WithParameterList(SF.ParameterList(SF.SingletonSeparatedList(SF.Parameter(SF.Identifier(Strings.ArgsVarName)).WithType(SF.IdentifierName(Strings.Types.StringArray)))))
                .WithExpressionBody(SF.ArrowExpressionClause(pipelineInvocation)).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken));
        }

        private static InvocationExpressionSyntax PipelineInvocation(ClassDeclarationSyntax cls, TypeSyntax requestType)
        {
            return SF.InvocationExpression(
                    SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        SF.IdentifierName(Strings.Types.LambdaHelper),
                        SF.GenericName(SF.Identifier(Strings.RunLambdaMethodName),
                            SF.TypeArgumentList(
                                SF.SeparatedList(new []
                                {
                                    requestType,
                                    SF.IdentifierName(cls.Identifier.Text)
                                })))))
                .WithArgumentList(SF.ArgumentList());
        }

        private static ClassDeclarationSyntax MakeSkillISkillLambda(ClassDeclarationSyntax cls, TypeSyntax requestType)
        {
            return SF.ClassDeclaration(cls.Identifier.Text)
                .WithModifiers(SF.TokenList(
                    SF.Token(SyntaxKind.PublicKeyword),
                    SF.Token(SyntaxKind.PartialKeyword)))
                .WithBaseList(SF.BaseList(
                    SF.SingletonSeparatedList<BaseTypeSyntax>(
                        SF.SimpleBaseType(
                            SF.GenericName(Strings.Types.SkillLambdaInterface)
                                .WithTypeArgumentList(SF.TypeArgumentList(
                                    SF.SingletonSeparatedList(requestType)))))));
        }
    }
}
