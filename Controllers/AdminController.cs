using Laundry.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System.Drawing;
using System.Linq;
using ZXing.Common;
using ZXing;
using System.Drawing.Imaging;
using System.Net.Mail;
using System.Net;
using System.Runtime.InteropServices.Marshalling;

namespace Laundry.Controllers
{
    [AdminFilter]
    public class AdminController : Controller
    {
        private readonly MyDbContext myDbContext;
        private readonly IWebHostEnvironment webHostEnvironment;

        public AdminController(MyDbContext myDbContext, IWebHostEnvironment webHostEnvironment)

        {
            this.myDbContext = myDbContext;
            this.webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            List<Payment> records = await myDbContext.Payments
                .Include(x => x.Booking)
                .ThenInclude(x => x.User)
                .Include(x => x.Booking)
                .ThenInclude(x => x.BookingClothes)
                .ThenInclude(x => x.Clothes)
                .Include(x => x.Booking)
                .ThenInclude(x => x.BookingClothes)
                .ThenInclude(x => x.Services)
                .Take(5)
                .ToListAsync();

            decimal totalRevenue = myDbContext.Payments.Sum(x => x.Amount);
            ViewBag.totalRevenue = totalRevenue;
            int totalBookings = myDbContext.BookingClothes.Count();
            ViewBag.totalBookings = totalBookings;
            return View(records);
        }
        public async Task<IActionResult> Customers()
        {
            List<Customer> customer = await myDbContext.Customers.ToListAsync();
            //return Json(customer);
            return View(customer);
        }

        public async Task<JsonResult> SearchUser(string search)
        {
            //return Json(search);
            List<User> user = await myDbContext.Users
                        .Where(x => x.Name.Contains(search))
                        .ToListAsync();

            if (user != null && user.Count > 0)
            {
                return Json(new { success = true, user = user });
            }
            return Json(new { success = false });

        }

        public IActionResult AddCustomer()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCustomer(Customer customer)
        {
            if (ModelState.IsValid)
            {
                await myDbContext.Customers.AddAsync(customer);

                await myDbContext.SaveChangesAsync();

                TempData["success"] = "Customer Added Successfully";

                return RedirectToAction("Index");
            }
            return View();
        }

