﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using HomeAccountingSystem_WebUI.Infrastructure;
using HomeAccountingSystem_WebUI.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace HomeAccountingSystem_WebUI.Controllers
{
    [Authorize(Roles = "Administrators")]
    [SessionState(SessionStateBehavior.Disabled)]
    public class RoleAdminController : Controller
    {
        private AccRoleManager RoleManager => HttpContext.GetOwinContext().GetUserManager<AccRoleManager>();
        private AccUserManager UserManager => HttpContext.GetOwinContext().GetUserManager<AccUserManager>();

        public ActionResult Index()
        {
            return View(RoleManager.Roles);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create([Required]string rolename)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await RoleManager.CreateAsync(new AccRoleModel(rolename));
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrorsToModel(result);
                }
            }
            return View(rolename);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(string id)
        {
            var role = await RoleManager.FindByIdAsync(id);
            if (role != null)
            {
                IdentityResult result = await RoleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return View("Error", result.Errors);
                }
            }
            else
            {
                return View("Error", new string[] {"Роль не найдена"});
            }
        }


        public async Task<ActionResult> Edit(string id)
        {
            var role = await RoleManager.FindByIdAsync(id);
            if (role != null)
            {
                string[] memberIds = role.Users.Select(x=>x.UserId).ToArray();
                IEnumerable<AccUserModel> members = UserManager.Users.Where(x => memberIds.Any(y => y == x.Id));
                IEnumerable<AccUserModel> nonMembers = UserManager.Users.Except(members);
                var roleEditModel = new RoleEditModel()
                {
                    Role = role,
                    Members = members,
                    NonMembers = nonMembers
                };
                return View(roleEditModel);
            }
            else
            {
                return View("Error", new string[] {"Роль не найдена"});
            }
         }

        [HttpPost]
        public async Task<ActionResult> Edit(RoleModificationModel model)
        {
            IdentityResult result;
            if (ModelState.IsValid)
            {
                foreach (var userId in model.IdsToAdd ?? new string[] {})
                {
                    result = await UserManager.AddToRoleAsync(userId, model.RoleName);
                    if (!result.Succeeded)
                    {
                        return View("Error", result.Errors);
                    }
                }
                foreach (var userId in model.IdsToDelete ?? new string[] {})
                {
                    result = await UserManager.RemoveFromRoleAsync(userId, model.RoleName);
                    if (!result.Succeeded)
                    {
                        return View("Error", result.Errors);
                    }
                }
                return RedirectToAction("Index");
            }
            else
            {
                return View("Error",new string[] {"Роль не найдена"});
            }
        }


        private void AddErrorsToModel(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("",error);
            }
        }
    }

}