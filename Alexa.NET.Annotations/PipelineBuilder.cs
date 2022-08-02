using System.Collections.Immutable;
using System.Text;
using Alexa.NET.Request.Type;
using Alexa.NET.RequestHandlers.Handlers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Alexa.NET.Annotations
{
    internal static class PipelineBuilder
    {
        private const string PipelineClass = "AlexaRequestPipeline";
        private const string PipelineFieldName = "_pipeline";
        private static readonly string[] MarkerList = { "Launch" };

        public static void Execute(SourceProductionContext context, ImmutableArray<ClassDeclarationSyntax?> args)
        {
            if (!args.Any())
            {
                return;
            }

            foreach (var cls in args)
            {
                var hint = $"{cls.Identifier.Text}.g.cs";
                var pipelineCode = BuildPipelineSource(cls);
                context.AddSource(hint, pipelineCode);
            }
        }

        private static string BuildPipelineSource(ClassDeclarationSyntax cls)
        {
            var sb = new StringBuilder();
            var writer = new StringWriter(sb);
            var generatedCodeUnit = GenerateCodeUnit(cls);
            generatedCodeUnit.NormalizeWhitespace().WriteTo(writer);
            return sb.ToString();
        }

        private static CompilationUnitSyntax GenerateCodeUnit(ClassDeclarationSyntax cls)
        {
            var usings = SF.List(new []
            {
                SF.UsingDirective(SF.QualifiedName(SF.QualifiedName(SF.IdentifierName("Alexa"),SF.IdentifierName("NET")),SF.IdentifierName("Request"))),
                SF.UsingDirective(SF.QualifiedName(SF.QualifiedName(SF.IdentifierName("Alexa"),SF.IdentifierName("NET")),SF.IdentifierName("RequestHandlers"))),
                SF.UsingDirective(SF.QualifiedName(SF.QualifiedName(SF.QualifiedName(SF.IdentifierName("Alexa"),SF.IdentifierName("NET")),SF.IdentifierName("RequestHandlers")),SF.IdentifierName("Handlers"))),
                SF.UsingDirective(SF.QualifiedName(SF.QualifiedName(SF.IdentifierName("System"),SF.IdentifierName("Threading")),SF.IdentifierName("Tasks"))),
                SF.UsingDirective(SF.IdentifierName("System")),
            }.Concat(cls.Ancestors().OfType<CompilationUnitSyntax>().Single().Usings).Distinct());

            var initialSetup = SF.CompilationUnit().WithUsings(usings);

            var nsUsage = cls.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();

            var skillClass = SF.ClassDeclaration(cls.Identifier.Text)
                .WithModifiers(SF.TokenList(
                    SF.Token(SyntaxKind.PublicKeyword),
                    SF.Token(SyntaxKind.PartialKeyword)));

            //Debugger.Launch();
            if (nsUsage != null)
            {
                return initialSetup.AddMembers(SF.NamespaceDeclaration(nsUsage.Name).AddMembers(skillClass.BuildSkill(cls)));
            }

            return initialSetup.AddMembers(skillClass.BuildSkill(cls));
        }

        public static ClassDeclarationSyntax BuildSkill(this ClassDeclarationSyntax skillClass, ClassDeclarationSyntax cls)
        {
            var handlers = cls.Members.OfType<MethodDeclarationSyntax>()
                .Where(HasMarkerAttribute).Select(m => MethodToPipelineClass(m, MarkerAttribute(m), cls));

            return skillClass
                .AddPipelineField()
                .AddExecuteMethod()
                .AddInitialization(handlers);

        }

        public static ClassDeclarationSyntax AddPipelineField(this ClassDeclarationSyntax skillClass)
        {
            var field = SF.FieldDeclaration(SF.VariableDeclaration(SF.IdentifierName("AlexaRequestPipeline")))
                .AddDeclarationVariables(SF.VariableDeclarator(PipelineFieldName))
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PrivateKeyword)));
            return skillClass.AddMembers(field);
        }

        public static ClassDeclarationSyntax AddInitialization(this ClassDeclarationSyntax skillClass,
            IEnumerable<ClassDeclarationSyntax> handlers)
        {
            var newPipeline = SF.ObjectCreationExpression(SF.IdentifierName(PipelineClass))
                .WithArgumentList(SF.ArgumentList(SF.SeparatedList(new[]{SF.Argument(
                SF.ImplicitArrayCreationExpression(
                    SF.InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                        SF.SeparatedList<ExpressionSyntax>(
                        handlers.Select(h =>
                            SF.ObjectCreationExpression(SF.IdentifierName(h.Identifier.Text)).WithArgumentList(
                                SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(SF.ThisExpression())))))))))})));

            var initializeMethod =
                SF.MethodDeclaration(SF.IdentifierName(PipelineClass), "Initialize")
                    .AddBodyStatements(
                        SF.ExpressionStatement(SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SF.IdentifierName(PipelineFieldName), newPipeline)),
                        SF.ReturnStatement(SF.IdentifierName(PipelineFieldName)));
            return skillClass.AddMembers(initializeMethod).AddMembers(handlers.ToArray());
        }

        public static ClassDeclarationSyntax AddExecuteMethod(this ClassDeclarationSyntax skillClass)
        {
            var executeMethod = SF.MethodDeclaration(
                SF.GenericName(SF.Identifier(nameof(Task)), SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName("SkillResponse")))), "Execute")
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword)))
                .AddBodyStatements(NotImplemented());

            return skillClass.AddMembers(executeMethod);
        }

        private static ClassDeclarationSyntax MethodToPipelineClass(MethodDeclarationSyntax method, string marker, ClassDeclarationSyntax containerClass)
        {
            if (marker == null) throw new ArgumentNullException(nameof(marker));
            var info = MarkerTypeInfo[marker];
            return SF.ClassDeclaration(method.Identifier.Text + "Handler")
                .WithBaseList(SF.BaseList(SF.SingletonSeparatedList(info.BaseType)))
                .WithModifiers(SF.TokenList(
                    SF.Token(SyntaxKind.PrivateKeyword)))
                .AddWrapperField(containerClass)
                .AddWrapperConstructor(containerClass)
                .AddExecuteMethod(method,info);
        }

        public static Dictionary<string, MarkerInfo> MarkerTypeInfo = new()
        {
            { "Launch", new(nameof(LaunchRequestHandler), nameof(LaunchRequest)) }
        };


        private static string? MarkerAttribute(MethodDeclarationSyntax method)
        {
            return method.AttributeLists.SelectMany(a => a.Attributes)
                .Select(a => a.Name is IdentifierNameSyntax id && MarkerList.Contains(id.Identifier.Text) ? id.Identifier.Text : null).FirstOrDefault();
        }

        private static bool HasMarkerAttribute(MethodDeclarationSyntax method)
        {
            return !string.IsNullOrWhiteSpace(MarkerAttribute(method));
        }

        private static ThrowStatementSyntax NotImplemented() => SF.ThrowStatement(
            SF.ObjectCreationExpression(SF.IdentifierName(nameof(NotImplementedException)),
                SF.ArgumentList(), null));
    }

    public class MarkerInfo
    {
        public MarkerInfo(string baseClassName, string requestType)
        {
            BaseType = SF.SimpleBaseType(SF.IdentifierName(baseClassName));
            RequestType = SF.IdentifierName(requestType);
        }

        public BaseTypeSyntax BaseType { get; }
        public IdentifierNameSyntax RequestType { get; }

    }
}