        public async Task<IActionResult> EditCustomer(int id)
        {
            Customer customer = await myDbContext.Customers.FindAsync(id);
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> EditCustomer(Customer customer)
        {
            if (ModelState.IsValid)
            {
                myDbContext.Customers.Update(customer);
                await myDbContext.SaveChangesAsync();
                TempData["success"] = "Customer update successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        public async Task<IActionResult> DeleteCustomer(int id)
        {
            Customer customer = await myDbContext.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound("Customer not found");
            }
            myDbContext.Customers.Remove(customer);
            await myDbContext.SaveChangesAsync();
            TempData["success"] = "Customer delete successfully";
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Services()
        {
            var services = await myDbContext.Services.OrderByDescending(x => x.Id).ToListAsync();
            return View(services);
        }
        public IActionResult AddServices()
        {
            return View();

        }
        [HttpPost]

        public async Task<IActionResult> AddServices(Services services, IFormFile Image)
        {
            var checkService = await myDbContext.Services.AnyAsync(x => x.Name == services.Name);
            if (checkService)
            {
                TempData["error"] = "Service already exists";
                return View();
            }
            if (Image == null)
            {
                ModelState.AddModelError("Image", "Image is Required");
                return View();
            }
            if (!ModelState.IsValid)
            {
                if (!Directory.Exists(Path.Combine(webHostEnvironment.WebRootPath, "services")))
                {
                    Directory.CreateDirectory(Path.Combine(webHostEnvironment.WebRootPath, "services"));
                }
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var fileExtension = Path.GetExtension(Image.FileName).ToLower();

                var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp" };
                var mimeType = Image.ContentType.ToLower();

                if (!allowedExtensions.Contains(fileExtension) || !allowedMimeTypes.Contains(mimeType))
                {
                    TempData["error"] = "Only JPG, PNG, and WEBP formats are allowed!";
                    return View();
                }
                var path = Path.Combine(webHostEnvironment.WebRootPath, "services", Image.FileName);
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    Image.CopyTo(fs);
                }
                services.Image = Image.FileName;
                await myDbContext.Services.AddAsync(services);
                await myDbContext.SaveChangesAsync();
                TempData["success"] = "Services Add Successfully";
                return RedirectToAction("Services");
            }
            return View();

        }

        public async Task<IActionResult> Clothes()
        {
            List<Clothes> clothes = await myDbContext.Clothes.Include(x => x.ClothesServices).ThenInclude(x => x.Services).OrderByDescending(x => x.Id).ToListAsync();
            //return Json(clothes);
            return View(clothes);
        }

        public async Task<IActionResult> AddClothes()
        {
            var services = await myDbContext.Services.ToListAsync();

            ViewBag.Services = services;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddClothes(ClothesViewModel model, IFormFile Image)
        {
            var services = await myDbContext.Services.ToListAsync();

            ViewBag.Services = services;
            if (model.SelectedServiceIds == null)
            {
                TempData["error"] = "At least one service is required";
                return View();
            }
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (!Directory.Exists(Path.Combine(webHostEnvironment.WebRootPath, "clothes")))
            {
                Directory.CreateDirectory(Path.Combine(webHostEnvironment.WebRootPath, "clothes"));
            }
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var fileExtension = Path.GetExtension(Image.FileName).ToLower();

            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            var mimeType = Image.ContentType.ToLower();

            if (!allowedExtensions.Contains(fileExtension) || !allowedMimeTypes.Contains(mimeType))
            {
                TempData["error"] = "Only JPG, PNG, and WEBP formats are allowed!";
                return View();
            }
            var path = Path.Combine(webHostEnvironment.WebRootPath, "clothes", Image.FileName);
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                Image.CopyTo(fs);
            }
            var clothes = new Clothes
            {
                Title = model.Title,
                Quantity = model.Quantity,
                Image = Image.FileName,
                Createdat = DateTime.Now
            };

            clothes.ClothesServices = new List<ClothesService>();

            foreach (var serviceId in model.SelectedServiceIds)
            {
                clothes.ClothesServices.Add(new ClothesService
                {
                    ClothesId = clothes.Id,
                    ServicesId = serviceId
                });
            }

            myDbContext.Clothes.Add(clothes);
            await myDbContext.SaveChangesAsync();

            TempData["success"] = "Cloth Added Successfully";

            return RedirectToAction("Clothes");
        }

        public async Task<IActionResult> ShowBookings()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("id")))
            {
                return RedirectToAction("Login", "Home");
            }
            List<Booking> bookings = await myDbContext.Bookings
                                                    .Include(x => x.User)
                                                    .Include(x => x.BookingClothes)
                                                        .ThenInclude(bc => bc.Clothes)
                                                    .Include(x => x.BookingClothes)
                                                        .ThenInclude(bs => bs.Services)
                                                    .OrderByDescending(x => x.Id)
                                                    .ToListAsync();

