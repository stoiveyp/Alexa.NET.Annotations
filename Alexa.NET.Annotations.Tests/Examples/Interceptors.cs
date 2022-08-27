using Alexa.NET;
using Alexa.NET.Annotations.Markers;
using Alexa.NET.Request.Type;
using Alexa.NET.RequestHandlers;
using Alexa.NET.Response;

[AlexaSkill]
public partial class Example
{
    [BeforeExecution]
    public Task<SkillResponse> Before1()
    {
        //INVALID - before execute method can't have a skill response return type
        return Task.FromResult(SkillResponse.Empty());
    }

    [AfterExecution]
    public Task<SkillResponse> After1()
    {
        //INVALID - after execute signature returning skill response must have a SkillResponse parameter
        return Task.FromResult(SkillResponse.Empty());
    }

    [AfterExecution]
    public void After2(SkillResponse response)
    {

    }

    [BeforeExecution]
    public Task Before2()
    {
        return Task.CompletedTask;
    }
}
