//HintName: Example.skill.g.cs
using System;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Request.Type;
using Alexa.NET.RequestHandlers;
using Alexa.NET.RequestHandlers.Handlers;
using System.Threading.Tasks;
using System.Threading.Tasks;

public partial class Example
{
    private AlexaRequestPipeline _pipeline;
    public virtual Task<SkillResponse> Execute(SkillRequest skillRequest) => _pipeline.Process(skillRequest);
    public void Initialize()
    {
        _pipeline = new AlexaRequestPipeline(new IAlexaRequestHandler<SkillRequest>[]{});
    }
}