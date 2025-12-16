using Inventory.DataAccess.Repository;
using Inventory.DataAccess.Repository.IRepository;
using Inventory.Models;
using Inventory.Models.ViewModels;
using Inventory.Utility;

using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace InventoryWeb.Controllers
{

    public class NotificationController : Controller
    {


        private readonly INotificationRepository _notificationRepository;
        private readonly IProductRepository _productRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserNotificationRepository _userNotificationRepository;


        public NotificationController(INotificationRepository db, IProductRepository db1, UserManager<ApplicationUser> userManager, IUserNotificationRepository userNotificationRepository)
        {
            _notificationRepository = db;
            _productRepository = db1;
            _userManager = userManager;
             _userNotificationRepository = userNotificationRepository;
        }


        // All notifications, admin
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Index()
        {
            IEnumerable<Notification> notificationEnumerable = await _notificationRepository.GetAll(includeProperties: "Product");

            List<Notification> notificationList = notificationEnumerable.ToList();

            return View(notificationList);
        }




        public async Task<IActionResult> CreateReport()
        {
            IQueryable<Notification> query = _notificationRepository
                .GetAllQueryable(includeProperties: "Product");

            List<Notification> notifications = await query
                .OrderBy(n => n.Product.Stock)
                .ToListAsync();

            // Cross-platform IST timezone
            TimeZoneInfo istZone =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")
                    : TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

            DateTime istNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);
            string generatedAt = istNow.ToString("dd MMM yyyy - hh:mm:ss tt") + " (IST)";

            using MemoryStream ms = new MemoryStream();

            Document doc = new Document(PageSize.A4, 25, 25, 40, 40);
            PdfWriter writer = PdfWriter.GetInstance(doc, ms);
            writer.PageEvent = new PdfFooter(generatedAt);

            doc.Open();

            // ---------- CENTERED HEADER ----------
            Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);

            Paragraph p1 = new Paragraph("GOVERNMENT OF INDIA", headerFont)
            {
                Alignment = Element.ALIGN_CENTER
            };
            Paragraph p2 = new Paragraph("NATIONAL INFORMATICS CENTRE - PUDUCHERRY", headerFont)
            {
                Alignment = Element.ALIGN_CENTER
            };
            Paragraph p3 = new Paragraph("INVENTORY - LOW STOCK REPORT", titleFont)
            {
                Alignment = Element.ALIGN_CENTER
            };

            doc.Add(p1);
            doc.Add(p2);
            doc.Add(p3);
            LineSeparator line = new LineSeparator
            {
                LineWidth = 1f,
                Percentage = 100f
            };

            doc.Add(new Chunk(line));
            doc.Add(new Paragraph("\n"));

            // ---------- TABLE ----------
            PdfPTable table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 10, 30, 10, 15, 20 });

            Font headerCellFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

            table.AddCell(new PdfPCell(new Phrase("Product ID", headerCellFont)));
            table.AddCell(new PdfPCell(new Phrase("Product Name", headerCellFont)));
            table.AddCell(new PdfPCell(new Phrase("Stock", headerCellFont)));
            table.AddCell(new PdfPCell(new Phrase("Price", headerCellFont)));
            table.AddCell(new PdfPCell(new Phrase("Time Created (IST)", headerCellFont)));

            foreach (Notification n in notifications)
            {
                DateTime istTime = TimeZoneInfo.ConvertTimeFromUtc(n.TimeCreated, istZone);

                table.AddCell(n.Product.Id.ToString());
                table.AddCell(n.Product.Name);
                table.AddCell(n.Product.Stock.ToString());
                table.AddCell(n.Product.Price.ToString("₹0.00"));
                table.AddCell(istTime.ToString("dd-MMM-yyyy hh:mm tt"));
            }

            doc.Add(table);
            doc.Close();

            string fileName = $"LowStockReport_{istNow:dd-MMM-yyyy_hh-mm-tt}.pdf";

            return File(ms.ToArray(), "application/pdf", fileName);
        }






        // Number on top of Bell icon
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            string? userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            int unreadCount = await _notificationRepository.GetUnreadNotificationCountAsync(userId);

            return Ok(new { unreadCount });
        }




        // Unread notification from clicking the bell icon
        [Authorize]
        public async Task<IActionResult> GetNotifications()
        {
            string? userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var notifications = await _notificationRepository.GetUnreadNotificationsAsync(userId);

            return View(notifications);
        }








        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken] // required for forms/anti-forgery
        public async Task<IActionResult> DismissNotification(int id)
        {
            string? userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            await _userNotificationRepository.DismissNotificationAsync(id, userId);

            return Ok();
        }







        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public async Task<IActionResult> DeleteNotification([FromBody] DeleteNotification request)
        {

            Console.WriteLine("Inside deleteNotification");
            
            if (request == null || request.NotificationId <= 0)
            {
                return BadRequest(new { message = "Invalid request." });
            }

            Notification? notificationFromDb = await _notificationRepository.Get(i => i.Id == request.NotificationId);

            if (notificationFromDb == null)
            {
                return BadRequest(new { message = "Notification not found." });
            }

            _notificationRepository.Remove(notificationFromDb);
            await _notificationRepository.Save();

            return Ok(new { success = true, message = "Notification deleted successfully." });
        }



    }
}
