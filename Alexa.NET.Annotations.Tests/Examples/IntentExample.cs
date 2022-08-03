﻿using Alexa.NET;
using Alexa.NET.Annotations.Markers;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using System.Threading.Tasks;

[AlexaSkill]
public partial class Example
{
    [Intent("PlayAGame")]
    public async Task<SkillResponse> PlayAGame(IntentRequest intent)
    {
        return ResponseBuilder.Tell("you win");
    }
}