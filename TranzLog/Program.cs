
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TranzLog.Data;
using TranzLog.Interfaces;
using TranzLog.Models.DTO;
using TranzLog.Repositories;
using TranzLog.Services;
using TranzLog.Services.AuthenticationServices;

namespace TranzLog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.AddConsole();
            var logger = LoggerFactory.Create(logging => logging.AddConsole())
                          .CreateLogger("StartupLogger");
            logger.LogInformation("Начинаем настройку сервисов...");
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
                logger.LogError("Ошибка в строке подключения к БД.");
            }
            builder.Services.AddAutoMapper(typeof(MappingProfile));
            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<IRepository<ShipperDTO>, ShipperRepository>();
            builder.Services.AddScoped<IRepository<ConsigneeDTO>, ConsigneeRepository>();
            builder.Services.AddScoped<IDriverRepository, DriverRepository>();
            builder.Services.AddScoped<IRepository<CargoDTO>, CargoRepository>();
            builder.Services.AddScoped<IRouteRepository, RouteRepository>();
            builder.Services.AddScoped<IRepository<VehicleDTO>, VehicleRepository>();
            builder.Services.AddScoped<ITransportOrderRepository, TransportOrderRepository>();
            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IManagerOrderService, ManagerOrderService>();
            builder.Services.AddScoped<IDriverOrderService, DriverOrderService>();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            var key = builder.Configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(key))
            {
                logger.LogError("В конфигурации отсутствует ключ JWT.");
            }
            var issuer = builder.Configuration["Jwt:Issuer"];
            var audience = builder.Configuration["Jwt:Audience"];
            if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
            {
                logger.LogError("В конфигурации JWT отсутствует Issuer или Audience.");
            }
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(option =>
            {
                option.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!))
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
            logger.LogInformation("Все сервисы добавлены.");
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
