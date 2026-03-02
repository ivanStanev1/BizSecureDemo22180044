using System.Security.Claims;
using BizSecureDemo22180044.Data;
using BizSecureDemo22180044.Models;
using BizSecureDemo22180044.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BizSecureDemo22180044.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
        private readonly PasswordHasher<AppUser> _hasher;
        // Добавяме Logger за мониторинг на сигурността
        private readonly ILogger<AccountController> _logger;

        public AccountController(AppDbContext db, PasswordHasher<AppUser> hasher, ILogger<AccountController> logger)
        {
            _db = db;
            _hasher = hasher;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register() => View(new RegisterVm());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            if (vm.Password.Length < 8 ||
                !vm.Password.Any(char.IsUpper) ||
                !vm.Password.Any(char.IsDigit) ||
                !vm.Password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                ModelState.AddModelError("", "Паролата трябва да е поне 8 знака и да съдържа главна буква, цифра и специален символ!");
                return View(vm);
            }

            var email = vm.Email.Trim().ToLowerInvariant();

            if (await _db.Users.AnyAsync(u => u.Email == email))
            {
                ModelState.AddModelError("", "Този email вече е регистриран.");
                return View(vm);
            }

            var user = new AppUser { Email = email };
            user.PasswordHash = _hasher.HashPassword(user, vm.Password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Нов потребител се регистрира успешно: {Email}", email);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login() => View(new LoginVm());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var email = vm.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                _logger.LogWarning("Опит за вход с несъществуващ имейл: {Email}", email);
                ModelState.AddModelError("", "Грешен email или парола.");
                return View(vm);
            }

            if (user.LockoutUntilUtc.HasValue && user.LockoutUntilUtc > DateTime.UtcNow)
            {
                _logger.LogCritical("Опит за достъп до ЗАКЛЮЧЕН акаунт: {Email}", email);
                var remaining = (int)(user.LockoutUntilUtc.Value - DateTime.UtcNow).TotalMinutes + 1;
                ModelState.AddModelError("", $"Акаунтът е заключен! Опитайте пак след {remaining} минути.");
                return View(vm);
            }

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, vm.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                user.FailedLogins = (user.FailedLogins ?? 0) + 1;

                
                _logger.LogWarning("Неуспешен опит {Count} за потребител: {Email}", user.FailedLogins, email);

                if (user.FailedLogins >= 5)
                {
                    user.LockoutUntilUtc = DateTime.UtcNow.AddMinutes(5);
                    user.FailedLogins = 0;
                    _logger.LogError("Акаунтът на {Email} беше ЗАКЛЮЧЕН поради твърде много опити.", email);
                }

                await _db.SaveChangesAsync();
                ModelState.AddModelError("", $"Грешна парола! Опит {user.FailedLogins} от 5.");
                return View(vm);
            }

            
            user.FailedLogins = 0;
            user.LockoutUntilUtc = null;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Потребител {Email} влезе успешно в системата.", email);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}