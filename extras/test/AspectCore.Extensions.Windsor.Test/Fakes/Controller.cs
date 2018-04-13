namespace AspectCore.Extensions.Windsor.Test.Fakes
{
    public class Controller : IController
    {
        public ICacheService Service { get; }

        public Controller(ICacheService service)
        {
            Service = service;
        }

        public Model Execute()
        {
            return Service.Get(100);
        }
    }
}
