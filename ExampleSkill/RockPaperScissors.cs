﻿using Alexa.NET;
using Alexa.NET.Annotations.Markers;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.RequestHandlers;
using Alexa.NET.Response;

namespace ExampleSkill
{
    [AlexaSkill(typeof(APLSkillRequest))]
    [AlexaLambda]
    public partial class RockPaperScissors
    {
        [Launch]
        public SkillResponse Launch()
        {
            return ResponseBuilder.Ask("What's your move? Rock, Paper or scissors?", new("What's your move?"));
        }

        [Intent("MakeMyMove")]
        public async Task<SkillResponse> PlayAGame(IntentRequest intentRequest, string move1)
        {
            return ResponseBuilder.Tell("You Win", null);
        }

        [Intent(BuiltInIntent.Help)]
        public SkillResponse Help(AlexaRequestInformation<APLSkillRequest> information) => ResponseBuilder.Empty();
    }
}