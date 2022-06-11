using System.Linq;
using Domain.Persistables;
using Microsoft.EntityFrameworkCore;

namespace DAL.Queries
{
    
    public static class UserQueries
    {
        public static User GetByLogin(this DbSet<User> users, string login)
        {
            return users.FirstOrDefault(x => x.Login == login);
        }
    }
}