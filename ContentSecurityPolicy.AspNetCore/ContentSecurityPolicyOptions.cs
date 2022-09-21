using ContentSecurityPolicy.AspNetCore;
using System;
using System.Collections.Generic;

namespace ContentSecurityPolicy;


/// <summary>
/// Options for content security policy.
/// </summary>
public class ContentSecurityPolicyOptions
{
    //AddBaseUri	base-uri
    //AddChildSrc child-src
    //AddConnectSrc   connect-src
    //AddDefaultSrc	default-src
    //AddFontSrc  font-src
    //AddFormAction   form-action
    //AddFrameAncestors   frame-ancestors
    //AddFrameSrc frame-src
    //AddImgSrc   img-src
    //AddManifestSrc  manifest-src
    //AddMediaSrc media-src
    //AddNavigateTo   navigate-to
    //AddObjectSrc    object-src
    //AddPrefetchSrc  prefetch-src
    //AddReportSample report-sample
    //AddReportTo report-to
    //AddSandbox  sandbox
    //AddScriptSrc    script-src
    //AddScriptSrcAttr    script-src-attr
    //AddScriptSrcElem    script-src-elem
    //AddStrictDynamic    strict-dynamic
    //AddStyleSrc style-src
    //AddStyleSrcAttr style-src-attr
    //AddStyleSrcElem style-src-elem
    //AddTrustedTypes trusted-types
    //AddUnsafeHashes unsafe-hashes
    //AddUpgradeInsecureRequests  upgrade-insecure-requests
    //AddWorkerSrc    worker-src

    private const string BaseUriString = "base-uri";
    private const string ChildSrcString = "child-src";
    private const string ConnectSrcString = "connect-src";
    private const string DefaultSrcString = "default-src";
    private const string FontSrcString = "font-src";
    private const string FormActionString = "form-action";
    private const string FrameAncestorsString = "frame-ancestors";
    private const string FrameSrcString = "frame-src";
    private const string ImgSrcString = "img-src";
    private const string ManifestSrcString = "manifest-src";
    private const string MediaSrcString = "media-src";
    private const string NavigateToString = "navigate-to";
    private const string ObjectSrcString = "object-src";
    private const string PrefetchSrcString = "prefetch-src";
    private const string ReportSampleString = "report-sample";
    private const string ReportToString = "report-to";
    private const string SandboxString = "sandbox";
    private const string ScriptSrcString = "script-src";
    private const string ScriptSrcAttrString = "script-src-attr";
    private const string ScriptSrcElemString = "script-src-elem";
    private const string StrictDynamicString = "strict-dynamic";
    private const string StyleSrcString = "style-src";
    private const string StyleSrcAttrString = "style-src-attr";
    private const string StyleSrcElemString = "style-src-elem";
    private const string TrustedTypesString = "trusted-types";
    private const string UnsafeHashesString = "unsafe-hashes";
    private const string UpgradeInsecureRequestsString = "upgrade-insecure-requests";
    private const string WorkerSrcString = "worker-src";

    private bool BaseUri { get; set; } = false;
    private bool ChildSrc { get; set; } = false;
    private bool ConnectSrc { get; set; } = false;
    private bool DefaultSrc { get; set; } = false;
    private bool FontSrc { get; set; } = false;
    private bool FormAction { get; set; } = false;
    private bool FrameAncestors { get; set; } = false;
    private bool FrameSrc { get; set; } = false;
    private bool ImgSrc { get; set; } = false;
    private bool ManifestSrc { get; set; } = false;
    private bool MediaSrc { get; set; } = false;
    private bool NavigateTo { get; set; } = false;
    private bool ObjectSrc { get; set; } = false;
    private bool PrefetchSrc { get; set; } = false;
    private bool ReportSample { get; set; } = false;
    private bool ReportTo { get; set; } = false;
    private bool Sandbox { get; set; } = false;
    private bool ScriptSrc { get; set; } = false;
    private bool ScriptSrcAttr { get; set; } = false;
    private bool ScriptSrcElem { get; set; } = false;
    private bool StrictDynamic { get; set; } = false;
    private bool StyleSrc { get; set; } = false;
    private bool StyleSrcAttr { get; set; } = false;
    private bool StyleSrcElem { get; set; } = false;
    private bool TrustedTypes { get; set; } = false;
    private bool UnsafeHashes { get; set; } = false;
    private bool UpgradeInsecureRequests { get; set; } = false;
    private bool WorkerSrc { get; set; } = false;


    private List<PolicyBase> Policies { get; set; } = new();


    /// <summary>
    /// Pre-compressed static files are delivered in responses when set to true.
    /// </summary>
    public bool EnablePrecompressedFiles { get; set; } = true;


    /// <summary>
    /// Images are substituted for more efficient responses when set to true.
    /// </summary>
    public bool EnableImageSubstitution { get; set; } = true;


    /// <summary>
    /// Used to prioritize image formats that contain higher quality per byte, if only size should be considered remove all entries.
    /// </summary>
    public readonly Dictionary<string, float> ImageSubstitutionCostRatio = new()
    {
        { "image/bmp", 2 },
        { "image/tiff", 1 },
        { "image/gif", 1 },
        { "image/apng", 0.9f },
        { "image/png", 0.9f },
        { "image/webp", 0.9f },
        { "image/avif", 0.8f }
    };
}
