using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Alexa.NET.Annotations;

internal class SkillInformation
{
    private SkillInformation(ClassDeclarationSyntax[] handlers, ClassDeclarationSyntax[] interceptors)
    {
        Handlers = handlers;
        Interceptors = interceptors;
        SkillRequestType = Strings.Types.SkillRequest;
    }

    public ClassDeclarationSyntax[] Interceptors { get; }
    public ClassDeclarationSyntax[] Handlers { get; }
    public ClassDeclarationSyntax? SkillClass { get; private set; }
    public string SkillRequestType { get; }

    public bool HasInterceptors => Interceptors.Any();


    public static SkillInformation GenerateFrom(ClassDeclarationSyntax cls, Action<Diagnostic> reportDiagnostic)
    {
        var handlers = cls.Members.OfType<MethodDeclarationSyntax>()
            .Where(MarkerHelper.HasHandlerAttribute).Select(m => m.ToHandler(m.HandlerAttribute()!, cls, reportDiagnostic))
            .Where(c => c != null).ToArray();

        var interceptors = cls.Members.OfType<MethodDeclarationSyntax>()
            .Where(MarkerHelper.HasInterceptorAttribute).Select(m => m.ToInterceptor(m.InterceptorAttribute()!, cls, reportDiagnostic))
            .Where(c => c != null).ToArray();

        return new(handlers!, interceptors!);
    }

    public void SetBuiltSkill(ClassDeclarationSyntax skillClass)
    {
        SkillClass = skillClass;
    }
}