using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Alexa.NET.Annotations;

internal static class PipelineClassExtensions
{
    //https://andrewlock.net/creating-a-source-generator-part-2-testing-an-incremental-generator-with-snapshot-testing/

    public static ClassDeclarationSyntax BuildSkill(this ClassDeclarationSyntax skillClass, ClassDeclarationSyntax cls)
    {
        var handlers = cls.Members.OfType<MethodDeclarationSyntax>()
            .Where(HasMarkerAttribute).Select(MethodToPipelineClass);
        return skillClass
            .AddPipelineField()
            .AddExecuteMethod()
            .AddInitialization(handlers);

    }

    public static ClassDeclarationSyntax AddPipelineField(this ClassDeclarationSyntax skillClass)
    {
        var field = SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("AlexaRequestPipeline")))
            .AddDeclarationVariables(SyntaxFactory.VariableDeclarator("_pipeline"))
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));
        return skillClass.AddMembers(field);
    }

    public static ClassDeclarationSyntax AddInitialization(this ClassDeclarationSyntax skillClass,
        IEnumerable<ClassDeclarationSyntax> handlers)
    {
        var initializeMethod =
            SyntaxFactory.MethodDeclaration(SyntaxFactory.IdentifierName("AlexaRequestPipeline"), "Initialize")
                .AddBodyStatements(NotImplemented());
        return skillClass.AddMembers(initializeMethod).AddMembers(handlers.ToArray());
    }

    public static ClassDeclarationSyntax AddExecuteMethod(this ClassDeclarationSyntax skillClass)
    {
        var executeMethod = SyntaxFactory.MethodDeclaration(
            SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"), SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList<TypeSyntax>(SyntaxFactory.IdentifierName("SkillResponse")))), "Execute")
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
            .AddBodyStatements(NotImplemented());

        return skillClass.AddMembers(executeMethod);
    }

    private static ClassDeclarationSyntax MethodToPipelineClass(MethodDeclarationSyntax method)
    {
        return SyntaxFactory.ClassDeclaration(method.Identifier.Text + "Handler")
            .WithModifiers(SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));
    }

    private static bool HasMarkerAttribute(MethodDeclarationSyntax method)
    {
        return method.AttributeLists.SelectMany(a => a.Attributes).Any(a => a.Name is IdentifierNameSyntax
        {
            Identifier.Text: "Launch"
        });
    }

    private static ThrowStatementSyntax NotImplemented() => SyntaxFactory.ThrowStatement(
        SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName("NotImplementedException"),
            SyntaxFactory.ArgumentList(), null));
}