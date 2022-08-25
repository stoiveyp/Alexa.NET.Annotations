using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;

namespace Alexa.NET.Annotations
{
    internal class Rules
    {
        private const string Category = "AlexaSkill";

        private static readonly LocalizableResourceString InvalidParameterTitle = new (nameof(Resources.InvalidParameterTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString InvalidParameterMessageFormat = new (nameof(Resources.InvalidParameterMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString InvalidParameterDescription = new (nameof(Resources.InvalidParameterDescription), Resources.ResourceManager, typeof(Resources));
        public const string InvalidParameterDiagnosticId = "AlexaSkillInvalidParameter";

        public static readonly DiagnosticDescriptor InvalidParameterRule = new DiagnosticDescriptor(InvalidParameterDiagnosticId, InvalidParameterTitle, InvalidParameterMessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: InvalidParameterDescription);

        private static readonly LocalizableResourceString InvalidReturnTypeTitle = new(nameof(Resources.InvalidReturnTypeTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString InvalidReturnTypeMessageFormat = new(nameof(Resources.InvalidReturnTypeMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString InvalidReturnTypeDescription = new(nameof(Resources.InvalidReturnTypeDescription), Resources.ResourceManager, typeof(Resources));
        public const string InvalidReturnTypeDiagnosticId = "AlexaSkillInvalidReturnType";

        public static readonly DiagnosticDescriptor InvalidReturnTypeRule = new DiagnosticDescriptor(InvalidReturnTypeDiagnosticId, InvalidReturnTypeTitle, InvalidReturnTypeMessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: InvalidReturnTypeDescription);
    }
}
