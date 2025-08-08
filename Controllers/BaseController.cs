using Laundry.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Laundry.Controllers
{
    public class BaseController : Controller
    {
        protected readonly MyDbContext _context;

        public BaseController(MyDbContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            List<Services> services = _context.Services.OrderByDescending(x => x.Id).Take(5).ToList();
            ViewBag.Services = services;
            base.OnActionExecuting(context);
        }
    }

}
