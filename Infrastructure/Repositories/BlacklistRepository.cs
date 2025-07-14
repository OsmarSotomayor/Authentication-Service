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
    public class BlacklistRepository: IBlacklistRepository
    {
        private readonly AppDbContext _context;

        public BlacklistRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsTokenBlacklistedAsync(string token)
        {
            return await _context.BlacklistedTokens
                .AnyAsync(t => t.Token == token && t.ExpirationDate > DateTime.UtcNow);
        }

        public async Task AddToBlacklistAsync(string token, DateTime expiration)
        {
            var item = new BlackListToken
            {
                Token = token,
                ExpirationDate = expiration
            };

            await _context.BlacklistedTokens.AddAsync(item);
            await _context.SaveChangesAsync();
        }
    }
}
