namespace ContentSecurityPolicy.AspNetCore;


/// <summary>
/// base-uri policy.
/// </summary>
[PolicyOptions]
[AddHashValue]
[AddHostSource]
[AddNone]
[AddNonce]
[AddReportSample]
[AddSelf]
[AddSchemeSource]
[AddStrictDynamic]
[AddUnsafeEval]
[AddUnsafeHashes]
[AddUnsafeInline]
public sealed partial class ChildSrcPolicyOptions : PolicyOptionsBase
{
}


/// <summary>
/// base-uri policy.
/// </summary>
[Policy("child-src")]
public sealed partial class ChildSrcPolicy : PolicyBase
{
}
