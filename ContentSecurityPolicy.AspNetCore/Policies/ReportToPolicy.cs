namespace ContentSecurityPolicy.AspNetCore;


/// <summary>
/// report-to policy.
/// </summary>
[PolicyOptions]
public sealed partial class ReportToPolicyOptions : PolicyOptionsBase
{
}


/// <summary>
/// prefetch-src policy.
/// </summary>
[Policy("report-to")]
public sealed partial class ReportToPolicy : PolicyBase
{
}
