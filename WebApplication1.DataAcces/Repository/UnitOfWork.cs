using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.DataAcces;
using WebApplication1.DataAccess.Repository.IRepository;
using WebApplication1.Models;

namespace WebApplication1.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        public IProductRepository Product { get; private set; }
        public ICoverTypeRepository CoverType { get; private set; }
        public ICategoryRepository Category { get; private set; }
        public ICompanyRepository Company { get; private set; }
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Product = new ProductRepository(_db);
            CoverType = new CoverTypeRepository(_db);
            Category = new CategoryRepository(_db);
            Company = new CompanyRepository(_db);
        }

        public void Save()
        {
           _db.SaveChanges();
        }
    }
}
