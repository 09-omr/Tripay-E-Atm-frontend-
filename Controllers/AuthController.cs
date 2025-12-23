using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using digitalpayment3.Models;
using digitalpayment3.Services;

namespace digitalpayment3.Controllers
{
    public class AuthController : Controller
    {
        private readonly NodeApiService _nodeApiService;

        public AuthController(NodeApiService nodeApiService)
        {
            _nodeApiService = nodeApiService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.PageTitle = "Giriþ Yap";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Node.js API'den kullanýcýyý ve rollerini al
                var result = await _nodeApiService.LoginAsync(model.Email, model.Password);
                
                if (result.Success && result.Data != null)
                {
                    var user = result.Data.User;
                    
                    // Session'a token kaydet
                    HttpContext.Session.SetString("NodeApiToken", result.Data.Token);
                    
                    // Claims oluþtur
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.FullName),
                        new Claim(ClaimTypes.Email, user.Email)
                    };

                    // Node.js API'den gelen rolleri ekle
                    // roles: ["admin"] veya roles: ["user"]
                    string primaryRole = "User"; // Default
                    if (user.Roles != null && user.Roles.Count > 0)
                    {
                        foreach (var role in user.Roles)
                        {
                            // "admin" -> "Admin", "user" -> "User"
                            var normalizedRole = char.ToUpper(role[0]) + role.Substring(1).ToLower();
                            claims.Add(new Claim(ClaimTypes.Role, normalizedRole));
                            
                            // Ýlk rolü primary olarak al
                            if (role.ToLower() == "admin")
                            {
                                primaryRole = "Admin";
                            }
                        }
                    }
                    else
                    {
                        // Rol yoksa default User rolü ekle
                        claims.Add(new Claim(ClaimTypes.Role, "User"));
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    TempData["SuccessMessage"] = $"Hoþ geldiniz, {user.FullName}!";
                    
                    // Admin ise admin paneline, deðilse hesaplarým sayfasýna yönlendir
                    if (primaryRole == "Admin")
                        return RedirectToAction("Index", "Admin");
                    else
                        return RedirectToAction("Index", "Account");
                }
                
                ModelState.AddModelError("", "E-posta veya þifre hatalý!");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.PageTitle = "Kayýt Ol";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string fullName)
        {
            // Node API'ye kayýt yap
            var result = await _nodeApiService.RegisterAsync(email, password, fullName);
            
            if (result.Success && result.Data != null)
            {
                var user = result.Data.User;
                
                HttpContext.Session.SetString("NodeApiToken", result.Data.Token);
                
                // Yeni kullanýcý her zaman User rolüyle baþlar
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, "User")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties { IsPersistent = false });

                TempData["SuccessMessage"] = $"Hoþ geldiniz, {user.FullName}! Hesabýnýz oluþturuldu.";
                return RedirectToAction("Index", "Account");
            }

            TempData["ErrorMessage"] = result.ErrorMessage ?? "Kayýt iþlemi baþarýsýz!";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("NodeApiToken");
            TempData["SuccessMessage"] = "Baþarýyla çýkýþ yaptýnýz.";
            return RedirectToAction("Index", "Home");
        }
    }
}
