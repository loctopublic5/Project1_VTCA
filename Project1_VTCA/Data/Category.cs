using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project1_VTCA.Data;

namespace Project1_VTCA.Data
{
    [Table("Categories")]
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required] 
        [StringLength(100)]
        public string Name { get; set; }

        public int? ParentID { get; set; }

        [ForeignKey("ParentID")] 
        public Category ParentCategory { get; set; }

        [Required]
        [StringLength(20)]
        public string CategoryType { get; set; }

        public bool IsPromotion { get; set; }

        public ICollection<Category> ChildCategories { get; set; }
        public ICollection<ProductCategory> ProductCategories { get; set; }
    }
}
