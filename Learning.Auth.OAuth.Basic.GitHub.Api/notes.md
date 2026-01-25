1. The user visits /login.
2. The app redirects the user to GitHub's authorisation endpoint (https://github.com/login/oauth/authorize).
3. The user logs into GitHub (if not already) and grants permission to the app.
4. GitHub redirects back to the app's callback path (/oauth/callback) with an authorisation code.
5. The OAuth middleware exchanges that code for an access token by POSTing to GitHub's token endpoint (https://github.com/login/oauth/access_token).
6. The middleware then (via the custom OnCreatingTicket event) uses the access token to call GitHub's user information endpoint (https://api.github.com/user).
7. It parses the JSON response, maps selected fields to standard claims (e.g. GitHub id → sub, GitHub login → name).
8. The middleware signs the user in using the cookie authentication scheme.
9. The user is redirected to the root (/), where the app can read context.User.Claims and display them (or use them for authorisation).
