namespace Learning.Auth.Identity.Cookies.Api;

public record Username(string Value)
{
    public static Username Empty = new Username(string.Empty);
}
