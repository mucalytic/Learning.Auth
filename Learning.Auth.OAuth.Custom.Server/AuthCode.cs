namespace Learning.Auth.OAuth.Custom.Server;

public record AuthCode(
    string   ClientId,
    string   CodeChallenge,
    string   CodeChallengeMethod,
    string   RedirectUri,
    DateTime Expiry);
