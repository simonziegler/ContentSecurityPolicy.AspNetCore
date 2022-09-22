namespace ContentSecurityPolicy.AspNetCore;


/// <summary>
/// base-uri policy.
/// </summary>
[PolicyOptions]
[AddSelf]
public sealed partial class BaseUriPolicyOptions : PolicyOptionsBase
{
}


/// <summary>
/// base-uri policy.
/// </summary>
[Policy("base-uri")]
public sealed partial class BaseUriPolicy : PolicyBase
{
}
