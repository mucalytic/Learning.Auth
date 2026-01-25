1. navigating to / checks the "youtube-enabled" policy.
2. that requires authentication under the "cookie" scheme,
3. so it redirects us to the /login endpoint which sets a cookie containing user details,
4. then it redirects us back to / which again checks the "youtube-enabled" policy.
5. that policy also requires that the user has the "youtube-token" claim,
6. but they don't, so it redirects us to the access-denied page.
7. we catch it at the OnRedirectToAccessDenied event and use oauth with authorise with youtube,
8. then we can grab claims, merge them with the  user's claims and set the "youtube-token" claim.
9. then we can access the / endpoint and do whatever we want there.
