//HintName: Example.lambda.g.cs
using System;
using Alexa.NET.Annotations.StaticCode;
using Alexa.NET.Request;

public partial class Example : ISkillLambda<APLSkillRequest>
{
}

static class Program
{
    static Task Main(string[] args) => LambdaHelper.RunLambda<APLSkillRequest, Example>();
}