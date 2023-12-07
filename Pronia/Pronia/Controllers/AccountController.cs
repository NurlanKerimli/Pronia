using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Pronia.DAL;
using Pronia.Models;
using Pronia.Utilities.Enums;
using Pronia.ViewModels;
using System.Text.RegularExpressions;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Pronia.Controllers
{
	[AutoValidateAntiforgeryToken]
	public class AccountController : Controller
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public AccountController(UserManager<AppUser> userManager,SignInManager<AppUser> signInManager,RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_roleManager = roleManager;
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

			await _userManager.AddToRoleAsync(user, UserRole.Member.ToString());
			await _signInManager.SignInAsync(user, false);
			return RedirectToAction("Index","Home");
		}

		public IActionResult Login()
		{
			return View();
		}
		[HttpPost]

		public async Task<IActionResult> Login(LoginVM loginVM,string? returnUrl)
		{
			if (!ModelState.IsValid) return View();
			AppUser user = await _userManager.FindByNameAsync(loginVM.UsernameOrEmail);
			if (user == null)
			{
				user = await _userManager.FindByEmailAsync(loginVM.UsernameOrEmail);
				if (user == null)
				{
					ModelState.AddModelError(String.Empty, "The Username,Email or Password is incorrect");
					return View();
				}
			}
			var result= await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.IsRemembered, true);
			if(result.IsLockedOut)
			{
				ModelState.AddModelError(String.Empty, "The Username,Email or Password is incorrect");
				return View();
			}
			if (!result.Succeeded)
			{
				ModelState.AddModelError(String.Empty, "Your account has been blocked because you entered your password incorrectly, please check again later");
				return View();
			}
			if(returnUrl is null)
			{
				return RedirectToAction("Index", "Home");
			}
			return Redirect(returnUrl);
		}
		public async Task<IActionResult> LogOut()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}
		public async Task<IActionResult> CreateRoles()
		{
			foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
			{
				if(!await _roleManager.RoleExistsAsync(role.ToString()))
				{
					await _roleManager.CreateAsync(new IdentityRole
					{
						Name = role.ToString(),
					});
				}
				
			}
			return RedirectToAction("Index", "Home");
		}
	}
}
