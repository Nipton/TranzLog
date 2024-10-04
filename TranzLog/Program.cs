
using Microsoft.EntityFrameworkCore;
using TranzLog.Data;
using TranzLog.Interfaces;
using TranzLog.Models.DTO;
using TranzLog.Repositories;

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
            builder.Services.AddScoped<IRepository<ShipperDTO>, ShipperRepository>();
            builder.Services.AddScoped<IRepository<ConsigneeDTO>, ConsigneeRepository>();
            builder.Services.AddScoped<IRepository<DriverDTO>, DriverRepository>();
            builder.Services.AddScoped<IRepository<CargoDTO>, CargoRepository>();
            builder.Services.AddScoped<IRepository<RouteDTO>, RouteRepository>();
            builder.Services.AddScoped<IRepository<VehicleDTO>, VehicleRepository>();
            builder.Services.AddScoped<IRepository<TransportOrderDTO>, TransportOrderRepository>();
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
