using Alexa.NET;
using Alexa.NET.Annotations;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;

namespace ExampleSkill
{
    [AlexaSkillLambda]
    public class RockPaperScissors
    {
        [Launch]
        public  SkillResponse Launch(LaunchRequest intent, ILambdaContext _)
        {
            return ResponseBuilder.Ask("What's your move? Rock, Paper or scissors?", new("What's your move?"));
        }
    }
}