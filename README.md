# hybridSPA

## Problem Description

Some applications like SharePoint and OWA are built as "hybrid" web applications, which are built with server-side and client-side components (e.g. an ASP.net  web application hosting a React single-page application). In these scenarios, the application will likely need authentication both client-side (e.g. a public client using MSAL.js) and server-side (e.g. a confidential client using MSAL.net ), and each application context will need to acquire its own tokens. This requires that the application complete two round-trips to the eSTS /authorize endpoint, one to get tokens for the confidential client, and a second to get tokens for the public client. Historically, the second leg of this flow is done silently via the implicit flow (e.g. ADAL.js or MSAL.js v1). However, when third-party cookies are blocked in the user's browser due to the browser's privacy features (e.g. Safari ITP), MSAL.js is unable to complete the second round-trip to the /authorize endpoint silently, resulting in the application needing to prompt the user to interactively navigate to the /authorize endpoint (via popup or full-page redirect) to complete authentication for the public client. And while MSAL.js v2 does not require third-party cookies for silent token renewal, it still relies on third-party cookies to silently authenticate the user when the application first loads.

## Hybrid SPA sample application

This sample demonstrates how to use MSAL.js v2 and MSAL .Net together in a "hybrid" application that performs both server-side and client-side authenication. 

It shows how to use two new APIs, `WithSpaAuthorizationCode` on `AcquireTokenByAuthorizationCode` in MSAL .Net and `acquireTokenByCode` in MSAL.js v2, to authenticate a user server-side using a confidential client, and then SSO that user client-side using a second authorization code that is returned to the confidential client and redeemed by the public client client-side. This helps mitigate user experience and performance concerns that arise when performing server-side and client-side authentication for the same user, especially when third-party cookies are blocked by the browser.

## Sample Overview

The application is implemented as an ASP.NET MVC project, while the web sign-on functionality is implemented via ASP.NET OpenId Connect OWIN middleware.

The sample also shows how to use MSAL.js V2 (Microsoft Authentication Library for JavaScript) to obtain an access token for Microsoft Graph. Specifically, the sample shows how to retrieve the last email messages received by the signed in user, and how to send a mail message as the user using Microsoft Graph.

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

