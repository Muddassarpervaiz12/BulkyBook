using Microsoft.AspNetCore.Routing.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication1.Models.ViewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> ListCart { get; set; }
        //i comment this because in orderheader we have price so use order header 
        //public double CartTotal { get; set; }
        public OrderHeader OrderHeader { get; set; }
    }
}
