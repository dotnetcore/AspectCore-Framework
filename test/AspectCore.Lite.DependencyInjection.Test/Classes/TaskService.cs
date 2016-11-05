using System;

namespace AspectCore.Lite.DependencyInjection.Test.Classes
{
    public class TaskService:ITaskService
    {
        public ILogger logger { get; set; }

        public TaskService(ILogger logger)
        {
            this.logger = logger;
        }

        public void Run()
        {
            Console.WriteLine("task run");
        }
    }
}