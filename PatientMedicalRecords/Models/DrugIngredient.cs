using PatientMedicalRecords.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PatientMedicalRecords.Models
{
    public class DrugIngredient
    {
        [Key] public int Id { get; set; }

        [Required] public int DrugId { get; set; }
        [Required] public int IngredientId { get; set; }

        [ForeignKey("DrugId")]
        public virtual Drug Drug { get; set; }

        [ForeignKey("IngredientId")]
        public virtual Ingredient Ingredient { get; set; }
    }
      

}

//public class DrugIngredient
//{
//    [Key]
//    public int DrugId { get; set; }
//    public virtual Drug? Drug { get; set; }

//    public int IngredientId { get; set; }
//    public virtual Ingredient? Ingredient { get; set; }
//}