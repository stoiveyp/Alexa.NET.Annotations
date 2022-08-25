using Alexa.NET;
using Alexa.NET.Annotations.Markers;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;

[AlexaSkill]
public partial class Example
{
    [Launch]
    public SkillRequest Launch(LaunchRequest intent)
    {
        return ResponseBuilder.Ask("What's your move? Rock, Paper or scissors?", new("What's your move?"));
    }
}
