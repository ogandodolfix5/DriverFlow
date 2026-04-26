using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MoneyTracker.Data;
using MoneyTracker.Models;

namespace MoneyTracker.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var transactions = await _context.Transactions
                .OrderByDescending(t => t.Fecha)
                .Take(15)
                .ToListAsync();

            // Resumen del día
            var hoy = DateTime.Today;
            var transHoy = transactions.Where(t => t.Fecha.Date == hoy).ToList();

            ViewBag.IngresosHoy = transHoy.Sum(t => t.MontoBruto + t.Tips);
            ViewBag.GastosHoy = transHoy.Sum(t => t.GetTotalGastos());
            ViewBag.NetoHoy = transHoy.Sum(t => t.GetNeto());

            // Total Neto General
            ViewBag.TotalNeto = transactions.Sum(t => t.GetNeto());

            // Perfil del Driver
            var profile = await _context.DriverProfiles.FirstOrDefaultAsync();
            ViewBag.DriverName = profile?.Nombre ?? User.Identity?.Name ?? "Driver";

            // Meta
            var metaActual = await _context.Goals
                .FirstOrDefaultAsync(g => g.Activa);

            if (metaActual != null && metaActual.MontoMeta > 0)
            {
                var progreso = Math.Round((ViewBag.TotalNeto / metaActual.MontoMeta) * 100, 1);

                ViewBag.Meta = metaActual;
                ViewBag.MetaActual = metaActual;
                ViewBag.ProgresoMeta = progreso;

                ViewBag.MensajeMeta = progreso switch
                {
                    >= 90 => "🎉 ¡Estás muy cerca de lograr tu meta!",
                    >= 70 => "🔥 Excelente ritmo, ¡sigue así!",
                    >= 50 => "💪 Vas por buen camino",
                    >= 25 => "📈 Comenzando fuerte",
                    _ => "🌱 Es el momento de acelerar"
                };
            }
            else
            {
                ViewBag.Meta = null;
                ViewBag.MetaActual = null;
                ViewBag.ProgresoMeta = 0;
                ViewBag.MensajeMeta = null;
            }

            // Análisis Inteligente
            if (transactions.Any())
            {
                var mejorApp = transactions.GroupBy(t => t.Aplicacion)
                    .Select(g => new { Aplicacion = g.Key, Neto = g.Sum(t => t.GetNeto()) })
                    .OrderByDescending(x => x.Neto)
                    .FirstOrDefault();

                var mejorTanda = transactions.GroupBy(t => t.TandaHoraria)
                    .Select(g => new { Tanda = g.Key, Neto = g.Sum(t => t.GetNeto()) })
                    .OrderByDescending(x => x.Neto)
                    .FirstOrDefault();

                ViewBag.MejorAplicacion = mejorApp?.Aplicacion ?? "Sin datos";
                ViewBag.MejorTanda = mejorTanda?.Tanda ?? "Sin datos";
            }

            return View(transactions);
        }

        // ====================== MÉTODO ELIMINAR ======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarPorAplicacion(string Aplicacion)
        {
            if (string.IsNullOrWhiteSpace(Aplicacion))
            {
                TempData["Error"] = "Nombre de aplicación no válido.";
                return RedirectToAction("Index");
            }

            try
            {
                var registros = await _context.Transactions
                    .Where(x => x.Aplicacion == Aplicacion)
                    .ToListAsync();

                if (registros.Any())
                {
                    _context.Transactions.RemoveRange(registros);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"✅ Se eliminaron {registros.Count} registros de {Aplicacion}.";
                }
                else
                {
                    TempData["Info"] = $"No se encontraron registros para {Aplicacion}.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Error al eliminar: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        public IActionResult About()
        {
            return View();
        }
    }
}