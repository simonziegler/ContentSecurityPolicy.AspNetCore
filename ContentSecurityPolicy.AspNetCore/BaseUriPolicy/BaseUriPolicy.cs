namespace ContentSecurityPolicy.AspNetCore.BaseUriPolicy;


/// <summary>
/// base-uri policy.
/// </summary>
internal sealed partial class BaseUriPolicy : PolicyBase
{
    /// <inheritdoc/>
    private protected override string PolicyName => "base-uri";


    public BaseUriPolicy(Func<BaseUriPolicyOptions, BaseUriPolicyOptions> options)
    {

    }
}
