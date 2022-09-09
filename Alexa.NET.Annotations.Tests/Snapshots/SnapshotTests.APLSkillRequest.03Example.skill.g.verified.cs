//HintName: Example.skill.g.cs
using System;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Request.Type;
using Alexa.NET.RequestHandlers;
using Alexa.NET.RequestHandlers.Handlers;
using System.Threading.Tasks;

public partial class Example
{
    private AlexaRequestPipeline<APLSkillRequest> _pipeline;
    public virtual Task<SkillResponse> Execute(APLSkillRequest skillRequest) => _pipeline.Process(skillRequest);
    public void Initialize()
    {
        _pipeline = new AlexaRequestPipeline<APLSkillRequest>(new IAlexaRequestHandler<APLSkillRequest>[]{new LaunchHandler(this), new FallbackHandler(this), new PlayAGameHandler(this)});
    }

    private class LaunchHandler : LaunchRequestHandler<APLSkillRequest>
    {
        private Example Wrapper { get; }

        internal LaunchHandler(Example wrapper)
        {
            Wrapper = wrapper;
        }

        public override Task<SkillResponse> Handle(AlexaRequestInformation<APLSkillRequest> information)
        {
            var request = (LaunchRequest)information.SkillRequest.Request;
            return Task.FromResult(Wrapper.Launch(request));
        }
    }

    private class FallbackHandler : IntentNameRequestHandler<APLSkillRequest>
    {
        private Example Wrapper { get; }

        internal FallbackHandler(Example wrapper) : base(BuiltInIntent.Fallback)
        {
            Wrapper = wrapper;
        }

        public override Task<SkillResponse> Handle(AlexaRequestInformation<APLSkillRequest> information)
        {
            var request = (IntentRequest)information.SkillRequest.Request;
            return Wrapper.Fallback(information.SkillRequest, request);
        }
    }

    private class PlayAGameHandler : IntentNameRequestHandler<APLSkillRequest>
    {
        private Example Wrapper { get; }

        internal PlayAGameHandler(Example wrapper) : base("PlayAGame")
        {
            Wrapper = wrapper;
        }

        public override Task<SkillResponse> Handle(AlexaRequestInformation<APLSkillRequest> information) => Wrapper.PlayAGame((IntentRequest)information.SkillRequest.Request);
    }
}