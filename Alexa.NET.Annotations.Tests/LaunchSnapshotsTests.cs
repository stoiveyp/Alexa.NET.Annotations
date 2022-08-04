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
}