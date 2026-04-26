using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data;
using MoneyTracker.Models;

namespace MoneyTracker.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ver / Editar Perfil
        public async Task<IActionResult> Index()
        {

            
            var profile = await _context.DriverProfiles.FirstOrDefaultAsync();
            if (profile == null)
            {
                profile = new DriverProfile { Nombre = "Driver" };
            }
            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(DriverProfile profile)
        {

            
            if (ModelState.IsValid)
            {
                var existing = await _context.DriverProfiles.FirstOrDefaultAsync();
                
                if (existing != null)
                {
                    existing.Nombre = profile.Nombre;
                    existing.Vehiculo = profile.Vehiculo;
                    existing.Placa = profile.Placa;
                    existing.Telefono = profile.Telefono;
                    existing.Email = profile.Email;
                }
                else
                {
                    _context.DriverProfiles.Add(profile);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Perfil actualizado correctamente";
                
            }

            
            return RedirectToAction(nameof(Index));
        }
    }
}