namespace AspectCore.Extensions.Windsor.Test.Fakes
{
    public class Controller : IController
    {
        public IService Service { get; }

        public Controller(IService service)
        {
            Service = service;
        }

        public Model Execute()
        {
            return Service.Get(100);
        }
    }
}
