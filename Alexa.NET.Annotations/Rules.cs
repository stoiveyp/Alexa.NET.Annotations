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
        private const string InvalidParameterDiagnosticId = "AlexaSkillInvalidParameter";

        public static readonly DiagnosticDescriptor InvalidParameterRule = new DiagnosticDescriptor(InvalidParameterDiagnosticId, InvalidParameterTitle, InvalidParameterMessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: InvalidParameterDescription);

        private static readonly LocalizableResourceString InvalidHandlerReturnTypeTitle = new(nameof(Resources.InvalidHandlerReturnTypeTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString InvalidHandlerReturnTypeMessageFormat = new(nameof(Resources.InvalidHandlerReturnTypeMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString InvalidHandlerReturnTypeDescription = new(nameof(Resources.InvalidHandlerReturnTypeDescription), Resources.ResourceManager, typeof(Resources));
        private const string InvalidHandlerReturnTypeDiagnosticId = "AlexaSkillInvalidHandlerReturnType";

        public static readonly DiagnosticDescriptor InvalidHandlerReturnTypeRule = new DiagnosticDescriptor(InvalidHandlerReturnTypeDiagnosticId, InvalidHandlerReturnTypeTitle, InvalidHandlerReturnTypeMessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: InvalidHandlerReturnTypeDescription);

        private static readonly LocalizableResourceString InvalidInterceptorReturnTypeTitle = new(nameof(Resources.InvalidInterceptorReturnTypeTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString InvalidInterceptorReturnTypeMessageFormat = new(nameof(Resources.InvalidInterceptorReturnTypeMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString InvalidInterceptorReturnTypeDescription = new(nameof(Resources.InvalidInterceptorReturnTypeDescription), Resources.ResourceManager, typeof(Resources));
        private const string InvalidInterceptorReturnTypeDiagnosticId = "AlexaSkillInvalidInterceptorReturnType";

        public static readonly DiagnosticDescriptor InvalidInterceptorReturnTypeRule = new DiagnosticDescriptor(InvalidInterceptorReturnTypeDiagnosticId, InvalidInterceptorReturnTypeTitle, InvalidInterceptorReturnTypeMessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: InvalidInterceptorReturnTypeDescription);
    }
}
