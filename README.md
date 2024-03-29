# Alexa.NET.Annotations
Library to help make writing your first Alexa skill smaller and easier

## Creating an Alexa Skill

To create a skill, add Alexa.NET.Annotations as a NuGet reference and then you can tag a class with the `AlexaSkill` attribute.
The big requirement is that the class has to be `partial` as the generator adds code to your class behind the scenes.

```csharp
using Alexa.NET;
using Alexa.NET.Annotations.Markers;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;

[AlexaSkill]
public partial class RockPaperScissors
{
    [Launch]
    public SkillResponse Launch(LaunchRequest intent)
    {
        return ResponseBuilder.Ask("What's your move? Rock, Paper or scissors?", new("What's your move?"));
    }

    [Intent("MakeMyMove")]
    public async Task<SkillResponse> PlayAGame(IntentRequest intentRequest)
    {
        return ResponseBuilder.Tell("You Win", null);
    }

    [Intent(BuiltInIntent.Help)]
    public SkillResponse Help(IntentRequest _) => ResponseBuilder.Empty();
}
```

These attributes add an `Execute` method to your class, which has this signature, and can be called by your code.

```csharp
public virtual Task<SkillResponse> Execute(SkillRequest skillRequest, object context = null);
```

## Attributes

There are several attributes available right now. The method name you attach these two doesn't matter and can be called anything, they're just examples.

### Launch

The launch attribute is for when your skill starts.

Return Type must be either `SkillResponse` or `Task<SkillResponse>`

```csharp
public SkillResponse Launch();
public Task<SkillResponse> Launch(LaunchRequest intent);
```

### Intent(IntentName)

The intent attribute wires up to a specific intent, named in the attribute argument.
If the signature contains string or Slot parameters, they are mapped to intent slots.

Return Type must be either `SkillResponse` or `Task<SkillResponse>`

Example signatures
```csharp
[Intent("Test")]
public async Task<SkillResponse> Intent(IntentRequest intentRequest);

[Intent("Test2")]
public SkillResponse Intent(IntentRequest intentRequest);

[Intent("Test3")]
public SkillResponse Intent(string slotOne, Slot slotTwo);
```

### BeforeResponse/AfterResponse

These two attributes are used to run common functionality you want to run before or after the handlers are run to generate the response.

Their return type can be either `void`, `SkillResponse' or 'Task<SkillResponse>`

AfterResponse also has access to the SkillResponse that is going to be returned as an optional argument

Examples
```csharp

[BeforeResponse]
public void LogBefore() {
    Logger.Information("running before the response is generated")
}

[AfterResponse]
public SkillResponse LogAfter(SkillResponse response) {
    return response;
}
```


## Wiring up an AWS Lambda

If you plan to use an AWS Lambda project with the .NET 6 runtime, Alexa.NET.Annotations has an extra attribute you can place at the class level which will wire up the AWS Lambda straight to your skill.

```csharp
[AlexaSkill]
[AlexaLambda]
public partial class RockPaperScissors
{
    ...
```

There are two requirements to make this work
*  Your class must have a parameterless constructor
*  You need to add a NuGet reference to [https://www.nuget.org/packages/Amazon.Lambda.RuntimeSupport/](https://www.nuget.org/packages/Amazon.Lambda.RuntimeSupport/)

This will generate a Program class and Main method straight to your skill pipeline. The `ILambdaContext` object passed into your lambda will automatically be wired up as the context parameter in your `Execute` method.

To make this work - when you push your code to AWS Lambda, where you'd normally reference the handler in a format of `[AssemblyName]::[Type]::[Method]` you just put `[AssemblyName]]`

Here's an example of this using the AWS Lambda Upload screen in Visual Studio

![AWS Lambda upload screen in visual studio showing the assembly handler attribute](/docs/uploadScreen.jpg?raw=true)