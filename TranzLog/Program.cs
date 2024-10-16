
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TranzLog.Data;
using TranzLog.Interfaces;
using TranzLog.Models.DTO;
using TranzLog.Repositories;
using TranzLog.Services.AuthenticationServices;

namespace TranzLog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
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
            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();            
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(option =>
            {
                option.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ValidAudience = builder.Configuration["JWT:Aidience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
                };
            });
            builder.Services.AddSwaggerGen(opt =>
            {
                opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "pls enter token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "Token",
                    Scheme = "bearer"
                });
                opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id="Bearer",
                        }
                    },
                    new string[]{ }
                    }
                });
            });
            var app = builder.Build();           
            using (var scope = app.Services.CreateScope()) //Создание администратора
            {
                var services = scope.ServiceProvider;                
                try
                {
                    var context = services.GetRequiredService<ShippingDbContext>();
                    var passwordHasher = services.GetRequiredService<IPasswordHasher>();
                    DbInitializer.Initialize(context, passwordHasher);
                }
                catch (Exception ex)
                {
                    app.Logger.LogError($"Ошибка инициализации базы данных: {ex.Message}");
                    Environment.Exit(1);
                }
            }   
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
