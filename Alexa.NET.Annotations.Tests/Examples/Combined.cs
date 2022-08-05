using Alexa.NET;
using Alexa.NET.Annotations.Markers;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using System.Threading.Tasks;

[AlexaSkill]
public partial class Example
{
    [Launch]
    public SkillResponse Launch(LaunchRequest intent, ILambdaContext _)
    {
        return ResponseBuilder.Ask("What's your move? Rock, Paper or scissors?", new("What's your move?"));
    }

    [Intent(BuiltInIntent.Fallback)]
    public async Task<SkillResponse> Fallback(IntentRequest intent)
    {
        return ResponseBuilder.Tell("you win");
    }

    [Intent("PlayAGame")]
    public async Task<SkillResponse> PlayAGame(IntentRequest intent)
    {
        return ResponseBuilder.Tell("you win");
    }
}
