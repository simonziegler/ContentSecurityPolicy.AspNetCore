namespace ContentSecurityPolicy.AspNetCore;

/// <summary>
/// Base interface for a policy.
/// </summary>
internal abstract class PolicyBase
{
    /// <summary>
    /// The policy's name.
    /// </summary>
    private protected abstract string PolicyName { get; }


    /// <summary>
    /// The policy's name.
    /// </summary>
    private readonly List<string> PolicyValues = new();


    /// <summary>
    /// Returns the full policy string.
    /// </summary>
    /// <returns></returns>
    public string GetPolicy()
    {
        return $"{PolicyName}: {string.Join(' ', PolicyValues)};";
    }


    /// <summary>
    /// Adds a value to the policy.
    /// </summary>
    /// <param name="value"></param>
    internal void AddValue(string value)
    {
        PolicyValues.Add(value);
    }
}
