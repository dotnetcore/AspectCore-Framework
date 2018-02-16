using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Sample.Models;

namespace AspNetCore.Sample.Services
{
    public interface IBookService
    {
        void Create(CreateBookDto dto);
    }
}
