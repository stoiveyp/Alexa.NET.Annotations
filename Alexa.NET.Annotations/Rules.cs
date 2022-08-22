using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;

namespace Alexa.NET.Annotations
{
    internal class Rules
    {
        private static readonly LocalizableResourceString InvalidParameterTitle = new (nameof(Resources.InvalidParameterTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString InvalidParameterMessageFormat = new (nameof(Resources.InvalidParameterMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString InvalidParameterDescription = new (nameof(Resources.InvalidParameterDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "AlexaSkill";
        public const string NewListDiagnosticId = "AlexaSkillInvalidParameter";

        public static readonly DiagnosticDescriptor InvalidParameterRule = new (NewListDiagnosticId, InvalidParameterTitle, InvalidParameterMessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: InvalidParameterDescription);
    }
}
