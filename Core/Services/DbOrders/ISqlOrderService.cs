using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities;

namespace Core.Services.DbOrders
{
    public interface ISqlOrderService
    {
        Task<DbOrder?> AddAsync(DbOrder bbOrder);
        Task<DbOrder?> GetOrderAsync(string id);
        Task<IEnumerable<DbOrder>> GetAllAsync();
        Task<bool> DeleteAsync(DbOrder bbOrder);
    }
}
