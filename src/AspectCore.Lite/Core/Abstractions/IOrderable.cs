using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Core
{
    /// <summary>
    /// Type implements IOrderable interface can be sorted
    /// </summary>
    public interface IOrderable : IComparable<IOrderable>
    {
        /// <summary>
        ///  Gets or sets the order in which the interceptor/async interceptor are executed.
        /// </summary>
        int Order { get; set; }
    }
}
