namespace ContentSecurityPolicy.AspNetCore;

/// <summary>
/// All policies inherit from this base class.
/// </summary>
public abstract class PolicyOptionsBase
{
    /// <summary>
    /// The policy's name.
    /// </summary>
    internal readonly List<string> PolicyValues = new();


    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="value"></param>
    ///// <returns></returns>
    //public PolicyOptionsBase AddValue(string value)
    //{
    //    PolicyValues.Add(value);
    //    return this;
    //}
}
