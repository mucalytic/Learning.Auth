using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/create-rsa-private-key", () =>
    Convert.ToBase64String(RSA.Create().ExportRSAPrivateKey()));

app.Run();
