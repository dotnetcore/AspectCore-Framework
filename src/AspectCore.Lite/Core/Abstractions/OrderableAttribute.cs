using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Core
{
    public abstract class OrderableAttribute : Attribute, IOrderable
    {
        public virtual int Order { get; set; }
        public int CompareTo(IOrderable other)
        {
            if (other == null) return 1;
            return Order.CompareTo(other.Order);
        }
    }
}
