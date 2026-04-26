using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyTracker.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Fecha")]
        public DateTime Fecha { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Aplicación")]
        public string Aplicacion { get; set; } = string.Empty;

        [Display(Name = "Tanda Horaria")]
        public string TandaHoraria { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Monto Bruto")]
        public decimal MontoBruto { get; set; }

        [Display(Name = "Gasolina")]
        public decimal Gasolina { get; set; } = 0;

        [Display(Name = "Mantenimiento")]
        public decimal Mantenimiento { get; set; } = 0;

        [Display(Name = "Comida")]
        public decimal Comida { get; set; } = 0;

        [Display(Name = "Otros Gastos")]
        public decimal OtrosGastos { get; set; } = 0;

        [Display(Name = "Tips")]
        public decimal Tips { get; set; } = 0;

        [Display(Name = "Notas / Incidencias")]
        public string? Notas { get; set; }

        // ==================== CALCULADOS COMO MÉTODOS (Solución definitiva) ====================
        public decimal GetTotalGastos() => Gasolina + Mantenimiento + Comida + OtrosGastos;

        public decimal GetNeto() => MontoBruto + Tips - GetTotalGastos();
        // ===================================================================================
    }
}