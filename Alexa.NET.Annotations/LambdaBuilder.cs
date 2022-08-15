using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Alexa.NET.Annotations
{
    internal class LambdaBuilder
    {
        public static CompilationUnitSyntax BuildLambdaClass(ClassDeclarationSyntax cls)
        {
            var usings = SF.List(new[]
            {
                SF.UsingDirective(Utility.BuildName("System")!),
                SF.UsingDirective(Utility.BuildName("Alexa","NET","Annotations","StaticCode")!)
            }.Distinct());

            var namespaceName = Utility.FindNamespace(cls);
            if (namespaceName != null)
            {
                usings = usings.Add(SF.UsingDirective(namespaceName));
            }

            var initialSetup = SF.CompilationUnit().WithUsings(usings);

            var skillClass = MakeSkillISkillLambda(cls);

            initialSetup = namespaceName != null ? initialSetup.AddMembers(SF.NamespaceDeclaration(namespaceName).AddMembers(skillClass)) : initialSetup.AddMembers(skillClass);

            var pipelineInvocation = PipelineInvocation(cls);

            var main = MainTask(pipelineInvocation);

            var staticClass = SF.ClassDeclaration("Program").WithModifiers(SF.TokenList(SF.Token(SyntaxKind.StaticKeyword))).AddMembers(main);

            return initialSetup.AddMembers(staticClass);
        }

        private static MemberDeclarationSyntax MainTask(InvocationExpressionSyntax pipelineInvocation)
        {
            return SF.MethodDeclaration(SF.IdentifierName(nameof(Task)), "Main")
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.StaticKeyword)))
                .WithParameterList(SF.ParameterList(SF.SingletonSeparatedList(SF.Parameter(SF.Identifier("args")).WithType(SF.IdentifierName("string[]")))))
                .WithExpressionBody(SF.ArrowExpressionClause(pipelineInvocation)).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken));
        }

        private static InvocationExpressionSyntax PipelineInvocation(ClassDeclarationSyntax cls)
        {
            return SF.InvocationExpression(
                    SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        SF.IdentifierName("LambdaHelper"),
                        SF.GenericName(SF.Identifier("RunLambda"),
                            SF.TypeArgumentList(
                                SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(cls.Identifier.Text))))))
                .WithArgumentList(SF.ArgumentList());
        }

        private static ClassDeclarationSyntax MakeSkillISkillLambda(ClassDeclarationSyntax cls)
        {
            return SF.ClassDeclaration(cls.Identifier.Text)
                .WithModifiers(SF.TokenList(
                    SF.Token(SyntaxKind.PublicKeyword),
                    SF.Token(SyntaxKind.PartialKeyword)))
                .WithBaseList(SF.BaseList(SF.SingletonSeparatedList<BaseTypeSyntax>(SF.SimpleBaseType(SF.IdentifierName("ISkillLambda")))));
        }
    }
}
