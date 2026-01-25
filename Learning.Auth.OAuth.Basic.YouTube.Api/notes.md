This is an example of "incremental authorisation", where you only run the OAuth 2.0 authentication code flow
when you need to access YouTube data. Users accessing other endpoints don't also have to authorise with YouTube.

The IClaimsTransformation only adds the "youtube-access-token" claim to the local user, it isn't persisted in the cookie.

 1. Navigating to / checks the "youtube-enabled" policy.
 2. That requires authentication under the "cookie" scheme.
 3. So it redirects us to the /login endpoint which sets a cookie containing user details (a generated user_id claim).
 4. Then it redirects us back to / which again checks the "youtube-enabled" policy.
 5. That policy also requires that the user has the "youtube-token" claim.
 6. But they don't, so normally it would redirect us to the access-denied page.
 7. But we've overridden that with a custom OnRedirectToAccessDenied event.
 8. In the delegate provided to the event, we make sure that the last path we tried to access was /.
 9. And because it was, we start an OAuth 2.0 authorisation code flow to authorise with YouTube (via Google).
10. This redirects us to Google's authorisation endpoint.
11. After the user consents, Google redirects us back to our callback endpoint with an authorisation code.
12. The OAuth middleware calls Google's token endpoint to exchange the code for an access token.
13. We intercept authentication ticket creation with a custom OnCreatingTicket event handler.
14. The handler retrieves the existing local principal (from the prior cookie session),
15. Then it stores the new access token in the database (keyed by the local user_id),
16. Then it clones the principal to preserve local claims, and adds the "youtube-token" claim as a flag.
17. A new enriched cookie is issued (with preserved user_id and the new flag claim).
18. We are finally able to access the / endpoint.
19. We use the stored access token to call the YouTube API on behalf of the user and return the results.
