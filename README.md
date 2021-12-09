# hybridSPA

## Problem Description

Some applications like SharePoint and OWA are built as "hybrid" web applications, which are built with server-side and client-side components (e.g. an ASP.net  web application hosting a React single-page application). In these scenarios, the application will likely need authentication both client-side (e.g. a public client using MSAL.js) and server-side (e.g. a confidential client using MSAL.net ), and each application context will need to acquire its own tokens. This requires that the application complete two round-trips to the eSTS /authorize endpoint, one to get tokens for the confidential client, and a second to get tokens for the public client. Historically, the second leg of this flow is done silently via the implicit flow (e.g. ADAL.js or MSAL.js v1). However, when third-party cookies are blocked in the user's browser due to the browser's privacy features (e.g. Safari ITP), MSAL.js is unable to complete the second round-trip to the /authorize endpoint silently, resulting in the application needing to prompt the user to interactively navigate to the /authorize endpoint (via popup or full-page redirect) to complete authentication for the public client. And while MSAL.js v2 does not require third-party cookies for silent token renewal, it still relies on third-party cookies to silently authenticate the user when the application first loads.

## Hybrid SPA sample application

This sample demonstrates how to use MSAL.js v2 and MSAL .Net together in a "hybrid" application that performs both server-side and client-side authenication. 

It shows how to use two new APIs, `WithSpaAuthorizationCode` on `AcquireTokenByAuthorizationCode` in MSAL .Net and `acquireTokenByCode` in MSAL.js v2, to authenticate a user server-side using a confidential client, and then SSO that user client-side using a second authorization code that is returned to the confidential client and redeemed by the public client client-side. This helps mitigate user experience and performance concerns that arise when performing server-side and client-side authentication for the same user, especially when third-party cookies are blocked by the browser.

## Sample Overview

The application is implemented as an ASP.NET MVC project, while the web sign-on functionality is implemented via ASP.NET OpenId Connect OWIN middleware.

The sample also shows how to use MSAL.js V2 (Microsoft Authentication Library for JavaScript) to obtain an access token for Microsoft Graph. Specifically, the sample shows how to get the profile of the user from Microsoft Graph.

## How To Run This Sample

To run this sample, you'll need:

