using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Alexa.NET.Annotations.Tests
{
    internal class Utility
    {
        public static Task Verify(string sampleCode)
        {
            var tree = CSharpSyntaxTree.ParseText(sampleCode);

            var compilation = CSharpCompilation.Create("Tests", new [] { tree });

            var generator = new PipelineGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGenerators(compilation);
            return Verifier.Verify(driver).UseDirectory("Snapshots");
        }

        public static bool HasSource(string sampleCode, string name)
        {
            var tree = CSharpSyntaxTree.ParseText(sampleCode);

            var compilation = CSharpCompilation.Create("Tests", new[] { tree });

            var generator = new PipelineGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGenerators(compilation);
            return driver.GetRunResult().Results.SelectMany(r => r.GeneratedSources).Any(s => s.HintName == name);
        }
    }
}
