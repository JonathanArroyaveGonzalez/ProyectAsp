
using System.ComponentModel.DataAnnotations;

namespace DB_MVC.Models

{
    public class BicicletaModel
    {
        public int Id { get; set; }


        [Required(ErrorMessage = "La placa es obligatoria")]
        [StringLength(10, ErrorMessage = "La placa no puede exceder 10 caracteres")]
        public string Placa { get; set; }

        [Required(ErrorMessage = "La marca es obligatoria")]
        [StringLength(50, ErrorMessage = "La marca no puede exceder 50 caracteres")]
        public string Marca { get; set; }
    }
}
