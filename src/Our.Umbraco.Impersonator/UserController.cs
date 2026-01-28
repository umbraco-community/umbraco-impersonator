using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
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
    [Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
    [MapToApi("impersonator")]
    public class ImpersonatorUserController : ManagementApiControllerBase
    {
        private const string IMPERSONATOR_USER_ID = "Impersonator.User.Id";

        private readonly IUserService _userService;
        private readonly IUmbracoMapper _umbracoMapper;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly IBackOfficeSignInManager _signInManager;

        public ImpersonatorUserController(
            IUserService userService,
            IUmbracoMapper umbracoMapper,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
            IBackOfficeSignInManager signInManager
            )
        {
            _userService = userService;
            _umbracoMapper = umbracoMapper;
            _backofficeSecurityAccessor = backOfficeSecurityAccessor;
            _signInManager = signInManager;
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

        [HttpGet("GetImpersonatingUserHash")]
        public async Task<string> GetImpersonatingUserHash()
        {
            ImpersonatedUserId impersonatingUserId = GetImpersonatingUserId();
            if (impersonatingUserId == null)
            {
                return null;
            }
            var userById = await _userService.GetAsync(impersonatingUserId.UserId);
            if (userById != null)
            {
                return "HASH";// _umbracoMapper.Map<UserBasic>(userById)?.EmailHash;
            }
            return null;
        }

        [HttpPost("EndImpersonation")]
        public async Task<IActionResult> EndImpersonation()
        {
            if (_backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser == null)
            {
                return BadRequest("notSignedIn");
            }

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
        public async Task<IActionResult> Impersonate(string id)
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

            if (!Guid.TryParse(id, out Guid userGuid))
            {
                return BadRequest("invalidUserId");
            }

            var userById = await _userService.GetAsync(userGuid);
            if (userById == null)
            {
                return NotFound("userNotFound");
            }

            var user = _umbracoMapper.Map<BackOfficeIdentityUser>(userById);

            // Store the impersonation info BEFORE signing in
            HttpContext.Session.SetString(IMPERSONATOR_USER_ID,
                JsonSerializer.Serialize(new ImpersonatedUserId(userGuid, currentUser.Id, HttpContext.Session.Id)));

            // Sign out the current user
            await _signInManager.SignOutAsync();

            // Sign in as the impersonated user - this sets the authentication cookie
            await _signInManager.SignInAsync(user, isPersistent: true);

            // Return success - the frontend will handle the OAuth flow
            return Ok("success");
        }
    }
}
