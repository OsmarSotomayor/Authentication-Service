using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context):base(context)
        {
            this._context = context;
        }

        public async Task CreateUser(User user)
        {
            this.Create(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            this.Update(user);
            await _context.SaveChangesAsync();
        }
        public async Task AddLoginAttemptAsync(LoginAttempt attempt)
        {
            await _context.LoginAttempts.AddAsync(attempt);
            await _context.SaveChangesAsync();
        }

        public async Task<User> GetByUsernameAsync(string username, bool track)
        {
            return await this.FindByCondition(usr => usr.Username == username, track).FirstOrDefaultAsync();
        }
    }
}
