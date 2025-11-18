using System.Security.Cryptography;

var rsaKey = RSA.Create();
var privateKey = rsaKey.ExportRSAPrivateKey();
File.WriteAllBytes("key", privateKey);

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", (HttpContext ctx) => "hello world");

app.Run();
