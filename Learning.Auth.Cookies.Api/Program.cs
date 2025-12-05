var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication()
                .AddCookie("default");
builder.Services.AddControllers();

var app = builder.Build();

app.UseStaticFiles();
app.UseAuthentication();
app.UseHttpsRedirection();
app.MapGet("/", () => "hello");
app.MapDefaultControllerRoute();
app.Run();