namespace MoneyTracker.Models
{
    public class Goal
    {
        public int Id { get; set; }

        
        public decimal MontoMeta { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public bool Activa { get; set; } = true;

        public string Tipo { get; set; } = "Mensual"; // Mensual, Semanal, Personalizada
    }
}