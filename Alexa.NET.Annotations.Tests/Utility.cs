using Alexa.NET.Annotations.Markers;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Alexa.NET.Annotations.Tests
{
    internal class Utility
    {
        public static Task Verify(string sampleCode)
        {
            var tree = CSharpSyntaxTree.ParseText(sampleCode);

            IEnumerable<PortableExecutableReference> references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(AlexaSkillAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(SkillResponse).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ILambdaContext).Assembly.Location),
            };

            var compilation = CSharpCompilation.Create("Tests", new [] { tree }, references:references);

            var generator = new PipelineGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGenerators(compilation);

            var result = driver.GetRunResult();

            return Verifier.Verify(driver).UseDirectory("Snapshots");
        }
    }
}
