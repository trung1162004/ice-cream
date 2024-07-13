using System.Security.Claims;
using Project.Helpers;
using Project.IdentitySettings;
using Project.Models.Identity;
using Project.Models.ViewModels;
using Project.Models;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Project.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly EmailHelper _emailHelper;
        private readonly RoleManager<AppRole> _roleManager;
      

        public UserController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, EmailHelper emailHelper, RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailHelper = emailHelper;
           
            _roleManager = roleManager;
        }

        public IActionResult Register()
        {
            ClaimsPrincipal claimUser = HttpContext.User;

            if (claimUser.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            // Trả về View để người dùng đăng nhập
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(SignUpViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var roleExists = await _roleManager.RoleExistsAsync("Users");
                if (!roleExists)
                {
                    // Nếu vai trò không tồn tại, tạo mới
                    await _roleManager.CreateAsync(new AppRole("Users"));
                }

                var user = new AppUser
                {
                    UserName = viewModel.UserName,
                    FullName = viewModel.FullName,
                    Email = viewModel.Email,
                    PhoneNumber = viewModel.PhoneNumber,
                    Gender = viewModel.Gender,
                    BirthDay = viewModel.BirthDay,
                    TwoFactorType = Project.Models.TwoFactorType.None,
                    Address = viewModel.Address,
                    CreatedOn = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, viewModel.Password);
                if (result.Succeeded)
                {
                    // Thêm người dùng vào vai trò
                    await _userManager.AddToRoleAsync(user, "Users");

                    var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = Url.Action("ConfirmEmail", "User", new
                    {
                        userId = user.Id,
                        token = confirmationToken
                    }, HttpContext.Request.Scheme);

                    await _emailHelper.SendAsync(new()
                    {
                        Subject = "Confirm e-mail",
                        Body = $"Please <a href='{confirmationLink}'>click</a> to confirm your e-mail address.",
                        To = user.Email
                    });
                    TempData["ConfirmEmailMessage"] = "Register successful.";
                    return RedirectToAction("Login");
                }
                result.Errors.ToList().ForEach(f => ModelState.AddModelError(string.Empty, f.Description));
            }
            return View(viewModel);
        }


        public IActionResult Login(string returnUrl)
        {
            // Kiểm tra và lưu returnUrl vào TempData
            if (!string.IsNullOrEmpty(returnUrl))
            {
                TempData["ReturnUrl"] = returnUrl;
            }
            if (TempData.ContainsKey("ConfirmEmailMessage"))
            {
                ViewBag.ConfirmEmailMessage = TempData["ConfirmEmailMessage"];
            }


            // Kiểm tra xem người dùng đã xác thực hay chưa
            if (User.Identity.IsAuthenticated)
            {
                // Người dùng đã xác thực, chuyển hướng đến trang chủ
                return RedirectToAction("Index", "Home");
            }

            // Trả về View để người dùng đăng nhập
            return View();
        }

        //private IActionResult RedirectToLocal(string returnUrl)
        //{
        //    // Kiểm tra xem returnUrl có hợp lệ hay không để tránh Redirect Loop
        //    if (Url.IsLocalUrl(returnUrl))
        //    {
        //        return Redirect(returnUrl);
        //    }
        //    else
        //    {
        //        return RedirectToAction("Index", "Home");
        //    }
        //}


        [HttpPost]
        public async Task<IActionResult> Login(SignInViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(viewModel.Email);
                if (user != null)
                {
                    await _signInManager.SignOutAsync();

                    var result = await _signInManager.PasswordSignInAsync(user, viewModel.Password, viewModel.RememberMe, true);

                    if (result.Succeeded)
                    {
                        await _userManager.ResetAccessFailedCountAsync(user);
                        await _userManager.SetLockoutEndDateAsync(user, null);

                        HttpContext.Session.SetString("uid", user.Id);
                        HttpContext.Session.SetString("name", user.FullName);
                        HttpContext.Session.SetString("email", user.Email);
                        HttpContext.Session.SetString("phone", user.PhoneNumber);
                        HttpContext.Session.SetString("address", user.Address);


                        // Kiểm tra vai trò của người dùng
                        var roles = await _userManager.GetRolesAsync(user);

                        if (roles.Contains("Admin"))
                        {
                            return RedirectToAction("Users", "Admin");
                        }
                        else if (roles.Contains("Users"))
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else if (result.RequiresTwoFactor)
                    {
                        return RedirectToAction("TwoFactorLogin", new { ReturnUrl = TempData["ReturnUrl"] });
                    }
                    else if (result.IsLockedOut)
                    {
                        var lockoutEndUtc = await _userManager.GetLockoutEndDateAsync(user);
                        var timeLeft = lockoutEndUtc.Value - DateTime.UtcNow;
                        ModelState.AddModelError(string.Empty, $"This account has been locked out, please try again {timeLeft.Minutes} minutes later.");
                    }
                    else if (result.IsNotAllowed)
                    {
                        ModelState.AddModelError(string.Empty, "You need to confirm your e-mail address.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid e-mail or password.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid e-mail or password.");
                }
            }
            return View(viewModel);
        }
      

    

        public async Task<IActionResult> Logout() {
            await _signInManager.SignOutAsync();
             HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");

        }



        [Authorize(Roles = "Users")]
        public async Task<IActionResult> Profile()
        {
            var me = await _userManager.FindByNameAsync(User.Identity?.Name);
            if (me == null)
            {
                return RedirectToAction("Index", "Home");
            }    

            //}
            return View(me.Adapt<UpdateProfileViewModel>());
        }

        [HttpPost]
        public async Task<IActionResult> Profile(UpdateProfileViewModel viewModel)
        {
            if (ModelState.IsValid) 
            {
                try
                {
                    var me = await _userManager.FindByNameAsync(User.Identity?.Name);
                    if (me != null)
                    {
                        me.UserName = viewModel.UserName;
                        me.FullName = viewModel.FullName;
                        me.Email = viewModel.Email;
                        me.PhoneNumber = viewModel.PhoneNumber;
                        me.Gender = viewModel.Gender;
                        me.BirthDay = viewModel.BirthDay;
                        me.Address = viewModel.Address;

                        var result = await _userManager.UpdateAsync(me);
                        if (result.Succeeded)
                        {
                            await _userManager.UpdateSecurityStampAsync(me);
                            // Skip sign-out and sign-in

                            // Optionally, you can add a success message to TempData
                            TempData["SuccessMessage"] = "Profile updated successfully.";

                            return RedirectToAction("Profile", "User");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle the exception (logging, notifying the user, etc.)
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the profile. Please try again.");
                    // Log the exception, for example: _logger.LogError(ex, "Error message");
                }
            }

            // If it reaches here, something went wrong, so we return back to the view with the model
            return View(viewModel);
        }


        [Authorize(Roles ="Users")]
        public IActionResult ChangePassword()
        {
         
           
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var me = await _userManager.FindByNameAsync(User.Identity?.Name);

                var passwordValid = await _userManager.CheckPasswordAsync(me, viewModel.Password);
                if (passwordValid)
                {
                    // Check if the new password is different from the old password
                    if (viewModel.Password == viewModel.NewPassword)
                    {
                        ModelState.AddModelError(string.Empty, "New password must be different from the old password.");
                        return View();
                    }

                    var result = await _userManager.ChangePasswordAsync(me, viewModel.Password, viewModel.NewPassword);
                    if (result.Succeeded)
                    {
                        await _userManager.UpdateSecurityStampAsync(me);

                        await _signInManager.SignOutAsync();
                        await _signInManager.SignInAsync(me, true);

                        return RedirectToAction("Index", "Home");
                    }
                    result.Errors.ToList().ForEach(f => ModelState.AddModelError(string.Empty, f.Description));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Password is invalid.");
                }
            }

            return View();
        }

        public IActionResult ForgotPassword()
        {
            ClaimsPrincipal claimUser = HttpContext.User;

            if (claimUser.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            // Trả về View để người dùng đăng nhập
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(viewModel.Email);
                if (user != null)
                {
                    var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                    var passwordLink = Url.Action("ResetPassword", "User", new
                    {
                        userId = user.Id,
                        token = passwordResetToken
                    }, HttpContext.Request.Scheme);

                    await _emailHelper.SendAsync(new()
                    {
                        Subject = "Reset password",
                        Body = $"Please <a href='{passwordLink}'>click</a> to reset your password.",
                        To = user.Email
                    });

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }
            return View(viewModel);
        }

        public IActionResult ResetPassword(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("Login", "User");
            }

            return View(new ResetPasswordViewModel
            {
                UserId = userId,
                Token = token
            });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(viewModel.UserId);
                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, viewModel.Token, viewModel.Password);
                    if (result.Succeeded)
                    {
                        await _userManager.UpdateSecurityStampAsync(user);

                        return RedirectToAction("Login", "User");
                    }
                    else
                    {
                        result.Errors.ToList().ForEach(f => ModelState.AddModelError(string.Empty, f.Description));
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }
            return View(viewModel);
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return RedirectToAction("Login");
                }
            }
            return RedirectToAction("Index", "Home");
        }

      

        public IActionResult GoogleLogin(string returnUrl)
        {
            var redirectUrl = Url.Action("signin-google", "User", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return new ChallengeResult("Google", properties);
        }


        [Route("signin-google")]
        public async Task<IActionResult> signingoogle(string ReturnUrl = "/")
        {
            var loginInfo = await _signInManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            var externalLoginResult = await _signInManager.ExternalLoginSignInAsync(loginInfo.LoginProvider, loginInfo.ProviderKey, true);
            if (externalLoginResult.Succeeded)
            {
                return Redirect(ReturnUrl);
            }

            var externalUserId = loginInfo.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var externalEmail = loginInfo.Principal.FindFirst(ClaimTypes.Email)?.Value;

            var existUser = await _userManager.FindByEmailAsync(externalEmail);
            if (existUser == null)
            {
                var user = new AppUser { Email = externalEmail };

                if (loginInfo.Principal.HasClaim(c => c.Type == ClaimTypes.Name))
                {
                    var userName = loginInfo.Principal.FindFirst(ClaimTypes.Name)?.Value;
                    if (userName != null)
                    {
                        userName = userName.Replace(' ', '-').ToLower() + externalUserId?.Substring(0, 5);
                        user.UserName = userName;
                    }
                    else
                    {
                        user.UserName = user.Email;
                    }
                }
                else
                {
                    user.UserName = user.Email;
                }

                var createResult = await _userManager.CreateAsync(user);
                if (createResult.Succeeded)
                {
                    var loginResult = await _userManager.AddLoginAsync(user, loginInfo);
                    if (loginResult.Succeeded)
                    {
                        // await SignInManager.SignInAsync(user, true);
                        await _signInManager.ExternalLoginSignInAsync(loginInfo.LoginProvider, loginInfo.ProviderKey, true);
                        await _userManager.AddToRoleAsync(user, "Users");
                        return Redirect(ReturnUrl);
                    }
                    else
                    {
                        loginResult.Errors.ToList().ForEach(f => ModelState.AddModelError(string.Empty, f.Description));
                    }
                }
                else
                {
                    createResult.Errors.ToList().ForEach(f => ModelState.AddModelError(string.Empty, f.Description));
                }
            }
            else
            {
                var loginResult = await _userManager.AddLoginAsync(existUser, loginInfo);
                if (loginResult.Succeeded)
                {
                    // await SignInManager.SignInAsync(user, true);
                    await _signInManager.ExternalLoginSignInAsync(loginInfo.LoginProvider, loginInfo.ProviderKey, true);
                    return Redirect(ReturnUrl);
                }
                else
                {
                    loginResult.Errors.ToList().ForEach(f => ModelState.AddModelError(string.Empty, f.Description));
                }
            }

            var errors = ModelState.Values.SelectMany(s => s.Errors).Select(s => s.ErrorMessage).ToList();

            return View("Error", errors);
        }

     
    }
}