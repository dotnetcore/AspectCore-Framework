using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Extensions.DataValidation;

namespace DataAnnotations.Sample
{
    public interface IAccountService
    {
        void Register(RegisterInput input);
        string TestString(string a);
    }

    public class AccountService : IAccountService
    {
        public IDataState DataState { get; set; }
        public string TestString(string str)
        {
            return str;
        }
        public void Register(RegisterInput input)
        {
            if (DataState.IsValid)
            {
                //验证通过
                Console.WriteLine("register.. name:{0},email:{1}", input.Name, input.Email);
            }

            if (!DataState.IsValid)
            {
                //验证失败
                foreach(var error in DataState.Errors)
                {
                    Console.WriteLine("error.. key:{0},message:{1}", error.Key, error.ErrorMessage);
                }
            }
            Console.WriteLine();
        }
    }
}