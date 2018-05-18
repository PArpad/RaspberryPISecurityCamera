using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RaspberryPISecurityCam.Data;
using RaspberryPISecurityCam.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using RaspberryPISecurityCam.Authorization;
using RaspberryPISecurityCam.Models.SecAppUserViewModels;

namespace RaspberryPISecurityCam.Controllers
{
    [Authorize(Roles = "SecCamUserAdministrators")]
    public class SecAppUsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public SecAppUsersController(ApplicationDbContext context, IAuthorizationService authorizationService, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _authorizationService = authorizationService;
            _signInManager = signInManager;
        }

        // GET: SecAppUsers
        public async Task<IActionResult> Index()
        {
            return View(await _context.SecAppUser.ToListAsync());
        }

        // GET: SecAppUsers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var secAppUser = await _context.SecAppUser
                .SingleOrDefaultAsync(m => m.SecAppUserId == id);
            if (secAppUser == null)
            {
                return NotFound();
            }

            return View(secAppUser);
        }


        public IActionResult Create()
        {
            return View(new SecAppUserEditViewModel
            {

            });
        }
        #region snippet_Create
        // POST: Contacts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SecAppUserEditViewModel editModel)
        {
            if (!ModelState.IsValid)
            {
                return View(editModel);
            }

            var secAppUser = ViewModel_to_model(new SecAppUser(), editModel);

            secAppUser.OwnerID = _userManager.GetUserId(User);

            var isAuthorized = await _authorizationService.AuthorizeAsync(
                                                        User, secAppUser,
                                                        SecAppUserOperations.Create);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }
            var user = new ApplicationUser { UserName = editModel.Name, Email=editModel.Email};
            var result = await _userManager.CreateAsync(user,"Admin_0");
            var createdUser= await _userManager.FindByNameAsync(user.UserName);
            secAppUser.OwnerID = createdUser.Id;
            if (result.Succeeded)
            {
                secAppUser.isFirstLogin = true;
                _context.Add(secAppUser);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else
            {
                editModel.errorResultViewModel =
                    new ErrorResultViewModel
                    {
                        Text = "Creating a new user has failed.",
                        ErrorMessage = "Description: " + result.Errors.FirstOrDefault().Description,
                        EndText = "Change a few things up and try submitting again.",
                        isError = true
                    };
                return View(editModel);
            }
        }
        #endregion

        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var secAppUser = await _context.SecAppUser.SingleOrDefaultAsync(
        //                                                m => m.SecAppUserId == id);
        //    if (secAppUser == null)
        //    {
        //        return NotFound();
        //    }

        //    var isAuthorized = await _authorizationService.AuthorizeAsync(
        //                                                User, secAppUser,
        //                                                SecAppUserOperations.Update);
        //    if (!isAuthorized.Succeeded)
        //    {
        //        return new ChallengeResult();
        //    }

        //    //var editModel = Model_to_viewModel(secAppUser);

        //    return View(secAppUser);
        //}

        //// POST: Contacts/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, SecAppUserEditViewModel editModel)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(editModel);
        //    }

        //    // Fetch Contact from DB to get OwnerID.
        //    var secAppUser = await _context.SecAppUser.SingleOrDefaultAsync(m => m.SecAppUserId == id);
        //    if (secAppUser == null)
        //    {
        //        return NotFound();
        //    }

        //    var isAuthorized = await _authorizationService.AuthorizeAsync(User, secAppUser,
        //                                                        SecAppUserOperations.Update);
        //    if (!isAuthorized.Succeeded)
        //    {
        //        return new ChallengeResult();
        //    }

        //    secAppUser = ViewModel_to_model(secAppUser, editModel);

        //    if (secAppUser.Status == UserStatus.Approved)
        //    {
        //        // If the contact is updated after approval, 
        //        // and the user cannot approve set the status back to submitted
        //        var canApprove = await _authorizationService.AuthorizeAsync(User, secAppUser,
        //                                SecAppUserOperations.Approve);

        //        if (!canApprove.Succeeded)
        //            secAppUser.Status = UserStatus.Submitted;
        //    }

        //    _context.Update(secAppUser);
        //    await _context.SaveChangesAsync();

        //    return RedirectToAction("Index");
        //}

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var secAppUser = await _context.SecAppUser.SingleOrDefaultAsync(m => m.SecAppUserId == id);
            if (secAppUser == null)
            {
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, secAppUser,
                                        SecAppUserOperations.Delete);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }
            if (secAppUser.Name == "admin")
            {
                return View("ErrorResultPartial", new SecAppUserDeleteViewModel
                {
                    errorResultViewModel = new ErrorResultViewModel
                    {
                        Text = "You can't delete the admin account",
                        isError = true
                    }
                });
            }
            SecAppUserDeleteViewModel secAppUserDeleteViewModel = new SecAppUserDeleteViewModel { Email = secAppUser.Email, Name = secAppUser.Name, OwnerID = secAppUser.OwnerID, Status = secAppUser.Status };
            return View(secAppUserDeleteViewModel);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var secAppUser = await _context.SecAppUser.SingleOrDefaultAsync(m => m.SecAppUserId == id);

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, secAppUser,
                                        SecAppUserOperations.Delete);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }

            _context.SecAppUser.Remove(secAppUser);
            await _context.SaveChangesAsync();
            var userToDelete = await _userManager.FindByNameAsync(secAppUser.Name);
            var result = await _userManager.DeleteAsync(userToDelete);
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View("Delete",new SecAppUserDeleteViewModel{errorResultViewModel= new ErrorResultViewModel
                {
                    Text = "Deleting this user has failed.",
                    ErrorMessage = "Description: " + result.Errors.FirstOrDefault().Description,
                    EndText= "",
                    isError = true
                } });
            }
        }

        private bool SecAppUserExists(int id)
        {
            return _context.SecAppUser.Any(e => e.SecAppUserId == id);
        }

        #region SetStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetStatus(int id, UserStatus status)
        {
            var secAppUser = await _context.SecAppUser.SingleOrDefaultAsync(m => m.SecAppUserId == id);

            var secAppUserOperation = (status == UserStatus.Approved) ? SecAppUserOperations.Approve
                                                                      : SecAppUserOperations.Reject;

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, secAppUser,
                                        secAppUserOperation);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }
            secAppUser.Status = status;
            _context.SecAppUser.Update(secAppUser);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        #endregion

        #region Set/Unset Role to Approved
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SecCamUserAdministrators")]
        public async Task<IActionResult> SetRoleApproved(int id, UserStatus status)
        {
            var secAppUser = await _context.SecAppUser.SingleOrDefaultAsync(m => m.SecAppUserId == id);

            var secAppUserOperation = (status == UserStatus.Approved) ? SecAppUserOperations.Approve
                                                                      : SecAppUserOperations.Reject;

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, secAppUser,
                                        secAppUserOperation);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }
            secAppUser.Status = status;
            _context.SecAppUser.Update(secAppUser);
            await _context.SaveChangesAsync();
            string userId = secAppUser.OwnerID;
            var user = await _userManager.FindByIdAsync(userId);

            IdentityResult IR = null;

            if (!await _roleManager.RoleExistsAsync(Constants.ApprovedUserRole))
            {
                IR = await _roleManager.CreateAsync(new IdentityRole(Constants.ApprovedUserRole));
            }

            var result = await _userManager.AddToRoleAsync(user, Constants.ApprovedUserRole);
            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SecCamUserAdministrators")]
        public async Task<IActionResult> SetRoleDisApproved(int id, UserStatus status)
        {
            var secAppUser = await _context.SecAppUser.SingleOrDefaultAsync(m => m.SecAppUserId == id);

            var secAppUserOperation = (status == UserStatus.Approved) ? SecAppUserOperations.Approve
                                                                      : SecAppUserOperations.Reject;

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, secAppUser,
                                        secAppUserOperation);
            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }
            secAppUser.Status = status;
            _context.SecAppUser.Update(secAppUser);
            await _context.SaveChangesAsync();
            string userId = secAppUser.OwnerID;
            var user = await _userManager.FindByIdAsync(userId);
            var result = await _userManager.RemoveFromRoleAsync(user, Constants.ApprovedUserRole);
            return RedirectToAction("Index");
        }

        #endregion

        private SecAppUser ViewModel_to_model(SecAppUser secAppUser, SecAppUserEditViewModel editModel)
        {
            secAppUser.Email = editModel.Email;
            secAppUser.Name = editModel.Name;

            return secAppUser;
        }

        private SecAppUserEditViewModel Model_to_viewModel(SecAppUser secAppUser)
        {
            var editModel = new SecAppUserEditViewModel();

            editModel.SecAppUserId = secAppUser.SecAppUserId;
            editModel.Email = secAppUser.Email;
            editModel.Name = secAppUser.Name;

            return editModel;
        }
    }
}
