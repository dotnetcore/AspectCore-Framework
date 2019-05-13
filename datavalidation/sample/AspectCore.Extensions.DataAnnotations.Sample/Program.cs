using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AspectCore.Extensions.DataAnnotations;
using AspectCore.Injector;

namespace DataAnnotations.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            //var target = new RegisterInput { Name = "lemon", Email = "lemon@lemon.com", Password = "*******" };
            //var validationResults = new List<ValidationResult>();
            //var context = new ValidationContext(target, null, null);
            //var isValid = Validator.TryValidateObject(target, context, validationResults, true);
            
            var services = new ServiceContainer();        
            services.AddType<IAccountService, AccountService>();

            services.AddDataAnnotations();

            var serviceResolver = services.Build();
            var accountService = serviceResolver.Resolve<IAccountService>();

            var result = accountService.TestString("test");
            accountService.Register(new RegisterInput { Name = null, Email = null });
            accountService.Register(new RegisterInput { Name = "lemon", Email = "lemon", Password = "****" });
            accountService.Register(new RegisterInput { Name = "lemon", Email = "lemon@lemon.com", Password = "****" });
            accountService.Register(new RegisterInput { Name = "lemon", Email = "lemon@lemon.com", Password = "*******" });

            Console.ReadKey();
        }
    }
}
