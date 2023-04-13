using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader obj);
        //Update order status in which have id and order status and payment status, payment status is nullable 
        // because its possible we do not update payment status every time
        void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);
        
    }
}
