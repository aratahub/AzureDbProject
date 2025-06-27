using System;
using System.Linq;
using System.Collections.Generic;
using Entities;
using Data;
using System.Threading.Tasks;


namespace Core.Services.DbOrders
{
    public class SqlOrderService : ISqlOrderService
    {
        private readonly AppDbContext _context;

        public SqlOrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DbOrder> AddAsync(DbOrder order)
        {
            await _context.DbOrder.AddAsync(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<DbOrder?> GetOrderAsync(string id)
        {
            return await _context.DbOrder.FindAsync(id);
        }

        public async Task<IEnumerable<DbOrder>> GetAllAsync()
        {
            return await Task.FromResult(_context.DbOrder.ToList());
        }

        public async Task<bool> DeleteAsync(DbOrder dbOrder)
        {
            _context.DbOrder.Remove(dbOrder);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
