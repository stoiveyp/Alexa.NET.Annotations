//HintName: AlexaSkillLambdaHelper.g.cs
using Alexa.NET.Request;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Newtonsoft.Json;

namespace Alexa.NET.Annotations.StaticCode
{
    public class LambdaHelper
    {
        private static readonly JsonSerializer JsonSerializer = new();

        public static Task RunLambda<TRequest,TSkill>() where TRequest : SkillRequest where TSkill : ISkillLambda<TRequest>, new()
        {
            var skillClass = new TSkill();
            skillClass.Initialize();

            //https://docs.aws.amazon.com/lambda/latest/dg/csharp-handler.html

            return LambdaBootstrapBuilder
                .Create<TRequest, SkillResponse>(new Func<TRequest,ILambdaContext,Task<SkillResponse>>((request, context) => skillClass.Execute(request,context)),
                    new Amazon.Lambda.Serialization.Json.JsonSerializer()).Build().RunAsync();
        }
    }

    public interface ISkillLambda<TRequest>
    {
        Task<SkillResponse> Execute(TRequest request, object context = null);
        void Initialize();
    }
}