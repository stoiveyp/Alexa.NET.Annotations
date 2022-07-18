using System.Runtime.CompilerServices;

namespace Alexa.NET.Annotations.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Enable();
    }
}