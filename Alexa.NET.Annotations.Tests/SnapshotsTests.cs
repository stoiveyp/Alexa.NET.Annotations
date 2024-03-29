using Xunit.Sdk;

namespace Alexa.NET.Annotations.Tests;

[UsesVerify]
public class SnapshotTests
{
    [Fact]
    public Task Launch()
    {
        var sampleCode = System.IO.File.ReadAllText("Examples/LaunchExample.cs");
        return Utility.Verify(sampleCode);
    }

    [Fact]
    public Task Intent()
    {
        var sampleCode = System.IO.File.ReadAllText("Examples/IntentExample.cs");
        return Utility.Verify(sampleCode);
    }

    [Fact]
    public Task IntentSlots()
    {
        var sampleCode = System.IO.File.ReadAllText("Examples/IntentSlots.cs");
        return Utility.Verify(sampleCode);
    }

    [Fact]
    public Task Combined()
    {
        var sampleCode = System.IO.File.ReadAllText("Examples/Combined.cs");
        return Utility.Verify(sampleCode);
    }

    [Fact]
    public Task APLSkillRequest()
    {
        var sampleCode = System.IO.File.ReadAllText("Examples/APLSkillRequest.cs");
        return Utility.Verify(sampleCode);
    }

    [Fact]
    public void AlexaLambdaAddsHelper()
    {
        var sampleCode = System.IO.File.ReadAllText("Examples/Combined.cs");
        Assert.True(Utility.HasSource(sampleCode, "AlexaSkillLambdaHelper.g.cs"));
    }

    [Fact]
    public Task InvalidReturnTypeCheck()
    {
        var sampleCode = System.IO.File.ReadAllText("Examples/InvalidReturnType.cs");
        return Utility.Verify(sampleCode);
    }

    [Fact]
    public Task Interceptors()
    {
        var sampleCode = System.IO.File.ReadAllText("Examples/Interceptors.cs");
        return Utility.Verify(sampleCode);
    }
}