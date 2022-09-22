namespace ContentSecurityPolicy.AspNetCore;


/// <summary>
/// Creates an <c>AddSelf()</c> generated function.
/// </summary>
public sealed class AddSelfAttribute : PolicyBaseAttribute
{
    /// <inheritdoc/>
    public override string FunctionName => "AddSelf";


    /// <inheritdoc/>
    public override string PolicyValue => "'self'";
}
