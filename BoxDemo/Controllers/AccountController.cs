using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security;
using System.Security.Claims;
using BoxDemo.Helpers;
using System.Threading.Tasks;
using Box.V2;
using Box.V2.Models;

namespace BoxDemo.Controllers
{
    public static class ControllerExtensions
    {
        public static string GetCurrentUserEmail(this Controller controller)
        {
            ClaimsPrincipal user = controller.HttpContext.GetOwinContext().Authentication.User;
            string email = user.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress").Value;
            return email;
        }
    }

    public class AccountController : Controller
    {
        [Authorize]
        public async Task<ActionResult> Setup()
        {
            string email = this.GetCurrentUserEmail();
            BoxUser user = await BoxHelper.CreateBoxUser(email);
            BoxClient userClient = BoxHelper.UserClient(user.Id);
            await BoxHelper.Setup(userClient);
            return RedirectToAction("index", "home");
        }

        public void SignIn()
        {
            // Send an OpenID Connect sign-in request.
            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "/Account/Setup" },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }

        public void SignOut()
        {
            string callbackUrl = Url.Action("SignOutCallback", "Account", routeValues: null, protocol: Request.Url.Scheme);

            HttpContext.GetOwinContext().Authentication.SignOut(
                new AuthenticationProperties { RedirectUri = callbackUrl },
                OpenIdConnectAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType);
        }

        public ActionResult SignOutCallback()
        {
            if (Request.IsAuthenticated)
            {
                // Redirect to home page if the user is authenticated.
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
    }
}
