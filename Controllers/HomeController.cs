using Laundry.Migrations;
using Laundry.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.Mail;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Stripe;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Stripe.Checkout;
using System.ComponentModel.DataAnnotations;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Reflection.PortableExecutable;
using static System.Net.Mime.MediaTypeNames;
using Stripe.Climate;
using System.Text;
namespace Laundry.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        private readonly MyDbContext myDbContext;

        public HomeController(ILogger<HomeController> logger, MyDbContext myDbContext) : base(myDbContext)
        {
            _logger = logger;
            this.myDbContext = myDbContext;
        }

        public void SetSession(User user)
        {
            HttpContext.Session.SetString("id", user.Id.ToString());
            HttpContext.Session.SetString("name", user.Name);
            HttpContext.Session.SetString("email", user.Email);
            HttpContext.Session.SetString("phone", user.Phone);
            HttpContext.Session.SetString("password", user.Password);
            HttpContext.Session.SetString("role", user.Role.ToString());
        }


        public IActionResult Register()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("id")))
            {
                return RedirectToAction("Index");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var checkEmail = await myDbContext.Users.AnyAsync(x => x.Email == user.Email);
            if (checkEmail)
            {
                TempData["error"] = "Email already exists";
                return View();
            }
            PasswordHasher<User> hash = new PasswordHasher<User>();
            user.Password = hash.HashPassword(user, user.Password);
            await myDbContext.Users.AddAsync(user);
            await myDbContext.SaveChangesAsync();
            TempData["success"] = "Registration Complete Successfully";
            SetSession(user);
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("Enter Your Email", "Enter Your Password"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("Enter Your Email"),
                Subject = "Welcome to Laundry Management System!",
                Body = $@"
                        Dear {user.Name},

                        Thank you for registering with Laundry Management System!

                        We’re excited to have you onboard. You can now log in and start using our services.

                        If you have any questions, feel free to contact us anytime.

                        Best regards,
                        Laundry Management Team
                        ",
                IsBodyHtml = false
            };

            mailMessage.To.Add(user.Email);


            //mailMessage.CC.Add("");

            await smtpClient.SendMailAsync(mailMessage);
            return RedirectToAction("Index");
        }

        public IActionResult Login()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("id")))
            {
                return RedirectToAction("Index");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(User user)
        {
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                TempData["error"] = "Both fields are required";
                return View();
            }
            var userData = await myDbContext.Users.Where(x => x.Email == user.Email).FirstOrDefaultAsync();
            if (userData != null)
            {
                PasswordHasher<User> hash = new PasswordHasher<User>();
                PasswordVerificationResult result = hash.VerifyHashedPassword(user, userData.Password, user.Password);

                if (result == PasswordVerificationResult.Success)
                {
                    TempData["success"] = "Login Successfully";
                    SetSession(userData);
                    if (userData.Role == Models.User._Role.Admin)
                    {
                        return RedirectToAction("Index", "Admin");
                    }

                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Password not match";
                    return View();
                }
            }
            else
            {
                TempData["error"] = "Email Not Found";
                return View();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            ViewData["CurrentAction"] = "Index";
            ViewData["CurrentController"] = "Home";
            return View();
        }

        public IActionResult Services()
        {
            ViewData["CurrentAction"] = "Services";
            ViewData["CurrentController"] = "Home";
            var services = myDbContext.Services
                .Include(s => s.ClothesServices)
                .ToList();

            var clothes = myDbContext.Clothes
                .Include(c => c.ClothesServices)
                .ToList();

            var model = new FetchClothesandServices
            {
                Services = services,
                Clothes = clothes
            };

            return View(model);
        }

        public async Task<JsonResult> GetClothes(int serviceId)
        {

            var clothes = await myDbContext.ClothesServices
                                            .Where(x => x.ClothesId == serviceId)
                                            .Select(x => new
                                            {
                                                Id = x.Clothes.Id,
                                                Title = x.Clothes.Title,
                                                Quantity = x.Clothes.Quantity,
                                                Image = x.Clothes.Image
                                            })
                                            .ToListAsync();

            return Json(new { success = true, clothes = clothes });


        }

        public async Task<IActionResult> ShowBookings()
        {
            ViewData["CurrentAction"] = "ShowBookings";
            ViewData["CurrentController"] = "Home";
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("id")))
            {
                return RedirectToAction("Login");
            }

            int userId = Convert.ToInt32(HttpContext.Session.GetString("id"));

            List<Booking> bookings = await myDbContext.Bookings
                .Where(x => x.UserId == userId)
                .Include(x => x.User)
                .Include(x => x.Payment)
                .Include(x => x.BookingClothes)
                    .ThenInclude(bc => bc.Clothes)
                .Include(x => x.BookingClothes)
                    .ThenInclude(bs => bs.Services)
                .OrderByDescending(x => x.Id)
                .ToListAsync();
            //return Json(bookings.Select(x => x.BookingClothes.Sum(x => x.Services.Price)));
            //var totalPrice = bookings.Select(x => x.BookingClothes.Sum(x => x.Services.Price));
            //totalPrice.Where(x => x.Id == 3);
            //ViewData["totalPrice"] = totalPrice;

            ViewBag.PublishableKey = "pk_test_51QC2ODIe4idSW70t3KGivHvvWGDachFhshcM3FC3kUOGiog9iupBTWRzeSR622duJ94Vzpuk034kvUAHK9OdviY100JX1FOvsF";

            return View(bookings);
        }


        [HttpGet]
        public async Task<IActionResult> GetAddress(double lat, double lon)
        {
            using var client = new HttpClient();
            var url = $"https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat={lat}&lon={lon}";

            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return Content(result, "application/json");
            }

            return BadRequest("Failed to fetch address");
        }


        public IActionResult Booking()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("id")))
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        [HttpPost]
        public IActionResult TempStoreBookingData([FromBody] List<BookingClothesTemp> items)
        {
            Console.WriteLine($"Received {items?.Count} items");
            if (items != null)
            {
                foreach (var item in items)
                {
                    Console.WriteLine($"Item: ClothId={item.ClothId}, ServiceId={item.ServiceId}, Quantity={item.Quantity}");
                }
            }

            HttpContext.Session.SetString("SelectedItems", JsonSerializer.Serialize(items));
            return Json(new { success = true });
        }




        [HttpPost]
        public async Task<IActionResult> Booking(Booking booking)
        {
            var userId = HttpContext.Session.GetString("id");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login");

            booking.UserId = Convert.ToInt32(userId);
            await myDbContext.Bookings.AddAsync(booking);
            await myDbContext.SaveChangesAsync();

            var selectedItemsJson = HttpContext.Session.GetString("SelectedItems");

            if (!string.IsNullOrEmpty(selectedItemsJson))
            {
                var selectedItems = JsonSerializer.Deserialize<List<BookingClothesTemp>>(selectedItemsJson);

                foreach (var item in selectedItems)
                {
                    //return Json(item);
                    // Create multiple records based on quantity
                    for (int i = 0; i < item.Quantity; i++)
                    {
                        var bookingClothes = new BookingClothes
                        {
                            BookingId = booking.Id,
                            ClothesId = item.ClothId,
                            ServicesId = item.ServiceId
                        };
                        await myDbContext.BookingClothes.AddAsync(bookingClothes);
                    }
                }

                await myDbContext.SaveChangesAsync();
            }

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("Enter Your Email", "Your App Password"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("Enter Your Email"),
                Subject = "Booking Confirmation",
                Body = $@"
                            Dear {HttpContext.Session.GetString("name")},

                            Thank you for your booking!

                            Your appointment has been successfully scheduled.

                            Here are your booking details:
                            • Date: {booking.Date.ToShortDateString()}
                            • Time: {booking.Time}
                            • Address: {booking.Address}

                            We look forward to serving you.

                            Best regards,
                            Laundry Management Team
                            ",
                IsBodyHtml = false
            };

            mailMessage.To.Add(HttpContext.Session.GetString("email"));

            //mailMessage.CC.Add("");

            await smtpClient.SendMailAsync(mailMessage);
            TempData["success"] = "Booking Added Successfully, We will contact you";
            return RedirectToAction("ShowBookings");
        }
        //public IActionResult testing(int bookingId = 33)
        //{
        //    var totalPrice =  myDbContext.Bookings.Include(x => x.BookingClothes).ThenInclude(x => x.Services).FirstOrDefault(x => x.Id == bookingId);
        //    return Json(totalPrice.BookingClothes.Sum(x => x.Services.Price));
        //}
        [HttpPost]
        public async Task<IActionResult> Payment(int? bookingId)
        {

            if (bookingId == null)
            {
                return Json(new { success = false, error = "Id Not Found" });
            }
            Booking booking = await myDbContext.Bookings.FindAsync(bookingId);
            if (booking == null)
            {
                return Json(new { success = false, error = "Booking Not Found" });
            }
            StripeConfiguration.ApiKey = "sk_test_51QC2ODIe4idSW70tDuPsYTghj5mmmTHUbF9PktegF6aT3EkievLJ3NsscCWp6xScUAxAxjGBTsJIw68AGuytkWQj00gzHHR6BL";
            var bookings = await myDbContext.Bookings.Include(x => x.BookingClothes).ThenInclude(x => x.Services).FirstOrDefaultAsync(x => x.Id == bookingId);
            double totalPrice = bookings.BookingClothes.Sum(x => x.Services.Price);
            //return totalPrice.BookingClothes.Sum(x => x.Services.Price);
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Test Product",
                        },
                        UnitAmount = (int)totalPrice * 100,
                    },
                    Quantity = 1,
                },
            },
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Home/AddPayment?bookingId={bookingId}",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Home/Cancel",
            };

            var service = new SessionService();
            Session session = service.Create(options);

            HttpContext.Session.SetString("sessionId", session.Id);
            HttpContext.Session.SetString("price", totalPrice.ToString());

            return Json(new { id = session.Id });
        }

        public async Task<IActionResult> AddPayment(int bookingId)
        {
            await myDbContext.Payments.AddAsync(new Models.Payment { BookingId = bookingId, PaymentMethod = Models.Payment.Method.Card, PaymentStatus = Models.Payment.Status.Completed, Amount = Convert.ToInt32(HttpContext.Session.GetString("price")), TransactionId = HttpContext.Session.GetString("sessionId") });
            await myDbContext.SaveChangesAsync();
            TempData["success"] = "Payment Added Successfully";
            HttpContext.Session.Remove("sessionId");
            HttpContext.Session.Remove("price");
            return RedirectToAction("ShowBookings");
        }

        public async Task<IActionResult> PaymentByCash(int? id)
        {
            if (id == null)
            {
                return NotFound("Id is missing");
            }
            var bookings = await myDbContext.Bookings.Include(x => x.BookingClothes).ThenInclude(x => x.Services).FirstOrDefaultAsync(x => x.Id == id);
            double totalPrice = bookings.BookingClothes.Sum(x => x.Services.Price);
            await myDbContext.Payments.AddAsync(new Models.Payment { BookingId = (int)id, Amount = (int)totalPrice, PaymentMethod = Models.Payment.Method.Cash, PaymentStatus = Models.Payment.Status.Pending });
            await myDbContext.SaveChangesAsync();
            TempData["success"] = "Your payment status is pending";
            return RedirectToAction("ShowBookings");
        }
        public IActionResult Contact()
        {
            ViewData["CurrentAction"] = "Contact";
            ViewData["CurrentController"] = "Home";
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Contact(string message, string subject)
        {
            if (!HttpContext.Session.Keys.Contains("id"))
            {
                TempData["error"] = "Please login first";
                return RedirectToAction("Login");
            }
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(subject))
            {
                TempData["error"] = "Please fill all fields";
                return View();
            }

            await myDbContext.Contacts.AddAsync(new Models.Contact { Message = message, Subject = subject, UserId = Convert.ToInt32(HttpContext.Session.GetString("id")) });
            await myDbContext.SaveChangesAsync();
            TempData["success"] = "Your issue submitted successfully, We will contact you ASAP";
            return RedirectToAction("Index");
        }

        public IActionResult Profile()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Profile(User user)
        {

            ModelState.Remove("password");
            if (!ModelState.IsValid)
            {
                return View();
            }
            if (user.Password != null)
            {
                if (user.Password.Length < 6)
                {
                    TempData["error"] = "Password must be at least 6 characters";
                    return View(user);
                }
                PasswordHasher<User> hash = new PasswordHasher<User>();
                user.Password = hash.HashPassword(user, user.Password);
            }
            else
            {
                user.Password = HttpContext.Session.GetString("password");
            }
            if (HttpContext.Session.GetString("role") == "Admin")
            {
                user.Role = Models.User._Role.Admin;
            }
            myDbContext.Users.Update(user);
            await myDbContext.SaveChangesAsync();
            SetSession(user);
            //return Json(user);
            if (user.Role == Models.User._Role.Admin)
            {
                return RedirectToAction("Index", "Admin");
            }
            return RedirectToAction("Index");
        }

        public IActionResult About()
        {
            ViewData["CurrentAction"] = "About";
            ViewData["CurrentController"] = "Home";
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
