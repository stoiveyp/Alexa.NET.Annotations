//HintName: Example.skill.g.cs
using System;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Request.Type;
using Alexa.NET.RequestHandlers;
using Alexa.NET.RequestHandlers.Handlers;
using System.Threading.Tasks;
using Alexa.NET.RequestHandlers.Interceptors;

public partial class Example
{
    private AlexaRequestPipeline _pipeline;
    public virtual Task<SkillResponse> Execute(SkillRequest skillRequest) => _pipeline.Process(skillRequest);
    public void Initialize()
    {
        _pipeline = new AlexaRequestPipeline(new IAlexaRequestHandler<SkillRequest>[]{}, null, new IAlexaRequestInterceptor<SkillRequest>[]{new Before1Handler(this), new After1Handler(this), new After2Handler(this), new Before2Handler(this), new Before3Handler(this), new Before4Handler(this)}, null);
    }

    private class Before1Handler : IAlexaRequestInterceptor
    {
        private Example Wrapper { get; }

        internal Before1Handler(Example wrapper)
        {
            Wrapper = wrapper;
        }

        public override async Task<SkillResponse> Handle(AlexaRequestInformation<SkillRequest> information, RequestInterceptorCall<SkillRequest> next)
        {
            var interceptorResponse = await Wrapper.Before1(information);
            if (interceptorResponse != null)
            {
                return interceptorResponse;
            }

            return await next(information);
        }
    }

    private class After1Handler : IAlexaRequestInterceptor
    {
        private Example Wrapper { get; }

        internal After1Handler(Example wrapper)
        {
            Wrapper = wrapper;
        }

        public override async Task<SkillResponse> Handle(AlexaRequestInformation<SkillRequest> information, RequestInterceptorCall<SkillRequest> next)
        {
            var response = await next(information);
            var interceptorResponse = await Wrapper.After1();
            if (interceptorResponse != null)
            {
                return interceptorResponse;
            }

            return response;
        }
    }

    private class After2Handler : IAlexaRequestInterceptor
    {
        private Example Wrapper { get; }

        internal After2Handler(Example wrapper)
        {
            Wrapper = wrapper;
        }

        public override async Task<SkillResponse> Handle(AlexaRequestInformation<SkillRequest> information, RequestInterceptorCall<SkillRequest> next)
        {
            var response = await next(information);
            Wrapper.After2(information, response);
            return response;
        }
    }

    private class Before2Handler : IAlexaRequestInterceptor
    {
        private Example Wrapper { get; }

        internal Before2Handler(Example wrapper)
        {
            Wrapper = wrapper;
        }

        public override async Task<SkillResponse> Handle(AlexaRequestInformation<SkillRequest> information, RequestInterceptorCall<SkillRequest> next)
        {
            await Wrapper.Before2();
            return await next(information);
        }
    }

    private class Before3Handler : IAlexaRequestInterceptor
    {
        private Example Wrapper { get; }

        internal Before3Handler(Example wrapper)
        {
            Wrapper = wrapper;
        }

        public override async Task<SkillResponse> Handle(AlexaRequestInformation<SkillRequest> information, RequestInterceptorCall<SkillRequest> next)
        {
            await Wrapper.Before3();
            return await next(information);
        }
    }

    private class Before4Handler : IAlexaRequestInterceptor
    {
        private Example Wrapper { get; }

        internal Before4Handler(Example wrapper)
        {
            Wrapper = wrapper;
        }

        public override async Task<SkillResponse> Handle(AlexaRequestInformation<SkillRequest> information, RequestInterceptorCall<SkillRequest> next)
        {
            Wrapper.Before4();
            return await next(information);
        }
    }
}