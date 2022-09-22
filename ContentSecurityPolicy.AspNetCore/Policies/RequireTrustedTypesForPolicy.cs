﻿namespace ContentSecurityPolicy.AspNetCore;


/// <summary>
/// require-trusted-types-for policy - considered deprecated.
/// </summary>
[PolicyOptions]
[AddScript]
public sealed partial class RequireTrustedTypesForPolicyOptions : PolicyOptionsBase
{
}


/// <summary>
/// require-trusted-types-for policy - considered deprecated.
/// </summary>
[Policy("require-trusted-types-for")]
public sealed partial class RequireTrustedTypesForPolicy : PolicyBase
{
}
