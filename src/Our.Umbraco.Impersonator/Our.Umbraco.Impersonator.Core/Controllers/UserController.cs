using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Our.Umbraco.Impersonator.Core.Models;
using System.Text;
using System.Text.Json;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Security;
using UmbracoConstants = Umbraco.Cms.Core.Constants;

namespace Our.Umbraco.Impersonator.Core.Controllers;

[MapToApi(Constants.ApiName)]
[ApiExplorerSettings(GroupName = Constants.ApiTitle)]
[ApiController]
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
[VersionedApiBackOfficeRoute("impersonator-api/user")]
public class UserController : Controller
{
    private readonly IUserService _userService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
    private readonly IBackOfficeSignInManager _signInManager;

    public UserController(
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

    private ImpersonatedUserId? GetImpersonatingUserId()
    {
        var impersonatedUserIdString = HttpContext.Session.GetString(Constants.SessionUserIdKey);
        if (impersonatedUserIdString == null) return null;

        var impersonatedUserId = JsonSerializer.Deserialize<ImpersonatedUserId>(impersonatedUserIdString);

        if (impersonatedUserId == null) return null;

        if (impersonatedUserId.SessionId != HttpContext.Session.Id)
        {
            HttpContext.Session.Remove(Constants.SessionUserIdKey);
            return null;
        }

        return impersonatedUserId;
    }

    [HttpGet]
    public Guid? GetImpersonatingUser()
    {
        return GetImpersonatingUserId()?.ImpersonatingUserId;
    }

    [HttpDelete]
    public async Task<ImpersonationResult> EndImpersonation()
    {
        if (_backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser == null)
        {
            return ImpersonationResult.NotSignedIn;
        }
        var impersonatingUserId = GetImpersonatingUserId();
        if (impersonatingUserId == null)
        {
            return ImpersonationResult.Success;
        }
        var userById = await _userService.GetAsync(impersonatingUserId.ImpersonatingUserId);
        if (userById != null)
        {
            var user = _umbracoMapper.Map<BackOfficeIdentityUser>(userById);
            ArgumentNullException.ThrowIfNull(user);

            HttpContext.Session.Remove(Constants.SessionUserIdKey);
            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(user, false);

            return ImpersonationResult.Success;
        }
        return ImpersonationResult.UserNotFound;
    }

    [HttpPut]
    public async Task<ImpersonationResult> Impersonate(Guid id)
    {
        var currentUser = _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        if (currentUser == null)
        {
            return ImpersonationResult.NotSignedIn;
        }
        if (currentUser.AllowedSections.Contains(UmbracoConstants.Applications.Users))
        {
            var userById = await _userService.GetAsync(id);
            if (userById != null)
            {
                var user = _umbracoMapper.Map<BackOfficeIdentityUser>(userById);
                ArgumentNullException.ThrowIfNull(user);

                HttpContext.Session.Remove(Constants.SessionUserIdKey);
                await _signInManager.SignOutAsync();
                await _signInManager.SignInAsync(user, true);
                HttpContext.Session.SetString(Constants.SessionUserIdKey, JsonSerializer.Serialize(new ImpersonatedUserId(id, currentUser.Key, HttpContext.Session.Id)));
                return ImpersonationResult.Success;
            }
            return ImpersonationResult.UserNotFound;
        }
        return ImpersonationResult.AccessDenied;
    }
}
