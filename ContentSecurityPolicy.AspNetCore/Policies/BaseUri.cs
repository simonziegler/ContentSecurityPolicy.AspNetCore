using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentSecurityPolicy.AspNetCore;


/// <summary>
/// base-uri policy.
/// </summary>
internal class BaseUri : PolicyBase
{
    public class PolicyOptions : PolicyOptionsBase, IAddSelf<PolicyOptions>
    {
    }


    /// <inheritdoc/>
    private protected override string PolicyName => "base-uri";


    public BaseUri(Func<PolicyOptions, PolicyOptions> options)
    {

    }


    /// <inheritdoc/>
    string PolicyBase.GetPolicy()
    {
        throw new NotImplementedException();
    }

}
