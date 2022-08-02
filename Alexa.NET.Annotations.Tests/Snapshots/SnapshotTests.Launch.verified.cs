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

public partial class Example
{
    private AlexaRequestPipeline _pipeline;
    public Task<SkillResponse> Execute()
    {
        throw new NotImplementedException();
    }

    AlexaRequestPipeline Initialize()
    {
        _pipeline = new AlexaRequestPipeline(new[]{new LaunchHandler(this)});
        return _pipeline;
    }

    private class LaunchHandler : LaunchRequestHandler
    {
        private Example Wrapper { get; }

        internal LaunchHandler(Example wrapper)
        {
            Wrapper = wrapper;
        }

        public override Task<SkillResponse> Handle(AlexaRequestInformation<SkillRequest> information) => Task.FromResult(Wrapper.Launch((LaunchRequest)information.SkillRequest.Request));
    }
}