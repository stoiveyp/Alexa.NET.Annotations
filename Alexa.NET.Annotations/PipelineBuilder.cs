using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Alexa.NET.Annotations.Markers;
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

        public static void Execute(SourceProductionContext context, ImmutableArray<ClassDeclarationSyntax?> args)
        {
            if (!args.Any())
            {
                return;
            }

            if (args.Any(c => c.ContainsAttributeNamed(nameof(AlexaLambdaAttribute).NameOnly())))
            {
                var assembly = Assembly.GetExecutingAssembly();
                var stream = assembly.GetManifestResourceStream("Alexa.NET.Annotations.StaticCode.LambdaHelper.cs");
                using var reader = new StreamReader(stream);
                context.AddSource("AlexaSkillLambdaHelper.g.cs", reader.ReadToEnd());
            }

            foreach (var cls in args)
            {
                var hint = $"{cls.Identifier.Text}.skill.g.cs";
                var pipelineCode = BuildPipelineSource(cls);
                context.AddSource(hint, pipelineCode);
            }
        }

        public static string NameOnly(this string fullAttribute) => fullAttribute.Substring(0,fullAttribute.Length-9);

        private static string BuildPipelineSource(ClassDeclarationSyntax cls)
        {
            var sb = new StringBuilder();
            var writer = new StringWriter(sb);
            var generatedCodeUnit = GenerateCodeUnit(cls);
            generatedCodeUnit.NormalizeWhitespace().WriteTo(writer);
            return sb.ToString();
        }

        private static NameSyntax? BuildName(params string[] pieces) => pieces.Aggregate<string?, NameSyntax?>(null, (current, piece) => current == null
                ? SF.IdentifierName(piece)
                : SF.QualifiedName(current, SF.IdentifierName(piece)));

        private static CompilationUnitSyntax GenerateCodeUnit(ClassDeclarationSyntax cls)
        {
            var usings = SF.List(new []
            {
                SF.UsingDirective(BuildName("System")!),
                SF.UsingDirective(BuildName("Alexa","NET","Request")!),
                SF.UsingDirective(BuildName("Alexa","NET","Response")!),
                SF.UsingDirective(BuildName("Alexa","NET","Request","Type")!),
                SF.UsingDirective(BuildName("Alexa","NET","RequestHandlers")!),
                SF.UsingDirective(BuildName("Alexa","NET","RequestHandlers","Handlers")!),
                SF.UsingDirective(BuildName("System","Threading","Tasks")!),
            }.Distinct());

            var initialSetup = SF.CompilationUnit().WithUsings(usings);

            var nsUsage = cls.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();

            var skillClass = SF.ClassDeclaration(cls.Identifier.Text)
                .WithModifiers(SF.TokenList(
                    SF.Token(SyntaxKind.PublicKeyword),
                    SF.Token(SyntaxKind.PartialKeyword)));

            
            if (nsUsage != null)
            {
                return initialSetup.AddMembers(SF.NamespaceDeclaration(nsUsage.Name).AddMembers(skillClass.BuildSkill(cls)));
            }

            return initialSetup.AddMembers(skillClass.BuildSkill(cls));
        }

        public static ClassDeclarationSyntax BuildSkill(this ClassDeclarationSyntax skillClass, ClassDeclarationSyntax cls)
        {
            var handlers = cls.Members.OfType<MethodDeclarationSyntax>()
                .Where(MarkerHelper.HasMarkerAttribute).Select(m => MethodToPipelineClass(m, m.MarkerAttribute()!, cls));

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
                SF.ArrayCreationExpression(SF.ArrayType(SF.IdentifierName("IAlexaRequestHandler<SkillRequest>[]")),
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
            const string skillRequestParameterName = "skillRequest";
            var invokePipeline = SF.InvocationExpression(
                SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,SF.IdentifierName(PipelineFieldName),SF.IdentifierName("Process")))
                .WithArgumentList(SF.ArgumentList(SF.SingletonSeparatedList(SF.Argument(SF.IdentifierName(skillRequestParameterName)))));

            var executeMethod = SF.MethodDeclaration(
                    SF.GenericName(SF.Identifier(nameof(Task)),
                        SF.TypeArgumentList(SF.SingletonSeparatedList<TypeSyntax>(SF.IdentifierName("SkillResponse")))),
                    "Execute")
                .WithParameterList(SF.ParameterList(SF.SingletonSeparatedList(SF.Parameter(SF.Identifier("skillRequest")).WithType(SF.IdentifierName("SkillRequest")))))
                .WithModifiers(SF.TokenList(SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.VirtualKeyword)))
                .WithExpressionBody(SF.ArrowExpressionClause(invokePipeline)).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken));

            return skillClass.AddMembers(executeMethod);
        }

        private static ClassDeclarationSyntax MethodToPipelineClass(MethodDeclarationSyntax method, AttributeSyntax marker, ClassDeclarationSyntax containerClass)
        {
            if (marker == null) throw new ArgumentNullException(nameof(marker));
            var info = MarkerInfo.MarkerTypeInfo[marker.MarkerName()!];
            return SF.ClassDeclaration(method.Identifier.Text + "Handler")
                .WithBaseList(SF.BaseList(SF.SingletonSeparatedList(info.BaseType(marker))))
                .WithModifiers(SF.TokenList(
                    SF.Token(SyntaxKind.PrivateKeyword)))
                .AddWrapperField(containerClass)
                .AddWrapperConstructor(containerClass, info.Constructor?.Invoke(marker))
                .AddExecuteMethod(method,info);
        }

        private static ThrowStatementSyntax NotImplemented() => SF.ThrowStatement(
            SF.ObjectCreationExpression(SF.IdentifierName(nameof(NotImplementedException)),
                SF.ArgumentList(), null));
    }
}
