using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data;
using MoneyTracker.Models;
using Microsoft.AspNetCore.Authorization;

namespace MoneyTracker.Controllers
{
    [Authorize]
    public class GoalsController : Controller
    {
        private readonly ApplicationDbContext _context;



        public GoalsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        [Authorize]
        public async Task<IActionResult> SetGoal(Goal goal)
        {
            if (ModelState.IsValid)
            {
                // Desactivar metas anteriores
                var oldGoals = await _context.Goals.Where(g => g.Activa).ToListAsync();
                foreach (var g in oldGoals)
                {
                    g.Activa = false;
                }

                goal.Activa = true;
                _context.Goals.Add(goal);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Meta guardada correctamente";
                // Redirigir al Index para que se actualice todo

            }

            // Si algo falla, igual recargamos el Index
            return RedirectToAction("Index", "Home");
        }
    }
}