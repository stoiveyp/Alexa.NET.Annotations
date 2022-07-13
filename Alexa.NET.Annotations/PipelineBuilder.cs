using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Alexa.NET.Annotations
{
    internal class PipelineBuilder
    {
        public static void Execute(SourceProductionContext context, (Compilation Compilation, ImmutableArray<ClassDeclarationSyntax?> Syntax) args)
        {
            if (!args.Syntax.Any())
            {
                return;
            }

            foreach (var cls in args.Syntax)
            {
                var hint = $"{cls.Identifier.Text}.g.cs";
                var pipelineCode = BuildPipelineSource(cls);
                context.AddSource(hint, pipelineCode);
            }
        }

        private static string BuildPipelineSource(ClassDeclarationSyntax cls)
        {
            var sb = new StringBuilder();
            var writer = new StringWriter(sb);
            var generatedCodeUnit = GenerateCodeUnit(cls);
            generatedCodeUnit.NormalizeWhitespace().WriteTo(writer);
            return sb.ToString();
        }

        private static CompilationUnitSyntax GenerateCodeUnit(ClassDeclarationSyntax cls)
        {
            var usings = SyntaxFactory.List(new UsingDirectiveSyntax[]
            {

            }.Concat(cls.Ancestors().OfType<CompilationUnitSyntax>().Single().Usings).Distinct());

            var initialSetup = SyntaxFactory.CompilationUnit().WithUsings(usings);

            var nsUsage = cls.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();

            var newClass = SyntaxFactory.ClassDeclaration(cls.Identifier.Text)
                .WithModifiers(SyntaxFactory.TokenList(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.PartialKeyword)));

            //Debugger.Launch();
            if (nsUsage != null)
            {
                return initialSetup.AddMembers(SyntaxFactory.NamespaceDeclaration(nsUsage.Name).AddMembers(newClass));
            }

            return initialSetup.AddMembers(newClass);
        }
    }
}
