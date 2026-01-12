using Microsoft.AspNetCore.Mvc;
using MyStoreMVC.DTOs;
using MyStoreMVC.Services;

namespace MyStoreMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;

        public AccountController(AuthService authService)
        {
            _authService = authService;
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            // Si ya está autenticado, redirigir a productos
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Products");
            }
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return View(registerDto);
            }

            var result = await _authService.RegisterAsync(registerDto);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(registerDto);
            }

            // Guardar token en cookie
            Response.Cookies.Append("AuthToken", result.Data!.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(24)
            });

            TempData["SuccessMessage"] = "Registration successful! Welcome " + result.Data.Username;
            return RedirectToAction("Index", "Products");
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Si ya está autenticado, redirigir a productos
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Products");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto loginDto, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(loginDto);
            }

            var result = await _authService.LoginAsync(loginDto);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(loginDto);
            }

            // Guardar token en cookie
            Response.Cookies.Append("AuthToken", result.Data!.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(24)
            });

            TempData["SuccessMessage"] = "Welcome back, " + result.Data.Username + "!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Products");
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");
            TempData["SuccessMessage"] = "You have been logged out successfully";
            return RedirectToAction("Index", "Products");
        }
    }
}