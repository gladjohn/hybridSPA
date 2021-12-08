﻿using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Identity.Client;
using HybridSPA.Utils;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web;

[assembly: OwinStartup(typeof(HybridSPA.Startup))]

namespace HybridSPA
{
    public class Startup
    {
        /// <summary>
        /// Configure OWIN to use OpenIdConnect
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            // Custom middleware initialization. This is activated when the code obtained from a code_grant is present in the querystring (&code=<code>).
            app.UseOAuth2CodeRedeemer(
                new OAuth2CodeRedeemerOptions
                {
                    ClientId = AuthenticationConfig.ClientId,
                    ClientSecret = AuthenticationConfig.ClientSecret,
                    RedirectUri = AuthenticationConfig.RedirectUri
                });

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    // The `Authority` represents the v2.0 endpoint - https://login.microsoftonline.com/common/v2.0
                    Authority = AuthenticationConfig.Authority,
                    ClientId = AuthenticationConfig.ClientId,
                    RedirectUri = AuthenticationConfig.RedirectUri,
                    PostLogoutRedirectUri = AuthenticationConfig.RedirectUri,
                    Scope = AuthenticationConfig.BasicSignInScopes + " Mail.Read", // a basic set of permissions for user sign in & profile access "openid profile offline_access"
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        // In a real application you would use IssuerValidator for additional checks, like making sure the user's organization has signed up for your app.
                        //     IssuerValidator = (issuer, token, tvp) =>
                        //     {
                        //        //if(MyCustomTenantValidation(issuer))
                        //        return issuer;
                        //        //else
                        //        //    throw new SecurityTokenInvalidIssuerException("Invalid issuer");
                        //    },
                        NameClaimType = "name",
                        SaveSigninToken = true,
                    },
                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        AuthorizationCodeReceived = OnAuthorizationCodeReceived,
                        AuthenticationFailed = OnAuthenticationFailed,
                        RedirectToIdentityProvider = OnRedirectToIdentityProvider,
                    },
                    // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/samesite/owin-samesite
                    CookieManager = new SameSiteCookieManager(new SystemWebCookieManager())
                });

        }

        private Task OnRedirectToIdentityProvider(RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> arg)
        {
            arg.ProtocolMessage.SetParameter("myNewParameter", "its Value");
            return Task.CompletedTask;
        }

        private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedNotification context)
        {
            try
            {
                // on a request controller 
                Dictionary<string, string> extraQueryParameters = new Dictionary<string, string>();
                extraQueryParameters.Add("dc", "ESTS-PUB-WUS2-AZ1-FD000-TEST1");
                extraQueryParameters.Add("hybridspa", "true");

                // Upon successful sign in, get the access token & cache it using MSAL
                IConfidentialClientApplication clientApp = MsalAppBuilder.BuildConfidentialClientApplication();
                AuthenticationResult result = await clientApp.AcquireTokenByAuthorizationCode(new[] { "Mail.read" }, context.Code)
                    .WithSpaAuthorizationCode(true)
                    .WithExtraQueryParameters(extraQueryParameters)
                    .ExecuteAsync();

                List<Claim> oauthClaims = new List<Claim>
                {
                    new Claim("access_token", result.AccessToken),
                    new Claim("id_token", result.IdToken),
                    new Claim("spa_auth_code", result.SpaAuthCode)
                };

                //context.HandleCodeRedemption(result.AccessToken, context.ProtocolMessage.IdToken);
                //AuthenticationConfig.SpaAuthCode = result.SpaAuthCode;

                //HttpContext.Current.Items.Add("Spa_Auth_Code", result.SpaAuthCode);

                HttpContext.Current.Session.Add("Spa_Auth_Code", result.SpaAuthCode);

            }
            catch
            {
 
            }
        }

        private Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            notification.HandleResponse();
            notification.Response.Redirect("/Error?message=" + notification.Exception.Message);
            return Task.FromResult(0);
        }
    }
}
