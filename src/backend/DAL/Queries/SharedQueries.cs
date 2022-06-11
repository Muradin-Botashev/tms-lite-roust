using System;
using System.Linq;
using Domain.Persistables;
using Microsoft.EntityFrameworkCore;

namespace DAL.Queries
{
    public static class SharedQueries
    {
        public static T GetById<T>(this IQueryable<T> users, Guid id) where T : class, IPersistable
        {
            return users.FirstOrDefault(x => x.Id == id);
        }
        
    }
}