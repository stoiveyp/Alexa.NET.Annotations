using Xunit.Sdk;

namespace Alexa.NET.Annotations.Tests;

[UsesVerify]
public class LaunchSnapshotsTests
{
    [Fact]
    public Task Test1()
    {
        var sampleCode = System.IO.File.ReadAllText("Examples/LaunchExample.cs");
        return Utility.Verify(sampleCode);
    }
}