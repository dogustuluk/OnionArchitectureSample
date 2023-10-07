﻿using ETicaretAPI.Application.Repositories;
using ETicaretAPI.Domain.Entities.Common;
using ETicaretAPI.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ETicaretAPI.Persistence.Repositories
{
    public class ReadRepository<T> : IReadRepository<T> where T : BaseEntity
    {
        private readonly ETicaretAPIDbContext _context;

        public ReadRepository(ETicaretAPIDbContext context)
        {
            _context = context;
        }

        public DbSet<T> Table => _context.Set<T>();

        public IQueryable<T> GetAll()
            => Table;

        public IQueryable<T> GetWhere(Expression<Func<T, bool>> method)
            => Table.Where(method);

        public async Task<T> GetSingleAsync(Expression<Func<T, bool>> method)
            => await Table.FirstOrDefaultAsync(method);

        public async Task<T> GetByIdAsync(string id)
        //marker pattern kullanılır reflection kullanmak istemezsek
            /*marker pattern
             * GetByIdAsync gibi generic yapılanmalarda değersel çalışmak istiyorsak o değeri temsil eden bir arayüz veya sınıf tasarlamamız gereklidir. burda base entity'i alabiliriz bunu yapmak için, eğer IEntity kullanıyorsan bu da olur.
             */
            => await Table.FirstOrDefaultAsync(data => data.Id == Guid.Parse(id));
    }
}