using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using BizSecureDemo22180044.Data;
using BizSecureDemo22180044.Models;

namespace BizSecureDemo22180044.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

       

        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
               
                return View(new List<Order>());
            }

            var uid = int.Parse(userIdClaim);

           
            var myOrders = await _db.Orders
                .Where(o => o.UserId == uid)
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            
            var allOrders = await _db.Orders
                .OrderByDescending(o => o.Id)
                .ToListAsync();

           
            ViewBag.AllOrders = allOrders;

            return View(myOrders);
        }

        public IActionResult Privacy()
        {
            return View();
        }

       
    }
}