var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/username", () =>
{
    return "aaron";
});

app.Run();
