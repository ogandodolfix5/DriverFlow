using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data;
using MoneyTracker.Models;
using System.Text;
using MoneyTracker.Services;           // ← Importante
using Microsoft.AspNetCore.Authorization;

namespace MoneyTracker.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;   // ← Inyectado

        public ReportsController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // ====================== VISTA CON FILTROS ======================
        public async Task<IActionResult> Index(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var query = _context.Transactions.AsQueryable();

            if (fechaDesde.HasValue) query = query.Where(t => t.Fecha.Date >= fechaDesde.Value.Date);
            if (fechaHasta.HasValue) query = query.Where(t => t.Fecha.Date <= fechaHasta.Value.Date);

            var transactions = await query.OrderByDescending(t => t.Fecha).ToListAsync();

            ViewBag.TotalIngresos = transactions.Sum(t => t.MontoBruto + t.Tips);
            ViewBag.TotalGastos = transactions.Sum(t => t.GetTotalGastos());
            ViewBag.TotalNeto = transactions.Sum(t => t.GetNeto());
            ViewBag.CantidadTransacciones = transactions.Count;

            ViewBag.FechaDesde = fechaDesde?.ToString("yyyy-MM-dd");
            ViewBag.FechaHasta = fechaHasta?.ToString("yyyy-MM-dd");

            ViewBag.PorAplicacion = transactions.GroupBy(t => t.Aplicacion)
                .Select(g => new { Aplicacion = g.Key ?? "Sin aplicación", Ingresos = g.Sum(t => t.MontoBruto + t.Tips), Gastos = g.Sum(t => t.GetTotalGastos()), Neto = g.Sum(t => t.GetNeto()), Cantidad = g.Count() })
                .OrderByDescending(x => x.Neto).ToList();

            ViewBag.PorTanda = transactions.GroupBy(t => t.TandaHoraria)
                .Select(g => new { Tanda = g.Key ?? "Sin tanda", Neto = g.Sum(t => t.GetNeto()), Cantidad = g.Count() })
                .OrderByDescending(x => x.Neto).ToList();

            return View(transactions);
        }

        // ====================== DESCARGAR CSV ======================
        public async Task<IActionResult> ExportCsv(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var query = _context.Transactions.AsQueryable();

            if (fechaDesde.HasValue) query = query.Where(t => t.Fecha.Date >= fechaDesde.Value.Date);
            if (fechaHasta.HasValue) query = query.Where(t => t.Fecha.Date <= fechaHasta.Value.Date);

            var transactions = await query.OrderByDescending(t => t.Fecha).ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Fecha,Aplicacion,Tanda,MontoBruto,Gasolina,Mantenimiento,Comida,OtrosGastos,Tips,TotalGastos,Neto,Notas");

            foreach (var t in transactions)
            {
                sb.AppendLine($"{t.Fecha:dd/MM/yyyy},{t.Aplicacion},{t.TandaHoraria},{t.MontoBruto}," +
                             $"{t.Gasolina},{t.Mantenimiento},{t.Comida},{t.OtrosGastos},{t.Tips}," +
                             $"{t.GetTotalGastos()},{t.GetNeto()},\"{t.Notas?.Replace("\"", "\"\"")}\"");
            }

            var fileName = $"Reporte_DriverCash_{DateTime.Today:yyyy-MM-dd}.csv";
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", fileName);
        }

        // ====================== ENVIAR REPORTE POR CORREO ======================
        [HttpPost]
        public async Task<IActionResult> SendReport()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "No se pudo identificar el usuario.";
                    return RedirectToAction("Index");
                }

                await _emailService.SendReportAsync(userId, "Mensual");

                TempData["Success"] = "✅ Reporte enviado correctamente a tu correo electrónico.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Error al enviar el reporte: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}