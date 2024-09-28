
using Microsoft.EntityFrameworkCore;
using TranzLog.Data;
using TranzLog.Models.DTO;

namespace TranzLog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            string? connection = builder.Configuration.GetConnectionString("db");
            string? versingString = builder.Configuration.GetConnectionString("Version");
            if (connection != null && versingString != null)
            {
                var version = new MySqlServerVersion(new Version(versingString));
                builder.Services.AddDbContext<ShippingDbContext>(dbContextOptions =>
                {
                    dbContextOptions.UseLazyLoadingProxies();
                    dbContextOptions.UseMySql(connection, version);
                });
            }
            else
            {
                Console.WriteLine("Ошибка в строке подключения к БД.");
                Environment.Exit(1);
            }
            builder.Services.AddAutoMapper(typeof(MappingProfile));
            builder.Services.AddMemoryCache();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
