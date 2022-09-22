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

namespace ContentSecurityPolicy.AspNetCore;

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
                if (classSymbol.BaseType != null && GetClassBaseTypeName(classSymbol) == "PolicyOptionsBase")
                {
                    StringBuilder source = new();

                    source.AppendLinesIndented(0, "using System;");
                    source.AppendLinesIndented(0, "");
                    source.AppendLinesIndented(0, $"namespace ContentSecurityPolicy.AspNetCore;");
                    source.AppendLinesIndented(0, "");
                    source.AppendLinesIndented(0, $"public sealed partial class {GetClassTypeName(classSymbol)} : PolicyOptionsBase");
                    source.AppendLinesIndented(0, "{");

                    var codeAdded = false;

                    codeAdded = ProcessAttribute(compilation, classSymbol, "AddSelf", codeAdded, source);

                    source.AppendLinesIndented(0, "}");

                    if (codeAdded)
                    {
                        context.AddSource($"{GetClassTypeName(classSymbol, true)}.AddedFunctions.g.cs", source.ToString());
                    }
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
        var attributeData = classSymbol.GetAttributes().Where(ad => ad.AttributeClass.Name == attributeSymbol.Name).FirstOrDefault();

        if (attributeData != default)
        {
            var policyValue = (attribute.AttributeClass.GetMembers().Where(x => x.Name == "PolicyValue").FirstOrDefault() as IFieldSymbol).ConstantValue;

            if (codeAdded)
            {
                source.AppendLinesIndented(1, "");
                source.AppendLinesIndented(1, "");
            }

            source.AppendLinesIndented(1, $"/// <summary>");
            source.AppendLinesIndented(1, $"/// Adds {policyValue} to the policy.");
            source.AppendLinesIndented(1, $"/// </summary>");
            source.AppendLinesIndented(1, $"/// <returns></returns>");
            source.AppendLinesIndented(1, $"public {GetClassTypeName(classSymbol)} {attributeName}()");
            source.AppendLinesIndented(1, "{");
            source.AppendLinesIndented(2, $"AddValue(\"{policyValue}\");");
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


    public static string GetClassBaseTypeName(INamedTypeSymbol classSymbol)
    {
        if (classSymbol.BaseType.ToString() == "object")
        {
            return "";
        }

        var namespacePrefix = classSymbol.BaseType.ToString().Substring(0, classSymbol.BaseType.ToString().IndexOf(classSymbol.BaseType.Name));
        return classSymbol.BaseType.ToString().Replace(namespacePrefix, "");
    }


    private static string GetLongAttributeName(string shortAttributeName)
    {
        return shortAttributeName + "Attribute";
    }
}
