using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Alexa.NET.Annotations
{
    [Generator]
    public class PipelineGenerator:IIncrementalGenerator
    {
        //Working through https://andrewlock.net/exploring-dotnet-6-part-9-source-generator-updates-incremental-generators/
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<ClassDeclarationSyntax?> candidateMarkers = context.SyntaxProvider.CreateSyntaxProvider(
                PipelineMarker.AttributePredicate,
                PipelineMarker.SkillClasses).Where(c => c != null);
            var combined = context.CompilationProvider.Combine(candidateMarkers.Collect());
            context.RegisterSourceOutput(combined, PipelineBuilder.Execute);
            context.RegisterPostInitializationOutput(PipelineMarker.StaticCodeGeneration);
        }
    }
}
