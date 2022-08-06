using System.Reflection;
using Alexa.NET.Annotations.Markers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Alexa.NET.Annotations
{
    internal static class PipelineMarker
    {
        public static void StaticCodeGeneration(IncrementalGeneratorPostInitializationContext obj)
        {

        }

        public static bool AttributePredicate(SyntaxNode sn, CancellationToken _)
        {
            return sn is AttributeSyntax;
        }

        public static ClassDeclarationSyntax? SkillClasses(GeneratorSyntaxContext context, CancellationToken _)
        {
            if (context.Node is not AttributeSyntax att)
            {
                return null;
            }

            if (att.Parent is not AttributeListSyntax list)
            {
                return null;
            }

            if (list.Parent is ClassDeclarationSyntax cls && cls.ContainsAttributeNamed(nameof(AlexaSkillAttribute).NameOnly()))
            {
                return cls;
            }

            return null;
        }

        public static bool ContainsAttributeNamed(this ClassDeclarationSyntax cls, string markerName)
        {
            return cls.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(n => n.MarkerName() == markerName);
        }
    }
}
