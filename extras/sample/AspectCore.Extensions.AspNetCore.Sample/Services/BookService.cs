using System;
using AspectCore.Extensions.DataValidation;
using AspNetCore.Sample.Models;

namespace AspNetCore.Sample.Services
{
    public class BookService : IBookService
    {
        public IDataState DataState { get; set; }

        public void Create(CreateBookDto dto)
        {
            if (string.Equals(dto?.Author, "lemon", StringComparison.Ordinal))
            {
                DataState?.Errors?.Add(new DataValidationError("Author", "lemon向阁下问好！"));
            }
        }
    }
}
