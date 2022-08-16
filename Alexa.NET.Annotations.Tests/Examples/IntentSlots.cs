using Alexa.NET;
using Alexa.NET.Annotations.Markers;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using System.Threading.Tasks;

namespace TestyMcTestFace;

[AlexaSkill]
public partial class Example
{
    [Intent("PlayAGame")]
    public async Task<SkillResponse> PlayAGame(string move1, string move2)
    {
        return ResponseBuilder.Tell("you win");
    }
}