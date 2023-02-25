﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using EmployeeManagement.Models;
using EmployeeManagement.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Controllers
{
	public class AdminstrationController : Controller
	{
		private readonly RoleManager<IdentityRole> roleManager;
		private readonly UserManager<ApplicationUser> userManager;

		public AdminstrationController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
		{
			this.roleManager = roleManager;
			this.userManager = userManager;
		}

		[HttpGet]
		public IActionResult CreateRole()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
		{
			if (ModelState.IsValid)
			{
				var role = new IdentityRole
				{
					Name = model.RoleName
				};
				var result = await roleManager.CreateAsync(role);

				if (result.Succeeded)
				{
					return RedirectToAction("RolesList", "Adminstration");
				}

				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
			}
			return View(model);
		}


		[HttpGet]
		public IActionResult RolesList()
		{
			var roles = roleManager.Roles;
			return View(roles);
		}

		[HttpGet]
		public async Task<IActionResult> EditRole(string id)
		{
			var role = await roleManager.FindByIdAsync(id);

			if (role == null)
			{
				ViewBag.ErrorMessage = $"Role with ID = {id} cannot be found";
				return View("StatusCodeError");
			}


			var model = new EditRoleViewModel
			{
				Id = role.Id,
				RoleName = role.Name
			};

			foreach (var user in await userManager.Users.ToListAsync())
			{
				if (await userManager.IsInRoleAsync(user, role.Name))
				{
					model.Users.Add(user.UserName);
				}
			}
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> EditRole(EditRoleViewModel model)
		{
			if (ModelState.IsValid)
			{
				var role = await roleManager.FindByIdAsync(model.Id);

				if (role == null)
				{
					ViewBag.ErrorMessage = $"Role with ID = {model.Id} cannot be found";
					return View("StatusCodeError");
				}

				role.Name = model.RoleName;
				var result = await roleManager.UpdateAsync(role);

				if (result.Succeeded)
				{
					return RedirectToAction("RolesList", "Adminstration");
				}

				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}

			}
			return View(model);

		}

		[HttpGet]
		public async Task<IActionResult> EditUsersInRole(string roleId)
		{
			ViewBag.roleId = roleId;

			var role = await roleManager.FindByIdAsync(roleId);

			if (role == null)
			{
				ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
				return View("NotFound");
			}

			var model = new List<UserRoleViewModel>();

			foreach (var user in userManager.Users.ToList())
			{
				var userRoleViewModel = new UserRoleViewModel
				{
					UserId = user.Id,
					UserName = user.UserName
				};

				if (await userManager.IsInRoleAsync(user, role.Name))
				{
					userRoleViewModel.IsSelected = true;
				}
				else
				{
					userRoleViewModel.IsSelected = false;
				}

				model.Add(userRoleViewModel);
			}

			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel>model, string roleId)
		{
			var role = await roleManager.FindByIdAsync(roleId);

			if (role == null)
			{
				ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
				return View("NotFound");
			}
			for (int i = 0; i < model.Count; i++)
			{
				var user = await userManager.FindByIdAsync(model[i].UserId);

				IdentityResult result = null;

				if (model[i].IsSelected && !(await userManager.IsInRoleAsync(user, role.Name)))
				{
					result = await userManager.AddToRoleAsync(user, roleId);
				}
				else if (!model[i].IsSelected && (await userManager.IsInRoleAsync(user, role.Name)))
				{
					result = await userManager.RemoveFromRoleAsync(user, role.Name);
				}

				

			}
			return RedirectToAction("EditRole", new { Id = roleId });

		}
	}
}