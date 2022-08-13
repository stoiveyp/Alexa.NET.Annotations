using Alexa.NET;
using Alexa.NET.Annotations.Markers;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;

namespace ExampleSkill
{
    [AlexaSkill]
    [AlexaLambda]
    public partial class RockPaperScissors
    {
        [Launch]
        public SkillResponse Launch(LaunchRequest intent)
        {
            return ResponseBuilder.Ask("What's your move? Rock, Paper or scissors?", new("What's your move?"));
        }

        [Intent("MakeMyMove")]
        public async Task<SkillResponse> PlayAGame(IntentRequest intentRequest)
        {
            return ResponseBuilder.Tell("You Win", null);
        }

        [Intent(BuiltInIntent.Help)]
        public SkillResponse Help(IntentRequest _) => ResponseBuilder.Empty();
    }
}