            return View(bookings);
        }
        public async Task<IActionResult> UserBookings(int? id)
        {
            if (id == null)
            {
                return NotFound("Id is missing");
            }
            List<Booking> bookings = await myDbContext.Bookings
                                                            .Where(x => x.UserId == id)
                                                            .Include(x => x.User)
                                                            .Include(x => x.Payment)
                                                            .Include(x => x.BookingClothes)
                                                                .ThenInclude(bc => bc.Clothes)
                                                            .Include(x => x.BookingClothes)
                                                                .ThenInclude(bc => bc.Services)
                                                            .Include(x => x.BookingClothes)
                                                                .ThenInclude(bc => bc.Barcode)
                                                            .OrderByDescending(x => x.Id)
                                                            .ToListAsync();
            //List<Barcode> barcodes = await myDbContext.Barcodes.
            return View(bookings);
        }
        [HttpPost]
        public async Task<IActionResult> GenerateBarcode(int id)
        {
            if (id == 0)
            {
                return NotFound("Id is missing");
            }

            var bookingCloth = await myDbContext.BookingClothes
                .Include(x => x.Booking)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (bookingCloth == null)
            {
                return NotFound();
            }

            var existingBarcode = await myDbContext.Barcodes
                .FirstOrDefaultAsync(x => x.BookingClothesId == id);

            if (existingBarcode != null)
            {
                TempData["error"] = "Barcode already exists";
                return RedirectToAction("UserBookings", new { id = bookingCloth.Booking.UserId });
            }

            // Barcode value
            string barcodeValue = "BC-" + id + "-" + Guid.NewGuid().ToString().Substring(0, 8);

            // Image file name and paths
            string fileName = id + ".png";
            string relativePath = "barcodes/" + fileName;
            string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string fullPath = Path.Combine(wwwRootPath, "barcodes", fileName);

            // Make sure the barcodes folder exists
            if (!Directory.Exists(Path.Combine(wwwRootPath, "barcodes")))
            {
                Directory.CreateDirectory(Path.Combine(wwwRootPath, "barcodes"));
            }

            // Generate barcode image
            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Height = 100,
                    Width = 300,
                    Margin = 10
                }
            };
            var pixelData = writer.Write(barcodeValue);

            using (var bitmap = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppRgb))
            {
                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, pixelData.Width, pixelData.Height),
                    ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
                try
                {
                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0,
                        pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }

                bitmap.Save(fullPath, ImageFormat.Png);
            }

            // Save in database
            Barcode barcode = new Barcode
            {
                BookingClothesId = id,
                BarcodeValue = barcodeValue,
                BarcodeImagePath = relativePath
            };

            await myDbContext.Barcodes.AddAsync(barcode);
            await myDbContext.SaveChangesAsync();

            TempData["success"] = "Barcode generated successfully";
            return RedirectToAction("UserBookings", new { id = bookingCloth.Booking.UserId });
        }
        [HttpPost]
        public async Task<IActionResult> InService(int? bookingClothesId, int? userId)
        {
            if (bookingClothesId == null || userId == null)
            {
                return NotFound("Ids are missing");
            }

            var bookingClothes = await myDbContext.BookingClothes.Include(x => x.Services).Include(x => x.Clothes).Include(x => x.Barcode).FirstOrDefaultAsync(x => x.Id == bookingClothesId);
            if (bookingClothes == null)
            {
                return NotFound();
            }
            if (bookingClothes.Barcode == null)
            {
                TempData["error"] = "Please generate barcode first";
                return RedirectToAction("UserBookings", new { id = userId });
            }
            if (bookingClothes == null)
            {
                return NotFound();
            }
            bookingClothes.Status = 1;
            await myDbContext.SaveChangesAsync();
            TempData["success"] = $"{bookingClothes.Clothes.Title} are goes for {bookingClothes.Services.Name}";
            return RedirectToAction("UserBookings", new { id = userId });
        }

        [HttpPost]
        public async Task<IActionResult> Ready(int? bookingClothesId, int? userId)
        {
            if (bookingClothesId == null || userId == null)
            {
                return NotFound("Ids are missing");
            }

            var bookingClothes = await myDbContext.BookingClothes.Include(x => x.Services).Include(x => x.Clothes).Include(x => x.Barcode).FirstOrDefaultAsync(x => x.Id == bookingClothesId);
            if (bookingClothes == null)
            {
                return NotFound();
            }
            bookingClothes.Status = 2;
            await myDbContext.SaveChangesAsync();
            TempData["success"] = $"{bookingClothes.Clothes.Title} are ready";
            return RedirectToAction("UserBookings", new { id = userId });
        }

        [HttpPost]
        public async Task<JsonResult> BookingReady(int? bookingId)
        {
            if (bookingId == null)
            {
                return Json(new { success = false, message = "Bookings id is missing" });
            }

            var booking = await myDbContext.Bookings
                .Include(x => x.BookingClothes)
                .ThenInclude(x => x.Clothes)
                .FirstOrDefaultAsync(x => x.Id == bookingId);
            var notReadyClothes = booking.BookingClothes.Where(x => x.Status != 2).ToList();
            //var sameClothes = booking.BookingClothes.Where(x => x.BookingId == bookingId && x.ClothesId == booking.)

            if (booking.BookingClothes.Any(x => x.Status != 2))
            {
                string titles = string.Join(", ", notReadyClothes.Select(x => x.Clothes.Title));
                //return Json(titles);
                //TempData["error"] = $"{titles} are not ready";
                //return Json(booking.BookingClothes.Any(x => x.Status != 2));
                return Json(new { success = false, message = $"{titles} are not ready", remaining = "yes" });
            }


            booking.Status = Booking._Status.Ready;
            await myDbContext.SaveChangesAsync();
            //TempData["success"] = "Booking is ready";
            return Json(new { success = true, message = "Booking is ready" });

        }

        public async Task<JsonResult> ConfirmReady(int? bookingId)
        {
            if (bookingId == null)
            {
                return Json(new { success = false, message = "Id is missing" });
            }

            var booking = await myDbContext.Bookings
                .Include(x => x.BookingClothes)
                .ThenInclude(x => x.Barcode)
                .FirstOrDefaultAsync(x => x.Id == bookingId);

            if (booking.BookingClothes.Any(x => x.Barcode == null))
            {
                return Json(new { success = false, message = "Please generate all barcode first" });
            }
            foreach(var cloth in booking.BookingClothes.Where(x => x.Status == 0 || x.Status == 1))
            {
                cloth.Status = 2;
            }
            //.ToList();

            booking.Status = Booking._Status.Ready;
            await myDbContext.SaveChangesAsync();
            return Json(new { success = true, message = "Booking is ready" });
        }

        [HttpPost]
        public async Task<IActionResult> BookingDelivered(int? bookingId, int? userId)
        {
            if (bookingId == null || userId == null)
            {
                return NotFound("Ids are missing");
            }

            var booking = await myDbContext.Bookings.FindAsync(bookingId);
            if (booking == null)
            {
                return NotFound("Booking not found");
            }
            booking.Status = Booking._Status.Delivered;
            await myDbContext.SaveChangesAsync();

            TempData["success"] = "Booking Delivered Successfully";
            return RedirectToAction("UserBookings", new { id = userId });

        }

        [HttpPost]
        public async Task<IActionResult> SendInvoice(int bookingId, int userId)
        {
            var booking = await myDbContext.Bookings
                .Include(b => b.User)
                .Include(b => b.BookingClothes)
                    .ThenInclude(bc => bc.Clothes)
                .Include(b => b.BookingClothes)
                    .ThenInclude(bc => bc.Services)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                TempData["error"] = "Booking not found.";
                return RedirectToAction("UserBookings", new { id = userId });
            }

            var userEmail = booking.User.Email;

            string emailBody = $@"
        <h2>Invoice for Booking #{booking.Id}</h2>
        <p><strong>Name:</strong> {booking.User.Name}</p>
        <p><strong>Address:</strong> {booking.Address}</p>
        <p><strong>Pickup Date & Time:</strong> {booking.Date.ToShortDateString()} {booking.Time}</p>
        <p><strong>Total Amount:</strong> {booking.BookingClothes.Sum(x => x.Services.Price)}</p>
        <p><strong>Payment Status:</strong> {(booking.Payment != null ? booking.Payment.PaymentStatus : "Not Paid")}</p>

        <table border='1' cellpadding='5'>
            <tr><th>Cloth</th><th>Service</th></tr>";

            foreach (var bc in booking.BookingClothes)
            {
                emailBody += $"<tr><td>{bc.Clothes.Title}</td><td>{bc.Services.Name}</td></tr>";
            }

            emailBody += "</table>";

            var mail = new MailMessage();
            mail.To.Add(userEmail);
            mail.CC.Add("aptechrafay2@gmail.com");
            mail.Subject = $"Laundry Invoice - Booking #{booking.Id}";
            mail.Body = emailBody;
            mail.IsBodyHtml = true;
            mail.From = new MailAddress("rafayrashid457@gmail.com");

            using var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("rafayrashid457@gmail.com", "siuymtzsjdocebzk"),
                EnableSsl = true,
            };

            await smtp.SendMailAsync(mail);
            TempData["success"] = "Invoice sent successfully!";
            return RedirectToAction("UserBookings", new { id = userId });

        }

        public async Task<IActionResult> PaidDelivery(int paymentId, int userId)
        {
            var payment = await myDbContext.Payments.FindAsync(paymentId);
            if (payment == null)
            {
                return NotFound("Payment not found");
            }
            payment.PaymentStatus = Payment.Status.Completed;
            await myDbContext.SaveChangesAsync();
            TempData["success"] = "Update payment status to completed";
            return RedirectToAction("UserBookings", new { id = userId });
        }
        public async Task<IActionResult> ShowUsers()
        {
            List<User> users = await myDbContext.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> Contact()
        {
            List<Contact> contacts = await myDbContext.Contacts.Include(x => x.User).ToListAsync();
            return View(contacts);
        }
    }
}
