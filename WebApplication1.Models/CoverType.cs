using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class CoverType
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Display(Name="Cover Type Name")]
        [MaxLength(50)]
        public String Name { get; set; }
    }
}
