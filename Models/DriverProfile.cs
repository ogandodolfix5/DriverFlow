using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Models
{
    public class DriverProfile
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nombre Completo")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Vehículo")]
        public string? Vehiculo { get; set; }

        [Display(Name = "Placa")]
        public string? Placa { get; set; }

        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Display(Name = "Correo")]
        public string? Email { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Today;

        public bool Activo { get; set; } = true;
    }
}