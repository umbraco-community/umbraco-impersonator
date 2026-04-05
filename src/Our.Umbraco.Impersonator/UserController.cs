using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Our.Umbraco.Impersonator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Cms.Web.Common.Security;

namespace Our.Umbraco.Impersonator
{
    [ApiController]
    [BackOfficeRoute("impersonator/api/v{version:apiVersion}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminAccess)]
    [MapToApi("impersonator")]
    public class ImpersonatorUserController : ManagementApiControllerBase
    {
        private const string IMPERSONATOR_USER_ID = "Impersonator.User.Id";

        private readonly IUserService _userService;
        private readonly IUmbracoMapper _umbracoMapper;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly IBackOfficeSignInManager _signInManager;
        private readonly IBackOfficeUserManager _backOfficeUserManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ImpersonatorUserController(
            IUserService userService,
            IUmbracoMapper umbracoMapper,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
            IBackOfficeSignInManager signInManager,
            IBackOfficeUserManager backOfficeUserManager,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _userService = userService;
            _umbracoMapper = umbracoMapper;
            _backofficeSecurityAccessor = backOfficeSecurityAccessor;
            _signInManager = signInManager;
            _backOfficeUserManager = backOfficeUserManager;
            _httpContextAccessor = httpContextAccessor;
        }


        private ImpersonatedUserId GetImpersonatingUserId()
        {
            var impersonatedUserIdString = HttpContext.Session.GetString(IMPERSONATOR_USER_ID);
            if (impersonatedUserIdString == null) return null;

            var impersonatedUserId = JsonSerializer.Deserialize<ImpersonatedUserId>(impersonatedUserIdString);

            if (impersonatedUserId == null) return null;

            if (impersonatedUserId.SessionId != HttpContext.Session.Id)
            {
                HttpContext.Session.Remove(IMPERSONATOR_USER_ID);
                return null;
            }

            return impersonatedUserId;
        }

        [HttpGet("GetImpersonatingUserName")]
        [AllowAnonymous]
        public async Task<string> GetImpersonatingUserName()
        {
            ImpersonatedUserId impersonatingUserId = GetImpersonatingUserId();
            if (impersonatingUserId == null)
            {
                return null;
            }
            var userById = await _userService.GetAsync(impersonatingUserId.UserId);
            if (userById != null)
            {
                return userById.Name;
            }
            return null;
        }

        [HttpGet("GetUsers")]
        public async Task<IEnumerable<SimpleUserModel>> GetUsers()
        {
            Guid currentUserKey = CurrentUserKey(_backofficeSecurityAccessor);
            var allUsers = await _userService.GetAllAsync(currentUserKey, 0, Int32.MaxValue);
            return allUsers.Result?.Items.Where(a => a.Key != currentUserKey).Select(a => new SimpleUserModel()
            {
                Key = a.Key,
                Name = a.Name
            });
        }

        [HttpPost("EndImpersonation")]
        [AllowAnonymous]
        public async Task<IActionResult> EndImpersonation()
        {
            ImpersonatedUserId impersonatingUserId = GetImpersonatingUserId();
            if (impersonatingUserId == null)
            {
                return Ok("success");
            }

            var userById = _userService.GetUserById(impersonatingUserId.ImpersonatingUserId);
            if (userById == null)
            {
                return NotFound("userNotFound");
            }

            var user = _umbracoMapper.Map<BackOfficeIdentityUser>(userById);

            // We have a bunch of auth cookies that contain user info, clear them out (17.3)
            RemoveAuthCookies();

            // Remove the impersonation session data
            HttpContext.Session.Remove(IMPERSONATOR_USER_ID);

            // Sign out the impersonated user
            await _signInManager.SignOutAsync();

            // Sign in as the original user - this sets the authentication cookie
            await _signInManager.SignInAsync(user, isPersistent: false);

            // Return success - the frontend will handle the OAuth flow
            return Ok("success");
        }

        [HttpPost("Impersonate")]
        public async Task<IActionResult> Impersonate(Guid id)
        {
            var currentUser = _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
            if (currentUser == null)
            {
                return BadRequest("notSignedIn");
            }
            if (!currentUser.AllowedSections.Contains(Constants.Applications.Users))
            {
                return Unauthorized("notAdministrator");
            }

            var userById = await _userService.GetAsync(id);
            if (userById == null)
            {
                return NotFound("userNotFound");
            }


            var user = _umbracoMapper.Map<BackOfficeIdentityUser>(userById);

            // We have a bunch of auth cookies that contain user info, clear them out (17.3)
            RemoveAuthCookies();

            // Store the impersonation info BEFORE signing in
            HttpContext.Session.SetString(IMPERSONATOR_USER_ID,
                JsonSerializer.Serialize(new ImpersonatedUserId(id, currentUser.Id, HttpContext.Session.Id)));

            // Sign out the current user
            await _signInManager.SignOutAsync();

            // Sign in as the impersonated user - this sets the authentication cookie
            await _signInManager.SignInAsync(user, isPersistent: true);

            // Return success - the frontend will handle the OAuth flow
            return Ok("success");
        }

        private void RemoveAuthCookies()
        {
            Response.Cookies.Delete("__Host-umbAccessToken", new CookieOptions
            {
                Path = "/",
                Secure = true
            });
            Response.Cookies.Delete("__Host-umbRefreshToken", new CookieOptions
            {
                Path = "/",
                Secure = true
            });
            //Response.Cookies.Delete("UMB_SESSION", new CookieOptions
            //{
            //    Path = "/",
            //    Secure = true
            //});
            Response.Cookies.Delete("UMB_UCONTEXT", new CookieOptions
            {
                Path = "/",
                Secure = true
            });
            Response.Cookies.Delete("UMB_UCONTEXT_EXPOSED", new CookieOptions
            {
                Path = "/",
                Secure = true
            });
        }
    }
}
