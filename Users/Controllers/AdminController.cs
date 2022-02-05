using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Users.Models;
using System.Security.Claims;

namespace Users.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private IUserValidator<AppUser> UserValidator;
        private IPasswordValidator<AppUser> passwordValidator;
        private IPasswordHasher<AppUser> passwordHasher;
        private UserManager<AppUser> userManager;

        public AdminController(UserManager<AppUser> userManager,IUserValidator<AppUser> UserValidator , 
            IPasswordValidator<AppUser> passwordValidator, IPasswordHasher<AppUser> passwordHasher)
        {
            this.userManager = userManager;
            this.UserValidator = UserValidator;
            this.passwordValidator = passwordValidator;
            this.passwordHasher = passwordHasher;

        }
        public ViewResult Index() => View(userManager.Users);
        public ViewResult create() => View();

        [HttpPost]
        public async  Task<IActionResult> create(UserViewModels models)
        {
            if (ModelState.IsValid)
            {
                AppUser users = new AppUser()
                { 
                    UserName = models.Name,
                    Email = models.Email
                };
                var result = await userManager.CreateAsync(users, models.Password);
                if (result.Succeeded)
                {
                   var res= await userManager.AddClaimAsync(users, new Claim("Editeuser", "ویرایش کاربران"));
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            return View(models);

        }
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var user = await userManager.FindByIdAsync(id);
                if (user !=null)
                {
                    var result = await userManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        AddErrorsFromResult(result);
                    }
                }
                    ModelState.AddModelError("", "User Not Found");

            }
            return View("Index", userManager.Users);
        }
        
        public async Task<IActionResult> Edite(string id)
        {
            var res = User.Claims.Any(c => c.Type == "Editeuser");
            if (res)
            {
                var user = await userManager.FindByIdAsync(id);
                if (user != null)
                {
                    return View(user);
                }
                else
                {
                    return RedirectToAction("Index");
                }

            }
            else
            {
                ModelState.AddModelError("Not in Claim", "You havn`t Claim");
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Edite(string id, string email, string password)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.Email = email;
                var validEmail = await UserValidator.ValidateAsync(userManager, user);
                if (!validEmail.Succeeded)
                {
                    AddErrorsFromResult(validEmail);
                }
                IdentityResult passiden = null;
                if (!string.IsNullOrEmpty(password))
                {
                    passiden = await passwordValidator.ValidateAsync(userManager, user, password);
                    if (passiden.Succeeded)
                    {
                        user.PasswordHash = passwordHasher.HashPassword(user, password);
                    }
                    else
                    {
                        AddErrorsFromResult(passiden);
                    }
                }
                if ((validEmail.Succeeded && passiden == null) || (validEmail.Succeeded
&& password != string.Empty && passiden.Succeeded))
                {
                    var result = await userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        AddErrorsFromResult(result);
                    }
                }

                else
                {
                    ModelState.AddModelError("", "User Not Found");
                }
                return View(user);

            }
            else
            {
                ModelState.AddModelError("", "User Not Found");
            }
            return View(user);
        }
        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
    }
}
