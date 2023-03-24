﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.DataAcces;
using WebApplication1.DataAccess.Repository.IRepository;
using WebApplication1.Models;

namespace WebApplication1.DataAccess.Repository
{
    public class CoverTypeRepository : Repository<CoverType>, ICoverTypeRepository
    {
        private readonly ApplicationDbContext _db;
        public  CoverTypeRepository(ApplicationDbContext db) :base(db)
        {
            _db = db;
        }
        public void Update(CoverType obj)
        {
            _db.coverTypes.Update(obj);
        }
    }
}
