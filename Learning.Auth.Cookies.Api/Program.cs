// https://youtu.be/hw2B6SZj8y8?si=tmi8-mft3bYkSgvQ&t=476

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication()
                .AddCookie("default", options => options.Cookie.Name = "aaron.cookie");
builder.Services.AddControllers(); // allows you to pick up controllers from the file directory

var app = builder.Build();

app.UseStaticFiles();
app.UseAuthentication();
app.UseHttpsRedirection();
app.MapGet("/", () => "hello");
app.MapDefaultControllerRoute();
app.Run();