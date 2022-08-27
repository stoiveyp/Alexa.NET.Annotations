﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Alexa.NET.Annotations
{
    internal static class PipelineBuilder
    {
        public static CompilationUnitSyntax BuildPipelineClasses(ClassDeclarationSyntax cls, Action<Diagnostic> reportDiagnostic)
        {
            var skillClass = SF.ClassDeclaration(cls.Identifier.Text)
                .WithModifiers(SF.TokenList(
                    SF.Token(SyntaxKind.PublicKeyword),
                    SF.Token(SyntaxKind.PartialKeyword)));


            var skillInfo = skillClass.BuildSkill(cls, reportDiagnostic);

            var usings = SF.List(new[]
            {
                Strings.Usings.System(),
                Strings.Usings.AlexaNetRequest(),
                Strings.Usings.AlexaNetResponse(),
                Strings.Usings.AlexaNetResponseType(),
                Strings.Usings.RequestHandlers(),
                Strings.Usings.RequestHandlerTypes(),
                Strings.Usings.Tasks(),
                skillInfo.HasInterceptors ? Strings.Usings.Interceptors() : null
            }.Where(u => u != null).Select(SF.UsingDirective!));

            var nsName = NamespaceHelper.Find(cls);
            var initialSetup = SF.CompilationUnit().WithUsings(usings);

            if (nsName != null)
            {
                return initialSetup.AddMembers(SF.NamespaceDeclaration(nsName).AddMembers(skillInfo.SkillClass!));
            }

            return initialSetup.AddMembers(skillInfo.SkillClass!);
        }

        public static SkillInformation BuildSkill(this ClassDeclarationSyntax skillClass, ClassDeclarationSyntax cls, Action<Diagnostic> reportDiagnostic)
        {
            var info = SkillInformation.GenerateFrom(cls, reportDiagnostic);
            info.SetBuiltSkill(skillClass
                .AddPipelineField()
                .AddExecuteMethod()
                .AddInitialization(info));
            return info;

        }

        public static ClassDeclarationSyntax AddPipelineField(this ClassDeclarationSyntax skillClass)
        {
            var field = SF.FieldDeclaration(SF.VariableDeclaration(SF.IdentifierName(Strings.Types.PipelineClass)))
                .AddDeclarationVariables(SF.VariableDeclarator(Strings.Names.PipelineField))
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PrivateKeyword)));
            return skillClass.AddMembers(field);
        }

        public static ClassDeclarationSyntax AddInitialization(this ClassDeclarationSyntax skillClass, SkillInformation information)
        {
            var argumentList = new List<ArgumentSyntax> { information.HandlerArray() };

            if (information.HasInterceptors)
            {
                //Error Handlers - not yet supported
                argumentList.Add(SF.Argument(SF.LiteralExpression(SyntaxKind.NullLiteralExpression)));
                argumentList.Add(information.InterceptorArray());
                argumentList.Add(SF.Argument(SF.LiteralExpression(SyntaxKind.NullLiteralExpression)));
            }

            var newPipeline = SF.ObjectCreationExpression(SF.IdentifierName(Strings.Types.PipelineClass))
                .WithArgumentList(SF.ArgumentList(SF.SeparatedList(argumentList)));

            var initializeMethod =
                SF.MethodDeclaration(SF.PredefinedType(SF.Token(SyntaxKind.VoidKeyword)), Strings.Names.InitializeMethod)
                    .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword)))
                    .AddBodyStatements(
                        SF.ExpressionStatement(SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SF.IdentifierName(Strings.Names.PipelineField), newPipeline)));
            return skillClass.AddMembers(initializeMethod).AddMembers(information.Handlers);
        }

        private static ArgumentSyntax HandlerArray(this SkillInformation information)
        {
            var arrayType = SF.ArrayType(SF.GenericName(
                        SF.Identifier(Strings.Types.RequestHandlerInterface))
                    .WithTypeArgumentList(SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(information.SkillRequestType)))))
                .WithRankSpecifiers(SF.SingletonList(SF.ArrayRankSpecifier(SF.SingletonSeparatedList<ExpressionSyntax>(SF.OmittedArraySizeExpression()))));

            return SF.Argument(
                SF.ArrayCreationExpression(arrayType,
                    SF.InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                        SF.SeparatedList<ExpressionSyntax>(
                            information.Handlers.Select(h =>
                                SF.ObjectCreationExpression(SF.IdentifierName(h.Identifier.Text)).WithArgumentList(
                                    SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(SF.ThisExpression())))))))));
        }

        private static ArgumentSyntax InterceptorArray(this SkillInformation information)
        {
            var arrayType = SF.ArrayType(SF.GenericName(
                        SF.Identifier(Strings.Types.RequestInterceptorInterface))
                    .WithTypeArgumentList(SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(information.SkillRequestType)))))
                .WithRankSpecifiers(SF.SingletonList(SF.ArrayRankSpecifier(SF.SingletonSeparatedList<ExpressionSyntax>(SF.OmittedArraySizeExpression()))));

            return SF.Argument(
                SF.ArrayCreationExpression(arrayType,
                    SF.InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                        SF.SeparatedList<ExpressionSyntax>(
                            information.Interceptors.Select(h =>
                                SF.ObjectCreationExpression(SF.IdentifierName(h.Identifier.Text)).WithArgumentList(
                                    SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(SF.ThisExpression())))))))));
        }

        public static ClassDeclarationSyntax AddExecuteMethod(this ClassDeclarationSyntax skillClass)
        {
            var invokePipeline = SF.InvocationExpression(
                SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName(Strings.Names.PipelineField), SF.IdentifierName(Strings.Names.ProcessMethod)))
                .WithArgumentList(SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(SF.IdentifierName(Strings.Names.SkillRequestParameter)))));

            var executeMethod = SF.MethodDeclaration(
                    SF.GenericName(SF.Identifier(nameof(Task)),
                        SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName(Strings.Types.SkillResponse)))),
                    Strings.Names.ExecuteMethod)
                .WithParameterList(SF.ParameterList(SF.SingletonSeparatedList(SF.Parameter(SF.Identifier(Strings.Names.SkillRequestParameter)).WithType(SF.IdentifierName(Strings.Types.SkillRequest)))))
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.VirtualKeyword)))
                .WithExpressionBody(SF.ArrowExpressionClause(invokePipeline)).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken));

            return skillClass.AddMembers(executeMethod);
        }
    }
}