- [Visual Studio 2019](https://aka.ms/vsdownload)
- An Internet connection
- An Azure AD account

You can get an Office365 office subscription, which will give you both an Azure AD account and a mailbox, at [https://products.office.com/en-us/try](https://products.office.com/en-us/try).

### Step 1:  Clone or download this repository

From your shell or command line:

```Shell
git clone https://github.com/gladjohn/hybridSPA.git
```
### Step 2:  Register the sample application with your Azure Active Directory tenant

1. Clone the source code from the git repo.
2. In the Azure Portal, create a new app registration.
3. In the root `web.config` on this sample, add the client id for this application as `ClientId`.
4. Add your application authority (e.g. `https://login.microsoftonline.com/<tenant GUID>`) to the `web.config` file as `Authority`.
5. In the Azure Portal, under the **Authentication** tab for your application, add the following **Web** redirect URIs:
    1. `http://localhost:44320`
6. Also add the following **Single-page application** redirect URIs:
    1. `http://localhost:44320/auth/client-redirect`
7. Under **Implicit grant and hybrid flows**, check the boxes to enable **Access tokens** and **ID tokens**.
8. Under the **Certificats & secrets** tab, create a new client secret. Add this client secret to the `web.config` file as `ClientSecret`.
9. In the manifest editor, add the following optional ID token claims:
    1. `sid`
    1. `login_hint`
10. Under the **API permissions** tabs, add the `User.Read` scope from Microsoft Graph.
11. Build the application and click Start.

#### Configure the service project

1. Open the solution in Visual Studio.
1. Open the `web.config` file.
1. Find the app key `ClientId` and replace the existing value with the application ID (clientId) of the application copied from the Azure portal.
1. Find the app key `ClientSecret` and replace the existing value with the key you saved during the creation of the app, in the Azure portal.

## Sample web.config 
```config
  <appSettings>
    <add key="webpages:Version" value="3.0.0.0" />
    <add key="webpages:Enabled" value="false" />
    <add key="ClientValidationEnabled" value="true" />
    <add key="UnobtrusiveJavaScriptEnabled" value="true" />
    <add key="ClientId" value="App_ID/Client_ID" />
    <add key="ClientSecret" value="App_Secret" />
    <add key="AADInstance" value="https://login.microsoftonline.com/{0}{1}" />
    <add key="redirectUri" value="https://localhost:44320" />
    <add key="Tenant" value="common" />
    <add key="Authority" value="https://login.microsoftonline.com/<Tenant GUID>" />
  </appSettings>
```  
#### Configure the SPA 

1. Open the solution in Visual Studio.
1. Open the `hybridspa.js` file.
1. Find the app key `clientId` and replace the existing value with the application ID (clientId) of the application copied from the Azure portal.

> Note : We will not be using the client secret for MSAL.js.

## Sample hybridspa.js
```js
const msalInstance = new msal.PublicClientApplication({
    auth: {
        clientId: "client_id",
        redirectUri: "https://localhost:44320/auth/client-redirect",
        authority: "https://login.microsoftonline.com/f645ad92-e38d-4d1a-b510-d1b09a74a8ca"
    }
})

```  

### Step 3:  Run the sample

Clean the solution, rebuild the solution, and run it.

Once you run the `Hybrid SPA` web application, you are presented with the standard ASP.NET home page.
Click on the **Sign-in with Microsoft** link on top-right to trigger the log-in flow.

On the sign-in page, enter the name and password of a work/school account. 

As you sign in, the app will change the sign-in button into a greeting to the current user - and two new menu commands will appear: `Read Mail` and `Send Mail`.

![Post sign-in](./ReadmeFiles/Postsign-in.JPG)

Click on **Read Mail**: the app will show a dump of the last few messages from the current user's inbox, as they are received from the Microsoft Graph.

Click on **Send Mail**. As it is the first time you do so, you will receive a message informing you that for the app to receive the permissions to send mail as the user, the user needs to grant additional consent. The message offers a link to initiate the process.

![Incremental Consent Link](./ReadmeFiles/IncrementalConsentLink.jpg)

Click it, and you will be transported back to the consent experience, this time it lists just one permission, which is **Send mail as you**.

![Incremental Consent prompt](./ReadmeFiles/Incrementalconsent.JPG)

Once you have consented to this permission, you will be transported back to the application: but this time, you will be presented with a simple experience for authoring an email. Use it to compose and send an email to a mailbox you have access to. Send the message and verify you receive it correctly.

Hit the **sign-out** link on the top right corner.

Sign in again with the same user, and follow the exact same steps described so far. You will notice that the send mail experience appears right away and no longer forces you to grant extra consent, as your decision has been recorded in your previous session.

> Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../issues) page.


## About the code

Here there's a quick guide to the most interesting authentication-related bits of the sample.

### Sign in

As it is standard practice for ASP.NET MVC apps, the sign-in functionality is implemented with the OpenID Connect OWIN middleware. Here there's a relevant snippet from the middleware initialization:

```CSharp
app.UseOpenIdConnectAuthentication(
    new OpenIdConnectAuthenticationOptions
    {
        // The `Authority` represents the v2.0 endpoint - https://login.microsoftonline.com/common/v2.0
        Authority = Globals.Authority,
        ClientId = Globals.ClientId,
        RedirectUri = Globals.RedirectUri,
        PostLogoutRedirectUri = Globals.RedirectUri,
        Scope = Globals.BasicSignInScopes + " Mail.Read", // a basic set of permissions for user sign in & profile access "openid profile offline_access"
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
            //NameClaimType = "name",
        },
        Notifications = new OpenIdConnectAuthenticationNotifications()
        {
            AuthorizationCodeReceived = OnAuthorizationCodeReceived,
            AuthenticationFailed = OnAuthenticationFailed,
        }
    });
```

Important things to notice:

- The Authority points to the new authentication endpoint, which supports both personal and work and school accounts.
- the list of scopes includes both entries that are used for the sign-in function (`openid, email, profile`) and for the token acquisition function (`offline_access` is required to obtain refresh_tokens as well; `Mail.Read` is required for getting access tokens that can be used when requesting to read the user's mail).
- In this sample, the issuer validation is turned off, which means that anybody with an account can access the application. Real life applications would likely be more restrictive, limiting access only to those Azure AD tenants or Microsoft accounts associated to customers of the application itself. In other words, real life applications would likely also have a sign-up function - and the sign-in would enforce that only the users who previously signed up have access. For simplicity, this sample does not include sign up features.

### Initial token acquisition

This sample makes use of OpenId Connect hybrid flow, where at authentication time the app receives both sign in info, the  [id_token](https://docs.microsoft.com/en-us/azure/active-directory/develop/id-tokens)  and artifacts (in this case, an  [authorization code](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-auth-code-flow)) that the app can use for obtaining an [access token](https://docs.microsoft.com/en-us/azure/active-directory/develop/access-tokens). This access token can be used to access other resources - in this sample, the Microsoft Graph, for the purpose of reading the user's mailbox.

This sample shows how to use MSAL to redeem the authorization code into an access token, which is saved in a cache along with any other useful artifact (such as associated  [refresh_tokens](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-auth-code-flow#refresh-the-access-token)) so that it can be used later on in the application from the controllers' actions to fetch access tokens after they are expired.

The redemption takes place in the `AuthorizationCodeReceived` notification of the authorization middleware. Here there's the relevant code:

```CSharp
        private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedNotification context)
        {
            // Upon successful sign in, get the access token & cache it using MSAL
            IConfidentialClientApplication clientApp = MsalAppBuilder.BuildConfidentialClientApplication(new ClaimsPrincipal(context.AuthenticationTicket.Identity));
            AuthenticationResult result = await clientApp.AcquireTokenByAuthorizationCode(new[] { "Mail.Read" }, context.Code).ExecuteAsync();
        }
```

Important things to notice:

- The  `IConfidentialClientApplication`  is the primitive that MSAL uses to model the Web application. As such, it is initialized with the main application's coordinates.
- The scope requested by  `AcquireTokenByAuthorizationCode`  is just the one required for invoking the API targeted by the application as part of its essential features. We'll see later that the app allows for extra scopes, but you can ignore those at this point.
- The instance of `IConfidentialClientApplication` is created and attached to an instance of `MSALPerUserMemoryTokenCache`, which is a custom cache implementation that uses a shared instance of a [MemoryCache](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.caching.memorycache?view=netframework-4.8) to cache tokens. When it acquires the access token, MSAL also saves this token in its token cache. When any code in the rest of the project tries to acquire an access token for Microsoft Graph with the same scope (Mail.Read), MSAL will return the cached token.

The IConfidentialClientApplication is created in a separate function in the `MsalAppBuilder` class.

```Csharp
        public static IConfidentialClientApplication BuildConfidentialClientApplication(ClaimsPrincipal currentUser)
        {
            IConfidentialClientApplication clientapp = ConfidentialClientApplicationBuilder.Create(Globals.ClientId)
                  .WithClientSecret(Globals.ClientSecret)
                  .WithRedirectUri(Globals.RedirectUri)
                  .WithAuthority(new Uri(Globals.Authority))
                  .Build();

            MSALPerUserMemoryTokenCache userTokenCache = new MSALPerUserMemoryTokenCache(clientapp.UserTokenCache, currentUser ?? ClaimsPrincipal.Current);
            return clientapp;
        }
```

Important things to notice:

- The method builds an instance of the IConfidentialClientApplication using the new [builder pattern introduced by MSAL v3.X](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Client-Applications).

- `MSALPerUserMemoryTokenCache` is a sample implementation of a custom MSAL token cache, which saves tokens in a [MemoryCache](https://docs.microsoft.com/en-us/dotnet/framework/performance/caching-in-net-framework-applications) instance shared across the web app. In a real-life application, you would likely want to save tokens in a long lived store instead, so that you don't need to retrieve new ones more often than necessary.

### Using access tokens in the app, handling token expiration

The `ReadMail` action in the `HomeController` class demonstrates how to take advantage of MSAL for getting access to protected API easily and securely. It also introduces you to the recommended [token acquisition pattern](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/AcquireTokenSilentAsync-using-a-cached-token) where you should first attempt to seek an access token in the cache.

Here is the relevant code:

```CSharp
    IConfidentialClientApplication app = MsalAppBuilder.BuildConfidentialClientApplication();
    AuthenticationResult result = null;
    var accounts = await app.GetAccountsAsync();
    string[] scopes = { "Mail.Read" };

    try
    {
        // try to get token silently
        result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAsync().ConfigureAwait(false);
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

    if (result != null)
    {
        // Use the token to read email
        HttpClient hc = new HttpClient();
        hc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.AccessToken);
        HttpResponseMessage hrm = await hc.GetAsync("https://graph.microsoft.com/v1.0/me/messages");

        string rez = await hrm.Content.ReadAsStringAsync();
        ViewBag.Message = rez;
    }

    return View();
}
```

The idea is simple. The code creates a new instance of `IConfidentialClientApplication` with the exact same coordinates as the ones used when redeeming the authorization code at authentication time. In particular, note that the exact same cache is used.
That done, all you need to do is to invoke `AcquireTokenSilent`, asking for the scopes you need. MSAL will look up the cache and return any cached token, which matches with the requirement. If such access tokens are expired or no suitable access tokens are present, but there is an associated refresh token, MSAL will automatically use that to get a new access token and return it transparently.

In the case in which refresh tokens are not present or they fail to obtain a new access token, MSAL will throw `MsalUiRequiredException`. That means that in order to obtain the requested token, the user must go through an interactive sign-in experience.

In the case of this sample, the `Mail.Read` permission is obtained as part of the login process - hence we need to trigger a new login; however we can't just redirect the user without warning, as it might be disorienting (what is happening, or why, would not be obvious to the user) and there might still be things they can do with the app that do not entail accessing mail. For that reason, the sample simply signals to the view to show a warning - and to offer a link to an action (`RefreshSession`) that the user can leverage for explicitly initiating the re-authentication process.

### Handling incremental consent and OAuth2 code redemption

The `SendMail` action demonstrates how to perform operations that require incremental consent.
Observe the structure of the GET overload of that action. The code follows the same structure as the one you saw in `ReadMail`: the difference is in how `MsalUiRequiredException` is handled.
The application did not ask for `Mail.Send` during sign-in, hence the failure to obtain a token silently could have been caused by the fact that the user did not yet grant consent for the app to use this permission. Instead of triggering a new sign-in as we have done in `ReadMail`, here we can craft a specific authorization request for this permission. The call to the utility function `GenerateAuthorizationRequestUrl` does precisely that, leveraging MSAL to generate an OAuth2/OpenId Connect request for an authorization code for the Mail.Send permission.
That request, which is in fact a URL, is injected in the view as a hyperlink: once again, the user sees that link as part of a warning that the current operation requires leaving the app and going back to the authentication and consent pages.
When the user clicks that link, they are brought through the authorization flow that eventually leads to the app receiving an authorization code that can be redeemed for an access token containing the scope requested. However, the standard collection of OWIN middleware doesn't include anything that can be used for redeeming an authorization code for access and refresh tokens outside of a sign-in flow.
This sample works around that limitation by providing a simple custom middleware, **which takes care of intercepting messages containing authorization codes, validating them, redeeming the code and saving the resulting tokens in an MSAL cache, and finally redirecting to the URL that originated the request.**

Back in Startup.Auth.cs, you can see the custom middleware initialization logic right between the cookie middleware and the OpenId Connect middleware. **The position in the pipeline is important**, as in order to save the tokens in the correct cache the custom middleware needs to know who the current user is.

```CSharp
    app.UseCookieAuthentication(new CookieAuthenticationOptions());

    app.UseOAuth2CodeRedeemer(
        new OAuth2CodeRedeemerOptions
        {
            ClientId = Globals.ClientId,
            ClientSecret = Globals.ClientSecret,
            RedirectUri = Globals.RedirectUri
        }
        );

app.UseOpenIdConnectAuthentication(

```

Note that the custom middleware is provided only as an example, and it has numerous limitations (like a hard dependency on `MSALPerUserMemoryTokenCache`) that limit its applicability outside of this scenario.
