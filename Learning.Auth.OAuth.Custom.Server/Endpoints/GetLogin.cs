using System.Web;

namespace Learning.Auth.OAuth.Custom.Server.Endpoints;

public static class GetLogin
{
    public static async Task Handler(string returnUrl, HttpResponse response)
    {
        var encodedReturnUrl = HttpUtility.UrlEncode(returnUrl);
        response.Headers.ContentType = new[] { "text/html" };
        await response.WriteAsync(
            $"""
            <html>
              <head>
                <title>Login</title>
              </head>
              <body>
                <form action="/login?returnUrl={encodedReturnUrl}" method="post">
                  <input type="submit" value = "Submit" />
                </form>
              </body>
            </html>    
            """);
    }
}
