using Microsoft.OpenApi.Models;
using SagraPOS.Models;
internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        // TODO field ID gets serialized as "id" and I don't like it (consider switching to newtosoft json)
        //.AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);
        builder.Services.AddSqlite<MenuDB>("Data Source=SagraPOS.sqlite3");
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SagraPOS API",
                Description = "Simple POS for sagra-type events",
                Version = "v1"
            });
        });
        var app = builder.Build();
        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            // app.UseHsts();
        }
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "SagraPOS aPI API V1");
        });
        // app.UseHttpsRedirection();
        app.UseStaticFiles();
        // app.UseRouting();
        // app.MapControllerRoute(
        //     name: "default",
        //     pattern: "{controller}/{action=Index}/{id?}");
        app.MapFallbackToFile("index.html");
        app.MapControllers();
        app.Run();
    }
}