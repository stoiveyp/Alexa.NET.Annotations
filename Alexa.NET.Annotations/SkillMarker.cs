using System.Reflection;
using Alexa.NET.Annotations.Markers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Alexa.NET.Annotations
{
    internal static class SkillMarker
    {
        public static void StaticCodeGeneration(IncrementalGeneratorPostInitializationContext obj)
        {

        }

        public static bool AttributePredicate(SyntaxNode sn, CancellationToken _)
        {
            return sn is ClassDeclarationSyntax;
        }

        public static ClassDeclarationSyntax? SkillClasses(GeneratorSyntaxContext context, CancellationToken _)
        {
            if (context.Node is not ClassDeclarationSyntax cls)
            {
                return null;
            }

            return cls.ContainsAttributeNamed(nameof(AlexaSkillAttribute).NameOnly()) ? cls : null;
        }

        public static bool ContainsAttributeNamed(this ClassDeclarationSyntax cls, string markerName) =>
            cls.GetAttributeNamed(markerName) != null;

        public static AttributeSyntax? GetAttributeNamed(this ClassDeclarationSyntax cls, string markerName)
        => cls.AttributeLists
                .SelectMany(al => al.Attributes)
                .FirstOrDefault(n => n.MarkerName() == markerName);
        
    }
}
