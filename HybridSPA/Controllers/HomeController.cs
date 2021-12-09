using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using HybridSPA.Utils;

namespace HybridSPA.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Name = ClaimsPrincipal.Current.FindFirst("name").Value;
            ViewBag.AuthorizationRequest = string.Empty;

            // The object ID claim will only be emitted for work or school accounts at this time.
            Claim oid = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");
            ViewBag.ObjectId = oid == null ? string.Empty : oid.Value;

            // The 'preferred_username' claim can be used for showing the user's primary way of identifying themselves
            ViewBag.Username = ClaimsPrincipal.Current.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn").Value;

            // The subject or nameidentifier claim can be used to uniquely identify the user
            ViewBag.Subject = ClaimsPrincipal.Current.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contacts.";

            return View();
        }

        public async Task<ActionResult> ViewProfile()
        {
            IConfidentialClientApplication app = MsalAppBuilder.BuildConfidentialClientApplication();
            AuthenticationResult result = null;
            var accountId = ClaimsPrincipal.Current.GetAccountId();
            var account = await app.GetAccountAsync(accountId);
            string[] scopes = { "user.read" };

            var spaAuthCode = HttpContext.Session["Spa_Auth_Code"];

            ViewBag.SpaAuthCode = spaAuthCode as string;
            ViewBag.Account = account as IAccount;

            try
            {
                // try to get token silently
                result = await app.AcquireTokenSilent(scopes, account).ExecuteAsync().ConfigureAwait(false);
            }
            catch (MsalUiRequiredException)
            {
                ViewBag.Relogin = "true";
                return View();
            }
            catch (Exception eee)
            {
                ViewBag.Error = "An error has occurred. Details: " + eee.Message;
                return View();
            }

            return View();
        }
               

        public void RefreshSession()
        {
            HttpContext.GetOwinContext().Authentication.Challenge(
                new AuthenticationProperties { RedirectUri = "/Home/ViewProfile" },
                OpenIdConnectAuthenticationDefaults.AuthenticationType);
        }
    }
}