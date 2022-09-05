using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Alexa.NET.Annotations.Markers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Alexa.NET.Annotations
{
    internal static class SkillBuilder
    {
        public static void Execute(SourceProductionContext context, ImmutableArray<ClassDeclarationSyntax?> args)
        {
            if (!args.Any())
            {
                return;
            }

            void AddHelper()
            {
                var assembly = Assembly.GetExecutingAssembly();
                var stream = assembly.GetManifestResourceStream("Alexa.NET.Annotations.StaticCode.LambdaHelper.cs");
                using var reader = new StreamReader(stream);
                context.AddSource("AlexaSkillLambdaHelper.g.cs", reader.ReadToEnd());
            }

            foreach (var cls in args.Where(a => a != null))
            {
                try
                {
                    context.AddSource($"{cls!.Identifier.Text}.skill.g.cs",
                        PipelineBuilder.BuildPipelineClasses(cls, context.ReportDiagnostic).ToCodeString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                if (cls!.ContainsAttributeNamed(nameof(AlexaLambdaAttribute).NameOnly()))
                {
                    AddHelper();
                    context.AddSource($"{cls.Identifier.Text}.lambda.g.cs",
                        LambdaBuilder.BuildLambdaClass(cls).ToCodeString());
                }
            }

        }

        public static string NameOnly(this string fullAttribute) => fullAttribute.Substring(0, fullAttribute.Length - 9);

        internal static string ToCodeString(this SyntaxNode token)
        {
            var sb = new StringBuilder();
            using var writer = new StringWriter(sb);
            token.NormalizeWhitespace().WriteTo(writer);
            return sb.ToString();
        }
    }
}
