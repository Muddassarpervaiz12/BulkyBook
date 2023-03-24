using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication1.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IProductRepository Product { get; }
        ICoverTypeRepository CoverType { get;}
        ICategoryRepository Category { get;}
        ICompanyRepository Company { get;}
        public void Save();
    }
}
