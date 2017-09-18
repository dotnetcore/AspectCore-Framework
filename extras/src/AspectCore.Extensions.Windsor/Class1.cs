using System;

namespace AspectCore.Extensions.Windsor
{
    public class Class1
    {
        public void Foo()
        {
            Castle.Windsor.WindsorContainer windsorContainer = new Castle.Windsor.WindsorContainer();
            windsorContainer.Kernel.ComponentModelCreated += Kernel_ComponentModelCreated;
            
        }

        private void Kernel_ComponentModelCreated(Castle.Core.ComponentModel model)
        {
            //model.Interceptors.
        }
    }
}
