﻿//HintName: Example.skill.g.cs
using System;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Request.Type;
using Alexa.NET.RequestHandlers;
using Alexa.NET.RequestHandlers.Handlers;
using System.Threading.Tasks;

namespace TestyMcTestFace
{
    public partial class Example
    {
        private AlexaRequestPipeline _pipeline;
        public virtual Task<SkillResponse> Execute(SkillRequest skillRequest) => _pipeline.Process(skillRequest);
        public void Initialize()
        {
            _pipeline = new AlexaRequestPipeline(new IAlexaRequestHandler<SkillRequest>[]{new PlayAGameHandler(this)});
        }

        private class PlayAGameHandler : IntentNameRequestHandler
        {
            private Example Wrapper { get; }

            internal PlayAGameHandler(Example wrapper) : base("PlayAGame")
            {
                Wrapper = wrapper;
            }

            public override Task<SkillResponse> Handle(AlexaRequestInformation<SkillRequest> information) {
                var intentRequest = (IntentRequest)information.SkillRequest.Request;
                var move1 = intentRequest.Slots["move1"].Value;
                var move2 = intentRequest.Slots["move2"].Value;
                return Wrapper.PlayAGame(move1, move2);
            }
        }
    }
}