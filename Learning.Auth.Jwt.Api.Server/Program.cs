using System.Security.Cryptography;

var rsaKey = RSA.Create();
var privateKey = File.ReadAllBytes("key");
rsaKey.ImportRSAPrivateKey(privateKey, out _);


var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", (HttpContext ctx) => "hello world");

app.Run();
