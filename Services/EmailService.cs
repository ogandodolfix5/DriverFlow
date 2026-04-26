using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using MoneyTracker.Data;
using Microsoft.EntityFrameworkCore;

namespace MoneyTracker.Services
{
    public class EmailService
    {
        private readonly EmailSettings _settings;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public EmailService(IOptions<EmailSettings> settings, 
                           UserManager<IdentityUser> userManager, 
                           ApplicationDbContext context)
        {
            _settings = settings.Value;
            _userManager = userManager;
            _context = context;
        }

        // Método público para enviar reporte
       public async Task SendReportAsync(string userId, string reportType = "Mensual")
{
    var user = await _userManager.FindByIdAsync(userId);
    if (user == null || string.IsNullOrEmpty(user.Email)) 
        return;

    var transactions = await _context.Transactions
        .OrderByDescending(t => t.Fecha)
        .ToListAsync();

    var totalIngresos = transactions.Sum(t => t.MontoBruto + t.Tips);
    var totalGastos = transactions.Sum(t => t.GetTotalGastos());
    var netoTotal = transactions.Sum(t => t.GetNeto());
    var margen = totalIngresos > 0 ? (netoTotal / totalIngresos) * 100 : 0;
    var extra = netoTotal - totalIngresos; // Si se pasó de la meta

    var body = $@"
    <div style='font-family: Arial, sans-serif; max-width: 700px; margin: 0 auto; background: #f9f9f9; padding: 20px;'>
        
        <div style='background: linear-gradient(135deg, #198754, #0d6efd); color: white; padding: 40px 20px; text-align: center; border-radius: 12px 12px 0 0;'>
            <h1 style='margin: 0; font-size: 42px;'>🎉 FELICIDADES 🎉</h1>
            <h2 style='margin: 10px 0 0 0;'>¡Has cumplido tu meta!</h2>
        </div>

        <div style='background: white; padding: 40px 30px; border-radius: 0 0 12px 12px;'>
            
            <p style='font-size: 18px;'>Hola, <strong>{user.Email}</strong></p>
            
            <div style='background: #f0f9f0; padding: 25px; border-radius: 10px; text-align: center; margin: 25px 0;'>
                <h3 style='color: #198754; margin: 0;'>Meta Alcanzada</h3>
                <h1 style='color: #198754; font-size: 48px; margin: 10px 0;'>{netoTotal.ToString("C")}</h1>
                <p style='color: #198754; font-size: 18px;'>¡Excelente trabajo!</p>
            </div>

            {(extra > 0 ? $@"
            <div style='background: #e6f7ff; padding: 20px; border-radius: 10px; text-align: center; margin: 20px 0;'>
                <h4 style='color: #0d6efd; margin: 0;'>¡Te pasaste de la meta!</h4>
                <h2 style='color: #0d6efd; margin: 8px 0;'>+{extra.ToString("C")}</h2>
                <p>¡Increíble esfuerzo!</p>
            </div>" : "")}

            <table style='width: 100%; border-collapse: collapse; margin: 25px 0;'>
                <tr>
                    <td style='padding: 12px 0; border-bottom: 1px solid #eee;'>Ingresos Totales</td>
                    <td style='padding: 12px 0; text-align: right; color: #198754; font-weight: bold;'>{totalIngresos.ToString("C")}</td>
                </tr>
                <tr>
                    <td style='padding: 12px 0; border-bottom: 1px solid #eee;'>Gastos Totales</td>
                    <td style='padding: 12px 0; text-align: right; color: #dc3545; font-weight: bold;'>-{totalGastos.ToString("C")}</td>
                </tr>
                <tr style='font-weight: bold; background: #f8f9fa;'>
                    <td style='padding: 15px 0;'>Utilidad Neta</td>
                    <td style='padding: 15px 0; text-align: right; color: #198754;'>{netoTotal.ToString("C")}</td>
                </tr>
            </table>

            <div style='text-align: center; margin: 30px 0;'>
                <p style='font-size: 18px; color: #198754;'><strong>Tu margen operativo fue del {margen:F1}%</strong></p>
            </div>

            <p style='text-align: center; font-size: 16px;'>
                ¡Sigue así! Establece una meta aún más alta y sigue superándote.
            </p>
        </div>

        <div style='text-align: center; padding: 20px; font-size: 0.9em; color: #666;'>
            Este mensaje fue generado automáticamente por <strong>DriverCash</strong><br>
            Gracias por usar nuestra plataforma
        </div>
    </div>";

    await SendEmailAsync(user.Email, "🎉 ¡Meta Cumplida! Felicidades", body);
}

        // Método público para enviar cualquier correo
        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, false);
            await smtp.AuthenticateAsync(_settings.SenderEmail, _settings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }

    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}