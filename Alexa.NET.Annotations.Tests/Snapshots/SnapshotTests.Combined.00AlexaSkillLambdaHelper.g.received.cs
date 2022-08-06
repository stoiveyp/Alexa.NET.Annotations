//HintName: AlexaSkillLambdaHelper.g.cs
using Alexa.NET.Request;
using Alexa.NET.Response;
using Amazon.Lambda.RuntimeSupport;
using Newtonsoft.Json;

namespace Alexa.NET.Annotations.StaticCode
{
    public class LambdaHelper
    {
        private static readonly MemoryStream ResponseStream = new();
        private static readonly JsonSerializer JsonSerializer = new();

        private static async Task RunLambda<T>(string[] args) where T:ISkillLambda,new()
        {
            var skillClass = new T();
            using var bootstrap = new LambdaBootstrap(req => HandleInvocation(req, skillClass.Process));
            await bootstrap.RunAsync();
        }

        private static async Task<InvocationResponse> HandleInvocation(InvocationRequest invocation, Func<SkillRequest, Task<SkillResponse>> process)
        {
            using var jr = new JsonTextReader(new StreamReader(invocation.InputStream));
            var input = JsonSerializer.Deserialize<SkillRequest>(jr);

            var output =  await process(input);

            ResponseStream.SetLength(0);
            using var jw = new JsonTextWriter(new StreamWriter(ResponseStream));
            JsonSerializer.Serialize(jw, output);
            ResponseStream.Position = 0;

            return new InvocationResponse(ResponseStream, false);
        }
    }

    internal interface ISkillLambda
    {
        Task<SkillResponse> Process(SkillRequest request);
    }
}
