using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Alexa.NET.Annotations
{
    [Generator]
    public class SkillGenerator:IIncrementalGenerator
    {
        //Working through https://andrewlock.net/exploring-dotnet-6-part-9-source-generator-updates-incremental-generators/
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<ClassDeclarationSyntax?> candidateMarkers = context.SyntaxProvider.CreateSyntaxProvider(
                SkillMarker.AttributePredicate,
                SkillMarker.SkillClasses).Where(c => c != null);
            var combined = candidateMarkers.Collect();
            context.RegisterSourceOutput(combined, SkillBuilder.Execute);
            context.RegisterPostInitializationOutput(SkillMarker.StaticCodeGeneration);
        }
    }
}
