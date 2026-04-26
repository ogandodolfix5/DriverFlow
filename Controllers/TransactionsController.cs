using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data;
using MoneyTracker.Models;
using Microsoft.AspNetCore.Authorization;
using MoneyTracker.Services;

namespace MoneyTracker.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Mostrar formulario (opcional)
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Guardar transacción
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                _context.Add(transaction);
                await _context.SaveChangesAsync();

                // ==================== VERIFICAR SI SE CUMPLIÓ LA META ====================
                await CheckAndNotifyGoalCompletion();

                return RedirectToAction("Index", "Home");
            }

            return View(transaction);
        }
        // ====================== ELIMINAR TRANSACCIÓN ======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Delete(int id, string? returnUrl = null)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }

            // Si viene desde Reportes, regresa a Reportes
            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("/Reports"))
            {
                return Redirect(returnUrl);
            }

            // Por defecto regresa a Inicio
            return RedirectToAction("Index", "Home");
        }
        private async Task CheckAndNotifyGoalCompletion()
        {
            var meta = await _context.Goals.FirstOrDefaultAsync(g => g.Activa);
            if (meta == null || meta.MontoMeta <= 0) return;

            var totalNeto = await _context.Transactions.SumAsync(t => t.MontoBruto + t.Tips - (t.Gasolina + t.Mantenimiento + t.Comida + t.OtrosGastos));

            if (totalNeto >= meta.MontoMeta)
            {
                var extra = totalNeto - meta.MontoMeta;

                var emailService = HttpContext.RequestServices.GetRequiredService<EmailService>();

                var body = $@"
            <h2>🎉 ¡FELICIDADES! Has cumplido tu meta</h2>
            <p>Hola, <strong>{User.Identity?.Name ?? "Driver"}</strong></p>
            <p>Has alcanzado tu meta mensual de <strong>{meta.MontoMeta.ToString("C")}</strong>.</p>
            
            <h3 style='color: #198754;'>Total Generado: {totalNeto.ToString("C")}</h3>
            
            {(extra > 0 ? $"<p>¡Incluso te pasaste por <strong>{extra.ToString("C")}</strong>! Increíble trabajo 🔥</p>" : "")}
            
            <p>Gracias por tu esfuerzo. Sigue así y establece una meta aún más alta.</p>
            <hr>
            <small>DriverCash - Tu compañero financiero</small>";

                await emailService.SendEmailAsync(
                    User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "",
                    "🎉 ¡Meta Cumplida! Felicidades",
                    body
                );

                // Opcional: Desactivar la meta una vez cumplida
                // meta.Activa = false;
                // await _context.SaveChangesAsync();
            }
        }
    }
}