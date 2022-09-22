﻿namespace ContentSecurityPolicy.AspNetCore;

/// <summary>
/// All policies inherit from this base class.
/// </summary>
public abstract class PolicyOptionsBase : IPolicyOptions<PolicyOptionsBase>
{
    /// <summary>
    /// The policy's name.
    /// </summary>
    private readonly List<string> PolicyValues = new();


    /// <inheritdoc/>
    public PolicyOptionsBase AddValue(string value)
    {
        PolicyValues.Add(value);
        return this;
    }
}