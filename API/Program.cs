using API.Hubs;

namespace API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddCors();

        builder.Services.AddControllers();
        builder.Services.AddControllersWithViews();
        builder.Services.AddSignalR(o =>
        {
            o.MaximumReceiveMessageSize = 9999999; //increase sending packet limit for base64 strings
            o.EnableDetailedErrors = true;
        });
        
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        app.UseCors(builder =>
        {
            builder
                .AllowAnyOrigin() // You can specify origins instead of allowing any origin
                .AllowAnyMethod() // You can specify allowed HTTP methods
                .AllowAnyHeader(); // You can specify allowed headers
        });

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseRouting(); // Ensure UseRouting() is called here

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });


        app.MapControllers();


        app.MapHub<MainHub>("/mainhub");

        app.Run();
    }
}
