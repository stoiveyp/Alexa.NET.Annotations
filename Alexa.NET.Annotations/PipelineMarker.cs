using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Alexa.NET.Annotations
{
    internal class PipelineMarker
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

            if (list.Parent is ClassDeclarationSyntax cls)
            {
                return cls;
            }

            return null;
        }
    }
}
