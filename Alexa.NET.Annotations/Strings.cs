﻿using System;
using Alexa.NET.Request;
using Alexa.NET.RequestHandlers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Alexa.NET.Annotations
{
    public static class Strings
    {

        public const string HandlerInformationName = "information";
        public const string WrapperVarName = "wrapper";
        public const string WrapperPropertyName = "Wrapper";
        public const string HandlerMethodName = "Handle";
        public const string MainMethod = "Main";
        public const string ArgsVarName = "args";
        public const string ProgramClassName = "Program";
        public const string RunLambdaMethodName = "RunLambda";

        public const string HandlerSuffix = "Handler";

        public const string RequestProperty = "Request";

        public static class Names
        {
            public const string PipelineField = "_pipeline";
            public const string SkillRequestParameter = "skillRequest";
            public const string InitializeMethod = "Initialize";
            public const string ProcessMethod = "Process";
            public const string ExecuteMethod = "Execute";
            public const string HandlerInformationProperty = "information";
            public const string IntentProperty = "Intent";
            public const string TypedRequestObject = "request";
            public const string SlotValueProperty = "SlotValue";
            public const string SlotValueValueProperty = "Value";
            public const string SlotsProperty = "Slots";
        }

        public static class Types
        {
            public const string SkillLambdaInterface = "ISkillLambda";
            public const string StringArray = "string[]";
            public const string PipelineClass = nameof(AlexaRequestPipeline);
            public const string SkillRequest = nameof(SkillRequest);
            public const string SkillResponse = nameof(SkillResponse);
            public const string RequestHandlerInterface = nameof(IAlexaRequestHandler);
            public const string AlexaRequestInformation = nameof(AlexaRequestInformation);
            public const string Task = "Task";
            public const string HandlerInformation = "AlexaRequestInformation";
            public const string LambdaHelper = nameof(LambdaHelper);
            public const string Var = "var";
            public const string IntentRequest = nameof(IntentRequest);
            public const string String = "string";
            public const string Slot = "Slot";
            public const string FullSlot = "Alexa.NET.Request.Slot";
        }

        public static class Usings
        {
            public static NameSyntax System() => Utility.BuildName("System")!;
            public static NameSyntax StaticCode() => Utility.BuildName("Alexa", "NET", "Annotations", "StaticCode")!;
            public static NameSyntax AlexaNetRequest() => Utility.BuildName("Alexa", "NET", "Request")!;
            public static NameSyntax AlexaNetResponse() => Utility.BuildName("Alexa", "NET", "Response")!;
            public static NameSyntax AlexaNetResponseType() => Utility.BuildName("Alexa", "NET", "Request", "Type")!;
            public static NameSyntax RequestHandlers() => Utility.BuildName("Alexa", "NET", "RequestHandlers")!;
            public static NameSyntax RequestHandlerTypes() => Utility.BuildName("Alexa", "NET", "RequestHandlers", "Handlers")!;
            public static NameSyntax Tasks() => Utility.BuildName("System", "Threading", "Tasks")!;
        }
    }
}
