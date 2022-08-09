//HintName: Example.lambda.g.cs
using System;
using Alexa.NET.Annotations.StaticCode;

public partial class Example : ISkillLambda
{
    static Task Main(string[] args) => LambdaHelper.RunLambda<Example>(args);
}