//HintName: Example.skill.g.cs
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
        public virtual Task<SkillResponse> Execute(SkillRequest skillRequest, object context = null) => _pipeline.Process(skillRequest, context);
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

            public override Task<SkillResponse> Handle(AlexaRequestInformation<SkillRequest> information)
            {
                var request = (IntentRequest)information.SkillRequest.Request;
                var move1 = request.Intent.Slots["move1"].SlotValue.Value;
                var move2 = request.Intent.Slots["move2"];
                return Wrapper.PlayAGame(move1, move2);
            }
        }
    }
}