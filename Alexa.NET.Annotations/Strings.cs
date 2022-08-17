using System;
using Alexa.NET.Request;
using Alexa.NET.RequestHandlers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Alexa.NET.Annotations
{
    public static class Strings
    {
        public const string HandlerInformationName = "information";
        public const string TypedRequestObjectIdentifier = "request";
        public const string WrapperVarName = "wrapper";
        public const string WrapperPropertyName = "Wrapper";
        public const string HandlerMethodName = "Handle";
        public const string HandlerInformationPropertyName = "information";
        public const string MainMethod = "Main";
        public const string ArgsVarName = "args";
        public const string ProgramClassName = "Program";
        public const string RunLambdaMethodName = "RunLambda";

        public const string HandlerSuffix = "Handler";

        public const string TypeTask = "Task";
        public const string TypeHandlerInformation = "AlexaRequestInformation";
        public const string TypeLambdaHelper = "LambdaHelper";

        public const string RequestProperty = "Request";

        public static class Names
        {
            public const string PipelineField = "_pipeline";
            public const string SkillRequestParameter = "skillRequest";
            public const string InitializeMethod = "Initialize";
            public const string ProcessMethod = "Process";
            public const string ExecuteMethod = "Execute";
        }

        public static class Types
        {
            public const string SkillLambdaInterface = "ISkillLambda";
            public const string StringArray = "string[]";
            public const string PipelineClass = nameof(AlexaRequestPipeline);
            public const string SkillRequest = nameof(SkillRequest);
            public const string SkillResponse = nameof(SkillResponse);
            public const string RequestHandlerInterface = nameof(IAlexaRequestHandler);
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

