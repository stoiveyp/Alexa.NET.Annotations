using Alexa.NET;
using Alexa.NET.Annotations.Markers;
using Alexa.NET.Request.Type;
using Alexa.NET.RequestHandlers;
using Alexa.NET.Response;

[AlexaSkill]
public partial class Example
{
    [BeforeExecution]
    public Task<SkillResponse> Before1(AlexaRequestInformation information)
    {
        return Task.FromResult(SkillResponse.Empty());
    }

    [AfterExecution]
    public Task<SkillResponse> After1()
    {
        return Task.FromResult(SkillResponse.Empty());
    }

    [AfterExecution]
    public void After2(AlexaRequestInformation information, SkillResponse response)
    {

    }

    [BeforeExecution]
    public Task Before2()
    {
        return Task.CompletedTask;
    }

    [BeforeExecution]
    public Task Before3(SkillResponse response)
    {
        return Task.CompletedTask;
    }

    [BeforeExecution]
    public void Before4() { }
}
