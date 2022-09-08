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
    private AlexaRequestPipeline<SkillRequest> _pipeline;
    public virtual Task<SkillResponse> Execute(SkillRequest skillRequest) => _pipeline.Process(skillRequest);
    public void Initialize()
    {
        _pipeline = new AlexaRequestPipeline<SkillRequest>(new IAlexaRequestHandler<SkillRequest>[]{}, null, new IAlexaRequestInterceptor<SkillRequest>[]{new Before1Interceptor(this), new After1Interceptor(this), new After2Interceptor(this), new Before2Interceptor(this), new Before3Interceptor(this), new Before4Interceptor(this)}, null);
    }

    private class Before1Interceptor : IAlexaRequestInterceptor
    {
        private Example Wrapper { get; }

        internal Before1Interceptor(Example wrapper)
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

    private class After1Interceptor : IAlexaRequestInterceptor
    {
        private Example Wrapper { get; }

        internal After1Interceptor(Example wrapper)
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

    private class After2Interceptor : IAlexaRequestInterceptor
    {
        private Example Wrapper { get; }

        internal After2Interceptor(Example wrapper)
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

    private class Before2Interceptor : IAlexaRequestInterceptor
    {
        private Example Wrapper { get; }

        internal Before2Interceptor(Example wrapper)
        {
            Wrapper = wrapper;
        }

        public override async Task<SkillResponse> Handle(AlexaRequestInformation<SkillRequest> information, RequestInterceptorCall<SkillRequest> next)
        {
            await Wrapper.Before2();
            return await next(information);
        }
    }

    private class Before3Interceptor : IAlexaRequestInterceptor
    {
        private Example Wrapper { get; }

        internal Before3Interceptor(Example wrapper)
        {
            Wrapper = wrapper;
        }

        public override async Task<SkillResponse> Handle(AlexaRequestInformation<SkillRequest> information, RequestInterceptorCall<SkillRequest> next)
        {
            await Wrapper.Before3();
            return await next(information);
        }
    }

    private class Before4Interceptor : IAlexaRequestInterceptor
    {
        private Example Wrapper { get; }

        internal Before4Interceptor(Example wrapper)
        {
            Wrapper = wrapper;
        }

        public override Task<SkillResponse> Handle(AlexaRequestInformation<SkillRequest> information, RequestInterceptorCall<SkillRequest> next)
        {
            Wrapper.Before4();
            return next(information);
        }
    }
}