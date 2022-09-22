namespace ContentSecurityPolicy.AspNetCore;


/// <summary>
/// Base for all CSP policy attributes.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public abstract class PolicyBaseAttribute : Attribute
{
    /// <summary>
    /// The name of the function added to the class decorated with this attribute.
    /// </summary>
    public abstract string FunctionName { get; }


    /// <summary>
    /// The policy value returned by the function added to the class decorated with this attribute.
    /// </summary>
    public abstract string PolicyValue { get; }
}
