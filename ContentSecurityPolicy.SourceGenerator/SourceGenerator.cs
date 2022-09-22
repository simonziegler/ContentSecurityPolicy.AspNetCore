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


    private readonly string[] _policyOptionAdditionalAttributes = { "AddNone", "AddReportSample", "AddSelf", "AddStrictDynamic", "AddUnsafeEval", "AddUnsafeHashes", "AddUnsafeInline" };


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

                source.AppendLinesIndented(0, "using System;");
                source.AppendLinesIndented(0, "");
                source.AppendLinesIndented(0, $"namespace ContentSecurityPolicy.AspNetCore;");
                source.AppendLinesIndented(0, "");
                source.AppendLinesIndented(0, $"public sealed partial class {GetClassTypeName(classSymbol)} : {GetClassBaseTypeName(classSymbol)}");
                source.AppendLinesIndented(0, "{");

                var codeAdded = false;

                codeAdded = ProcessPolicyAttribute(classSymbol, source, codeAdded);

                codeAdded = ProcessPolicyOptionsAttribute(classSymbol, source, codeAdded);

                foreach (var additionalAtributeName in _policyOptionAdditionalAttributes)
                {
                    codeAdded = ProcessAdditionalPolicyOptionsAttribute(classSymbol, additionalAtributeName, source, codeAdded);
                }

                codeAdded = ProcessGroupNamePolicyOptionsAttribute(classSymbol, source, codeAdded);
                codeAdded = ProcessHashValuePolicyOptionsAttribute(classSymbol, source, codeAdded);
                codeAdded = ProcessHostSourcePolicyOptionsAttribute(classSymbol, source, codeAdded);
                codeAdded = ProcessNoncePolicyOptionsAttribute(classSymbol, source, codeAdded);
                codeAdded = ProcessSchemeSourcePolicyOptionsAttribute(classSymbol, source, codeAdded);
                codeAdded = ProcessUriPolicyOptionsAttribute(classSymbol, source, codeAdded);

                source.AppendLinesIndented(0, "}");

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


    private bool ProcessPolicyAttribute(INamedTypeSymbol classSymbol, StringBuilder source, bool codeAdded)
    {
        var attribute = classSymbol.GetAttributes().Where(ad => ad.AttributeClass.Name == $"PolicyAttribute").FirstOrDefault();

        if (attribute == default)
        {
            return codeAdded;
        }

        var policyName = attribute.ConstructorArguments.FirstOrDefault().Value;

        source.AppendLinesIndented(1, "/// <inheritdoc/>");
        source.AppendLinesIndented(1, $"private protected override string PolicyName => \"{policyName}\";");

        source.AppendLinesIndented(1, "");
        source.AppendLinesIndented(1, "");
        source.AppendLinesIndented(1, $"private {GetClassTypeName(classSymbol)}Options Options {{ get; set; }}");

        source.AppendLinesIndented(1, "");
        source.AppendLinesIndented(1, "");
        source.AppendLinesIndented(1, $"public {GetClassTypeName(classSymbol)}(string nonceValue, Action<{GetClassTypeName(classSymbol)}Options> configureOptions)");
        source.AppendLinesIndented(1, "{");
        source.AppendLinesIndented(2, "Options = new(nonceValue);");
        source.AppendLinesIndented(2, "configureOptions.Invoke(Options);");
        source.AppendLinesIndented(1, "}");

        source.AppendLinesIndented(1, "");
        source.AppendLinesIndented(1, "");
        source.AppendLinesIndented(1, "/// <inheritdoc />");
        source.AppendLinesIndented(1, $"public override string GetPolicyValue()");
        source.AppendLinesIndented(1, "{");
        source.AppendLinesIndented(2, "return $\"{PolicyName} {string.Join(' ', Options.PolicyValues)};\";");
        source.AppendLinesIndented(1, "}");

        return true;
    }


    private bool ProcessPolicyOptionsAttribute(INamedTypeSymbol classSymbol, StringBuilder source, bool codeAdded)
    {
        var attribute = classSymbol.GetAttributes().Where(ad => ad.AttributeClass.Name == $"PolicyOptionsAttribute").FirstOrDefault();

        if (attribute == default)
        {
            return codeAdded;
        }

        source.AppendLinesIndented(1, $"public {GetClassTypeName(classSymbol)}(string nonceValue)");
        source.AppendLinesIndented(1, "{");
        source.AppendLinesIndented(2, "NonceValue = nonceValue;");
        source.AppendLinesIndented(1, "}");

        source.AppendLinesIndented(1, "");
        source.AppendLinesIndented(1, "");
        source.AppendLinesIndented(1, "/// <summary>");
        source.AppendLinesIndented(1, "/// Adds a policy value.");
        source.AppendLinesIndented(1, "/// </summary>");
        source.AppendLinesIndented(1, "/// <param name=\"value\">The value to be added to the policy</param>");
        source.AppendLinesIndented(1, "/// <returns></returns>");
        source.AppendLinesIndented(1, $"private {GetClassTypeName(classSymbol)} AddValue(string value)");
        source.AppendLinesIndented(1, "{");
        source.AppendLinesIndented(2, "PolicyValues.Add(value);");
        source.AppendLinesIndented(2, "return this;");
        source.AppendLinesIndented(1, "}");


        source.AppendLinesIndented(1, "");
        source.AppendLinesIndented(1, "");

        source.AppendLinesIndented(1, "/// <summary>");
        source.AppendLinesIndented(1, "/// Conditionally adds a policy value.");
        source.AppendLinesIndented(1, "/// </summary>");
        source.AppendLinesIndented(1, "/// <param name=\"value\">The value to be added to the policy</param>");
        source.AppendLinesIndented(1, "/// <param name=\"conditionalFunc\">The conditional function delegate determining whether to add the supplied value to the policy</param>");
        source.AppendLinesIndented(1, "/// <returns></returns>");
        source.AppendLinesIndented(1, $"private {GetClassTypeName(classSymbol)} AddValueIf(string value, Func<bool> conditionalFunc)");
        source.AppendLinesIndented(1, "{");
        source.AppendLinesIndented(2, "return conditionalFunc.Invoke() ? AddValue(value) : this;");
        source.AppendLinesIndented(1, "}");

        return true;
    }


    private bool ProcessAdditionalPolicyOptionsAttribute(INamedTypeSymbol classSymbol, string attributeName, StringBuilder source, bool codeAdded)
    {
        var attribute = classSymbol.GetAttributes().Where(ad => ad.AttributeClass.Name == $"{GetLongAttributeName(attributeName)}").FirstOrDefault();

        if (attribute == default)
        {
            return codeAdded;
        }

        source.AppendLinesIndented(1, "");
        source.AppendLinesIndented(1, "");

        var policyValue = (attribute.AttributeClass.GetMembers().Where(x => x.Name == "PolicyValue").FirstOrDefault() as IFieldSymbol).ConstantValue;

        source.AppendLinesIndented(1, "/// <summary>");
        source.AppendLinesIndented(1, $"/// Adds {policyValue} to the policy.");
        source.AppendLinesIndented(1, "/// </summary>");
        source.AppendLinesIndented(1, "/// <returns></returns>");
        source.AppendLinesIndented(1, $"public {GetClassTypeName(classSymbol)} {attributeName}()");
        source.AppendLinesIndented(1, "{");
        source.AppendLinesIndented(2, $"return AddValue(\"{policyValue}\");");
        source.AppendLinesIndented(1, "}");



        source.AppendLinesIndented(1, "");
        source.AppendLinesIndented(1, "");

        source.AppendLinesIndented(1, "/// <summary>");
        source.AppendLinesIndented(1, $"/// Conditionally adds {policyValue} to the policy.");
        source.AppendLinesIndented(1, "/// </summary>");
        source.AppendLinesIndented(1, $"/// <param name=\"conditionalFunc\">The conditional function delegate determining whether to add {policyValue} to the policy</param>");
        source.AppendLinesIndented(1, "/// <returns></returns>");
        source.AppendLinesIndented(1, $"public {GetClassTypeName(classSymbol)} {attributeName}If(Func<bool> conditionalFunc)");
        source.AppendLinesIndented(1, "{");
        source.AppendLinesIndented(2, $"return conditionalFunc.Invoke() ? {attributeName}() : this;");
        source.AppendLinesIndented(1, "}");

        return true;
    }


    private bool ProcessGroupNamePolicyOptionsAttribute(INamedTypeSymbol classSymbol, StringBuilder source, bool codeAdded)
    {
        var attribute = classSymbol.GetAttributes().Where(ad => ad.AttributeClass.Name == $"{GetLongAttributeName("AddGroupName")}").FirstOrDefault();

        if (attribute == default)
        {
            return codeAdded;
        }

        source.AppendLinesIndented(1, "");
        source.AppendLinesIndented(1, "");

        source.AppendLinesIndented(1, "/// <summary>");
        source.AppendLinesIndented(1, $"/// Adds a group name to the policy.");
        source.AppendLinesIndented(1, "/// </summary>");
        source.AppendLinesIndented(1, "/// <returns></returns>");
        source.AppendLinesIndented(1, $"public {GetClassTypeName(classSymbol)} AddGroupName(string groupName)");
        source.AppendLinesIndented(1, "{");
        source.AppendLinesIndented(2, "return AddValue(groupName);");
        source.AppendLinesIndented(1, "}");



        source.AppendLinesIndented(1, "");
        source.AppendLinesIndented(1, "");

        source.AppendLinesIndented(1, "/// <summary>");
        source.AppendLinesIndented(1, $"/// Conditionally adds a group name to the policy.");
        source.AppendLinesIndented(1, "/// </summary>");
        source.AppendLinesIndented(1, $"/// <param name=\"conditionalFunc\">The conditional function delegate determining whether to add the nonce to the policy</param>");
        source.AppendLinesIndented(1, "/// <returns></returns>");
        source.AppendLinesIndented(1, $"public {GetClassTypeName(classSymbol)} AddGroupNameIf(string groupName, Func<bool> conditionalFunc)");
        source.AppendLinesIndented(1, "{");
        source.AppendLinesIndented(2, $"return conditionalFunc.Invoke() ? AddGroupName(groupName) : this;");
        source.AppendLinesIndented(1, "}");

        return true;
    }


    private bool ProcessHashValuePolicyOptionsAttribute(INamedTypeSymbol classSymbol, StringBuilder source, bool codeAdded)
    {
        var attribute = classSymbol.GetAttributes().Where(ad => ad.AttributeClass.Name == $"{GetLongAttributeName("AddHashValue")}").FirstOrDefault();

        if (attribute == default)
        {
            return codeAdded;
        }

        source.AppendLinesIndented(1, "");
        source.AppendLinesIndented(1, "");

        source.AppendLinesIndented(1, "/// <summary>");
        source.AppendLinesIndented(1, $"/// Adds a hash value to the policy.");
        source.AppendLinesIndented(1, "/// </summary>");
        source.AppendLinesIndented(1, "/// <returns></returns>");
        source.AppendLinesIndented(1, $"public {GetClassTypeName(classSymbol)} AddHashValue(HashAlgorithm hashAlgorithm, string hashValue)");
        source.AppendLinesIndented(1, "{");
        source.AppendLinesIndented(2, "return AddValue($\"'{hashAlgorithm.ToString().ToLower()}-{hashValue}'\");");
        source.AppendLinesIndented(1, "}");



        source.AppendLinesIndented(1, "");
        source.AppendLinesIndented(1, "");

        source.AppendLinesIndented(1, "/// <summary>");
        source.AppendLinesIndented(1, $"/// Conditionally adds a hash value to the policy.");
        source.AppendLinesIndented(1, "/// </summary>");
        source.AppendLinesIndented(1, $"/// <param name=\"conditionalFunc\">The conditional function delegate determining whether to add the nonce to the policy</param>");
        source.AppendLinesIndented(1, "/// <returns></returns>");
        source.AppendLinesIndented(1, $"public {GetClassTypeName(classSymbol)} AddHashValueIf(HashAlgorithm hashAlgorithm, string hashValue, Func<bool> conditionalFunc)");
        source.AppendLinesIndented(1, "{");
        source.AppendLinesIndented(2, $"return conditionalFunc.Invoke() ? AddHashValue(hashAlgorithm, hashValue) : this;");
        source.AppendLinesIndented(1, "}");

        return true;
    }


    private bool ProcessHostSourcePolicyOptionsAttribute(INamedTypeSymbol classSymbol, StringBuilder source, bool codeAdded)
    {
        var attribute = classSymbol.GetAttributes().Where(ad => ad.AttributeClass.Name == $"{GetLongAttributeName("AddHostSourceValue")}").FirstOrDefault();

        if (attribute == default)
        {
            return codeAdded;
        }

        source.AppendLinesIndented(1, "");
        source.AppendLinesIndented(1, "");

        source.AppendLinesIndented(1, "/// <summary>");
        source.AppendLinesIndented(1, $"/// Adds a host source value to the policy.");
        source.AppendLinesIndented(1, "/// </summary>");
        source.AppendLinesIndented(1, "/// <returns></returns>");
        source.AppendLinesIndented(1, $"public {GetClassTypeName(classSymbol)} AddHostSource(string hostSourceValue)");
        source.AppendLinesIndented(1, "{");
        source.AppendLinesIndented(2, "return AddValue($\"'{hostSourceValue}'\");");
        source.AppendLinesIndented(1, "}");



        source.AppendLinesIndented(1, "");
        source.AppendLinesIndented(1, "");

        source.AppendLinesIndented(1, "/// <summary>");
        source.AppendLinesIndented(1, $"/// Conditionally adds a host source value to the policy.");
        source.AppendLinesIndented(1, "/// </summary>");
        source.AppendLinesIndented(1, $"/// <param name=\"conditionalFunc\">The conditional function delegate determining whether to add the nonce to the policy</param>");
        source.AppendLinesIndented(1, "/// <returns></returns>");
        source.AppendLinesIndented(1, $"public {GetClassTypeName(classSymbol)} AddHostSourceIf(string hostSourceValue, Func<bool> conditionalFunc)");
        source.AppendLinesIndented(1, "{");
        source.AppendLinesIndented(2, $"return conditionalFunc.Invoke() ? AddHostSource(hostSourceValue) : this;");
        source.AppendLinesIndented(1, "}");

        return true;
    }


    private bool ProcessNoncePolicyOptionsAttribute(INamedTypeSymbol classSymbol, StringBuilder source, bool codeAdded)
    {
        var attribute = classSymbol.GetAttributes().Where(ad => ad.AttributeClass.Name == $"{GetLongAttributeName("AddNonce")}").FirstOrDefault();

        if (attribute == default)
        {
            return codeAdded;
        }

        source.AppendLinesIndented(1, "");
        source.AppendLinesIndented(1, "");

        source.AppendLinesIndented(1, "/// <summary>");
        source.AppendLinesIndented(1, $"/// Adds a nonce to the policy.");
        source.AppendLinesIndented(1, "/// </summary>");
        source.AppendLinesIndented(1, "/// <returns></returns>");
        source.AppendLinesIndented(1, $"public {GetClassTypeName(classSymbol)} AddNonce()");
        source.AppendLinesIndented(1, "{");
        source.AppendLinesIndented(2, "return AddValue($\"'nonce-{NonceValue}'\");");
        source.AppendLinesIndented(1, "}");



        source.AppendLinesIndented(1, "");
        source.AppendLinesIndented(1, "");

        source.AppendLinesIndented(1, "/// <summary>");
        source.AppendLinesIndented(1, $"/// Conditionally adds a nonce to the policy.");
        source.AppendLinesIndented(1, "/// </summary>");
        source.AppendLinesIndented(1, $"/// <param name=\"conditionalFunc\">The conditional function delegate determining whether to add the nonce to the policy</param>");
        source.AppendLinesIndented(1, "/// <returns></returns>");
        source.AppendLinesIndented(1, $"public {GetClassTypeName(classSymbol)} AddNonceIf(Func<bool> conditionalFunc)");
        source.AppendLinesIndented(1, "{");
        source.AppendLinesIndented(2, $"return conditionalFunc.Invoke() ? AddNonce() : this;");
        source.AppendLinesIndented(1, "}");

        return true;
    }


    private bool ProcessSchemeSourcePolicyOptionsAttribute(INamedTypeSymbol classSymbol, StringBuilder source, bool codeAdded)
    {
        var attribute = classSymbol.GetAttributes().Where(ad => ad.AttributeClass.Name == $"{GetLongAttributeName("AddSchemeSource")}").FirstOrDefault();

        if (attribute == default)
        {
            return codeAdded;
        }

        source.AppendLinesIndented(1, "");
        source.AppendLinesIndented(1, "");

        source.AppendLinesIndented(1, "/// <summary>");
        source.AppendLinesIndented(1, $"/// Adds a scheme source to the policy.");
        source.AppendLinesIndented(1, "/// </summary>");
        source.AppendLinesIndented(1, "/// <returns></returns>");
        source.AppendLinesIndented(1, $"public {GetClassTypeName(classSymbol)} AddSchemeSource(SchemeSource schemeSource)");
        source.AppendLinesIndented(1, "{");
        source.AppendLinesIndented(2, "return AddValue($\"{schemeSource.ToString().ToLower()}:\");");
        source.AppendLinesIndented(1, "}");



        source.AppendLinesIndented(1, "");
        source.AppendLinesIndented(1, "");

        source.AppendLinesIndented(1, "/// <summary>");
        source.AppendLinesIndented(1, $"/// Conditionally adds a scheme source to the policy.");
        source.AppendLinesIndented(1, "/// </summary>");
        source.AppendLinesIndented(1, $"/// <param name=\"conditionalFunc\">The conditional function delegate determining whether to add the nonce to the policy</param>");
        source.AppendLinesIndented(1, "/// <returns></returns>");
        source.AppendLinesIndented(1, $"public {GetClassTypeName(classSymbol)} AddSchemeSourceIf(SchemeSource schemeSource, Func<bool> conditionalFunc)");
        source.AppendLinesIndented(1, "{");
        source.AppendLinesIndented(2, $"return conditionalFunc.Invoke() ? AddSchemeSource(schemeSource) : this;");
        source.AppendLinesIndented(1, "}");

        return true;
    }


    private bool ProcessUriPolicyOptionsAttribute(INamedTypeSymbol classSymbol, StringBuilder source, bool codeAdded)
    {
        var attribute = classSymbol.GetAttributes().Where(ad => ad.AttributeClass.Name == $"{GetLongAttributeName("AddUri")}").FirstOrDefault();

        if (attribute == default)
        {
            return codeAdded;
        }

        source.AppendLinesIndented(1, "");
        source.AppendLinesIndented(1, "");

        source.AppendLinesIndented(1, "/// <summary>");
        source.AppendLinesIndented(1, $"/// Adds a uri to the policy.");
        source.AppendLinesIndented(1, "/// </summary>");
        source.AppendLinesIndented(1, "/// <returns></returns>");
        source.AppendLinesIndented(1, $"public {GetClassTypeName(classSymbol)} AddUri(string uri)");
        source.AppendLinesIndented(1, "{");
        source.AppendLinesIndented(2, "return AddValue(uri);");
        source.AppendLinesIndented(1, "}");



        source.AppendLinesIndented(1, "");
        source.AppendLinesIndented(1, "");

        source.AppendLinesIndented(1, "/// <summary>");
        source.AppendLinesIndented(1, $"/// Conditionally adds a uri to the policy.");
        source.AppendLinesIndented(1, "/// </summary>");
        source.AppendLinesIndented(1, $"/// <param name=\"conditionalFunc\">The conditional function delegate determining whether to add the nonce to the policy</param>");
        source.AppendLinesIndented(1, "/// <returns></returns>");
        source.AppendLinesIndented(1, $"public {GetClassTypeName(classSymbol)} AddUriIf(string uri, Func<bool> conditionalFunc)");
        source.AppendLinesIndented(1, "{");
        source.AppendLinesIndented(2, $"return conditionalFunc.Invoke() ? AddUri(uri) : this;");
        source.AppendLinesIndented(1, "}");

        return true;
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
