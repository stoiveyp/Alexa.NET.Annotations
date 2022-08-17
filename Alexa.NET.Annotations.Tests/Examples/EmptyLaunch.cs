using Alexa.NET;
using Alexa.NET.Annotations.Markers;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;

[AlexaSkill]
public partial class Example
{
    [Launch]
    public SkillResponse Launch()
    {
        return ResponseBuilder.Ask("What's your move? Rock, Paper or scissors?", new("What's your move?"));
    }
}
