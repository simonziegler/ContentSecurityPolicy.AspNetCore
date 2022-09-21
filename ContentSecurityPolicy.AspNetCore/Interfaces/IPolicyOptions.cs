namespace ContentSecurityPolicy.AspNetCore;

/// <summary>
/// Adds 'self'.
/// </summary>
internal interface IPolicyOptions<T> where T : class, IPolicyOptions<T>
{
    /// <summary>
    /// Adds a policy value.
    /// </summary>
    /// <returns></returns>
    public T AddValue(string value);
}
