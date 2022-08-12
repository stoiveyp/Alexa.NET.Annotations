using Alexa.NET.Request;
using Alexa.NET.Response;
using Amazon.Lambda.RuntimeSupport;
using Newtonsoft.Json;

namespace Alexa.NET.Annotations.StaticCode
{
    public class LambdaHelper
    {
        private static readonly JsonSerializer JsonSerializer = new();

        public static Task RunLambda<T>() where T : ISkillLambda, new()
        {
            var skillClass = new T();
            skillClass.Initialize();

            //https://docs.aws.amazon.com/lambda/latest/dg/csharp-handler.html

            return LambdaBootstrapBuilder
                .Create<SkillRequest, SkillResponse>(skillClass.Execute,
                    new Amazon.Lambda.Serialization.Json.JsonSerializer()).Build().RunAsync();
        }
    }

    public interface ISkillLambda
    {
        Task<SkillResponse> Execute(SkillRequest request);
        void Initialize();
    }
}