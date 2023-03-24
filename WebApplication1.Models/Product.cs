using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required] [Display(Name="Book Title")] public String Title { get; set; }

        [Required] public String Description { get; set; }

        [Required] public String ISBN { get; set; }

         [Required] public String Author { get; set; }

        [Required] [Range(1, 10000, ErrorMessage = "You must enter price between 0 to 1000")]
        [Display(Name ="List Price")]
        public double ListPrice { get; set; }

        //If customer buy one product then use this price variable
        [Required]
        [Range(1, 10000, ErrorMessage = "You must enter price between 0 to 1000")]
        [Display(Name = "Price for 1-50")]
        public double Price { get; set; }

        //If customer buy 50 or more then 50 book then have better price
        [Required]
        [Range(1, 10000, ErrorMessage = "You must enter price between 0 to 1000")]
        [Display(Name = "Price for 51-100")]
        public double Price50 { get; set; }

        //If customer buy 100 or more then 100 book then have better price
        [Required]
        [Range(1, 10000, ErrorMessage = "You must enter price between 0 to 1000")]
        [Display(Name = "Price for 100+")]
        public double Price100 { get; set; }

        //Display Image
        [Display(Name = "Image")]
        [ValidateNever] [Required] public String Imageurl { get; set; }

        //Foregin key of category
        [Required][Display(Name = "Category")] public int CategoryId { get; set; }
        [ForeignKey("CategoryId")] [ValidateNever] public Catogery Catogery { get; set; }

        //Foregin Key of Cover Type
        [Display(Name = "Cover Type")]
        [Required] public int CoverTypeId { get; set; }
        [ForeignKey("CoverTypeId")] [ValidateNever] public CoverType CoverType { get; set; }
    }
}
