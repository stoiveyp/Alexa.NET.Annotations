//HintName: Example.g.cs
using Alexa.NET.RequestHandlers;
using Alexa.NET.RequestHandlers.Handlers;
using System.Threading.Tasks;
using System;
using System.Runtime.InteropServices;
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

    private class LaunchHandler
    {
        private Example Wrapper { get; }

        private LaunchHandler(Example wrapper)
        {
            Wrapper = wrapper;
        }

        private Task<SkillResponse> Execute()
        {
            return Wrapper.Launch();
        }
    }
}