using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication1.Utility
{
    public static class SD
    {
        //here we have static details
        public const string Role_User_Indi = "Individual";
        public const string Role_User_Comp = "Company";
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "Employee";

		// when order is created
		public const string StatusPending = "Pending";

		//when payment is approve then status get approved
		public const string StatusApproved = "Approved";

		//In Process is updated by admin when they are processing the order
		public const string StatusInProcess = "Processing";

		//after proccess the order this is the final status that is shipped
		public const string StatusShipped = "Shipped";

		//cancel and refund is if customer cancel the order or want to refund.
		public const string StatusCancelled = "Cancelled";
		public const string  StatusRefunded = "Refunded";


		//initally payment is pending
		public const string PaymentStatusPending = "Pending";

		//once payment done its approve
		public const string PaymentStatusApproved = "Approved";

		//if company account then this will have 30 days to make the paayment after order is shippied
		public const string PaymentStatusDelayPayment = "ApprovedForDelayPayment";

		//if reject payment then status is rejected
		public const string PaymentStatusRejected = "Rejected";

	}
}
