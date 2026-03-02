using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BizSecureDemo22180044.Data;
using BizSecureDemo22180044.Models;

namespace BizSecureDemo22180044.Controllers
{
    public class OrdersController : Controller
    {
        private readonly AppDbContext _db;

        public OrdersController(AppDbContext db)
        {
            _db = db;
        }

        
        public IActionResult Create()
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order)
        {
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                order.UserId = int.Parse(userId);

                _db.Add(order);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            return View(order);
        }

       
        public async Task<IActionResult> Details(int id)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}