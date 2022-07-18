//HintName: Example.g.cs
using Alexa.NET.RequestHandlers;
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
        throw new NotImplementedException();
    }

    private class LaunchHandler
    {
    }
}