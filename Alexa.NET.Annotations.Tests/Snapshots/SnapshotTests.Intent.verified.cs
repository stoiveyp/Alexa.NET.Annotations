//HintName: Example.g.cs
using Alexa.NET.Request;
using Alexa.NET.RequestHandlers;
using Alexa.NET.RequestHandlers.Handlers;
using System.Threading.Tasks;
using System;
using Alexa.NET;
using Alexa.NET.Annotations.Markers;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using System.Threading.Tasks;

public partial class Example
{
    private AlexaRequestPipeline _pipeline;
    public virtual Task<SkillResponse> Execute(SkillRequest skillRequest) => _pipeline.Process(skillRequest);
    AlexaRequestPipeline Initialize()
    {
        _pipeline = new AlexaRequestPipeline(new IAlexaRequestHandler<SkillRequest>[]{new PlayAGameHandler(this)});
        return _pipeline;
    }

    private class PlayAGameHandler : IntentNameRequestHandler
    {
        private Example Wrapper { get; }

        internal PlayAGameHandler(Example wrapper) : base("PlayAGame")
        {
            Wrapper = wrapper;
        }

        public override Task<SkillResponse> Handle(AlexaRequestInformation<SkillRequest> information) => Wrapper.PlayAGame((IntentRequest)information.SkillRequest.Request);
    }
}