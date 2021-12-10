//MSAL instance
const msalInstance = new msal.PublicClientApplication({
    auth: {
        clientId: "639982ad-d26c-40a5-852a-80817e3fdae6",
        redirectUri: "https://localhost:44320",
        authority: "https://login.microsoftonline.com/organizations/v2.0"
    }
})

// Add here the endpoints for MS Graph API services you would like to use.
const graphConfig = {
    graphMeEndpoint: "https://graph.microsoft.com/v1.0/me",
    graphMailEndpoint: "https://graph.microsoft.com/v1.0/me/messages"
};

// Select DOM elements to work with
const welcomeDiv = document.getElementById("welcomeMessage");
const signInButton = document.getElementById("signIn");
const signOutButton = document.getElementById('signOut');
const cardDiv = document.getElementById("card-div");
const mailButton = document.getElementById("readMail");
const profileButton = document.getElementById("seeProfile");
const profileDiv = document.getElementById("profile-div");
