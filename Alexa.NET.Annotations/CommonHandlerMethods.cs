using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Alexa.NET.Annotations
{
    internal static class CommonHandlerMethods
    {
        public static ClassDeclarationSyntax AddWrapperConstructor(this ClassDeclarationSyntax skillClass, MethodDeclarationSyntax method, ClassDeclarationSyntax wrapperClass)
        {
            return skillClass;
        }
    }
}
