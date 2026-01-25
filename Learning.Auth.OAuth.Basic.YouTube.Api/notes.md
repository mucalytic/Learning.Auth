 1. navigating to / checks the "youtube-enabled" policy.
 2. that requires authentication under the "cookie" scheme,
 3. so it redirects us to the /login endpoint which sets a cookie containing user details,
 4. then it redirects us back to / which again checks the "youtube-enabled" policy.
 5. that policy also requires that the user has the "youtube-token" claim,
 6. but they don't, so normally it would redirect us to the access-denied page.
 7. but we've overridden that with a custom OnRedirectToAccessDenied event.
 8. in the delegate provided to the event, we make sure that the last path we tried to access was /
 9. and because it was, we start an oauth 2.0 authorisation code flow to authorise with youtube.
10. this redirects us to google's authorisation endpoint.
11. after the user consents, google redirects us back to our callback endpoint with an authorisation code.
12. then we call google's token endpoint to exchange the code for an access token.
13. we intercept authentication ticket creation with a custom OnCreatingTicket event handler.
14. the handler saves the access token to the database,
15. then we call google's userinfo endpoint to get the user's claims using the access token.
16. we merge those claims with the local user's claims and set the "youtube-token" claim.
17. then we are finally able to access the / endpoint and do whatever we want there.
