using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
	public class OrderHeader
	{
		public int Id { get; set; }
		public string ApplicationUserId { get; set; }
		[ForeignKey("ApplicationUserId")]
		[ValidateNever]
		public ApplicationUser ApplicationUser{ get; set; }

		[Required]
		public DateTime OrderDate { get; set; }
		public DateTime ShippingDate { get; set; }
		public double OrderTotal { get; set; }
		//in some scenairo what can happen people payment 30 days after delivery, 
		//so we need orderstatus and paymentstatus
		public string? OrderStatus { get; set; }
		public string? PaymentStatus { get; set; }
		public string? TrackingNumber { get; set; }
		//on which Carrier ship the order
		public string? Carrier { get; set; }

		public DateTime PaymentDate { get; set; }
		//if we taking payment after 30 days the order is delivered or shipped, in that case we populate payment due date
		public DateTime PaymentDueDate { get; set; }

		//we will use strip for payment so we need transcation Id to  map things which strip 
		public string? SessionId { get; set; }
		public string? PaymentIntentId { get; set; }


		[Required]
		public string PhoneNumber { get; set; }

		[Required]
		public string StreetAddress { get; set; }

		[Required]
		public string City { get; set; }

		[Required]
		public string State { get; set; }

		[Required]
		public string PostalCode { get; set; }

		[Required]
		public string Name { get; set; }

	}

}
