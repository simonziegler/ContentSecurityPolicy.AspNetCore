using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
#if DEBUG && GENERATOR_DEBUG
using System.Diagnostics;
#endif
using System.Linq;
using System.Text;

namespace Vectis.SourceGenerator;

[Generator]
internal class SourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUG && GENERATOR_DEBUG
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif

        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        Extensions.LinesGenerated = 0;

        // retreive the populated receiver 
        if (context.SyntaxReceiver is not SyntaxReceiver receiver)
        {
            return;
        }
        
        // we're going to create a new compilation that contains the attribute.
        // TODO: we should allow source generators to provide source during initialize, so that this step isn't required.
        //CSharpParseOptions options = (context.Compilation as CSharpCompilation).SyntaxTrees[0].Options as CSharpParseOptions;
        Compilation compilation = context.Compilation;

        foreach (var classNode in receiver.Classes)
        {
            var modifiers = classNode.Modifiers.Select(m => m.Text).ToList();
            SemanticModel classModel = compilation.GetSemanticModel(classNode.SyntaxTree);
            INamedTypeSymbol classSymbol = classModel.GetDeclaredSymbol(classNode);

            try
            {
                StringBuilder source = new();

                var codeAdded = false;

                codeAdded = ProcessAttribute(compilation, classSymbol, "AddSelf", codeAdded, source);

                if (codeAdded)
                {
                    context.AddSource($"{GetClassTypeName(classSymbol, true)}.AddedFunctions.g.cs", source.ToString());
                }
            }
            catch (Exception exception)
            {
                context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
                        "CSP0001",
                        $"Source Generation Exception",
                        $"Source generation encountered an exception building '{classSymbol.Name}': '{exception.Message}'",
                        "Vectis Source Generation",
                        DiagnosticSeverity.Error,
                        true
                    ), classNode.GetLocation()));
            }
        }
    }


    private bool ProcessAttribute(Compilation compilation, INamedTypeSymbol classSymbol, string attributeName, bool codeAdded, StringBuilder source)
    {
        var attribute = classSymbol.GetAttributes().Where(ad => ad.AttributeClass.Name == $"{GetLongAttributeName(attributeName)}").FirstOrDefault();
        INamedTypeSymbol attributeSymbol = compilation.GetTypeByMetadataName($"ContentSecurityPolicy.AspNetCore.{GetLongAttributeName(attributeName)}");

        if (classSymbol.GetAttributes().Any(ad => ad.AttributeClass.Name == attributeSymbol.Name))
        {
            var attributeData = classSymbol.GetAttributes().SingleOrDefault(ad => ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));

            var functionName = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "FunctionName").Value;
            var policyValue = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "PolicyValue").Value;

            if (codeAdded)
            {
                source.AppendLinesIndented(1, "");
                source.AppendLinesIndented(1, "");
            }

            source.AppendLinesIndented(1, $"/// <summary>");
            source.AppendLinesIndented(1, $"/// Adds {policyValue} to the policy.");
            source.AppendLinesIndented(1, $"/// </summary>");
            source.AppendLinesIndented(1, $"/// <returns></returns>");
            source.AppendLinesIndented(1, $"public {GetClassTypeName(classSymbol)} {functionName}()");
            source.AppendLinesIndented(1, "{");
            source.AppendLinesIndented(2, $"AddValue({policyValue})");
            source.AppendLinesIndented(2, "return this;");
            source.AppendLinesIndented(1, "}");

            return true;
        }

        return codeAdded;
    }


    /// <summary>
    /// Created on demand before each generation pass
    /// </summary>
    class SyntaxReceiver : ISyntaxReceiver
    {
        ///// <summary>
        ///// Dictionary keyed by class nodes that have a ViewModelRecord attribute and with value being a list of
        ///// properties with the ViewModelProperty attribute in that record.
        ///// </summary>
        //public readonly Dictionary<ClassDeclarationSyntax, List<PropertyDeclarationSyntax>> ClassNodes = new();


        /// <summary>
        /// List of classes with the ViewModelRecord attribute. Diagnostic reporting will be created for these classes
        /// because the attribute is for partial records only.
        /// </summary>
        public readonly List<ClassDeclarationSyntax> Classes = new();


        /// <summary>
        /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
        /// </summary>
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // any field with at least one attribute is a candidate for property generation
            if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
            {
                Classes.Add(classDeclarationSyntax);
            }
        }
    }


    private static string GetClassTypeName(INamedTypeSymbol classSymbol, bool suppressGeneric = false)
    {
        if (suppressGeneric)
        {
            return classSymbol.Name;
        }

        return classSymbol.ConstructedFrom.ToString().Substring(classSymbol.ConstructedFrom.ToString().IndexOf(classSymbol.Name));
    }


    private static string GetLongAttributeName(string shortAttributeName)
    {
        return shortAttributeName + "Attribute";
    }
}
