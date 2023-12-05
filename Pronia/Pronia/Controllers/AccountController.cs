using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Pronia.DAL;
using Pronia.Models;
using Pronia.ViewModels;
using System.Text.RegularExpressions;

namespace Pronia.Controllers
{
	public class AccountController : Controller
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;

		public AccountController(UserManager<AppUser> userManager,SignInManager<AppUser> signInManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
		}
		public IActionResult Register()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Register(RegisterVM userVM)
		{
			if(!ModelState.IsValid) return View();
			AppUser user = new AppUser
			{
				Name = userVM.Name,
				Surname = userVM.Surname,
				UserName = userVM.Username,
				Email = userVM.Email,

			};
			IdentityResult result= await _userManager.CreateAsync(user,userVM.Password);
			if(!result.Succeeded)
			{
				foreach (IdentityError error in result.Errors)
				{
					ModelState.AddModelError(String.Empty, error.Description);

				}
				return View();
			}
			if (ModelState.IsValid)
			{
				bool isEmailValid = Regex.IsMatch(userVM.Email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
				if (isEmailValid)
				{
					return RedirectToAction("Index", "Home");
				}
				else
				{
					ModelState.AddModelError("Email", "Invalid email address format.");
				}
			}

			return View(userVM);
			await _signInManager.SignInAsync(user, false);
			return RedirectToAction("Index","Home");
		}
		public async Task<IActionResult> LogOut()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}
	}
}
