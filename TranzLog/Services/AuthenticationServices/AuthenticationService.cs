using AutoMapper;
using TranzLog.Data;
using TranzLog.Exceptions;
using TranzLog.Interfaces;
using TranzLog.Models;
using TranzLog.Models.DTO;

namespace TranzLog.Services.AuthenticationServices
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ShippingDbContext db;
        private readonly IMapper mapper;
        private readonly IPasswordHasher passwordHasher;
        private readonly ITokenGenerator tokenGenerator;
        public AuthenticationService(ShippingDbContext db, IMapper mapper, IPasswordHasher passwordHasher, ITokenGenerator tokenGenerator)
        {
            this.db = db;
            this.mapper = mapper;
            this.passwordHasher = passwordHasher;
            this.tokenGenerator = tokenGenerator;
        }
        public string AuthenticateAsync(LoginDTO loginDTO)
        {
            RegistrationResult result = new RegistrationResult();
            User? user = db.Users.FirstOrDefault(x => x.UserName == loginDTO.UserName);
            if (user == null)
            {
                throw new UserNotFoundException($"Пользователь {loginDTO.UserName} не найден.");
            }
            if(passwordHasher.VerifyPassword(loginDTO.Password, user.Password, user.Salt))
            {
                var token = tokenGenerator.GetToken(user);
                return token;
            }
            else
            {
                throw new InvalidPasswordException($"Неверный пароль.");
            }
        }

        public async Task<RegistrationResult> RegisterAsync(RegisterDTO registerDto)
        {
            RegistrationResult result = new RegistrationResult();
            if(db.Users.FirstOrDefault(x => x.UserName == registerDto.UserName) != null)
            {
                result.Errors.Add("Имя пользователя уже занято.");
                return result;
            }
            User user = mapper.Map<User>(registerDto);           
            user.Salt = new byte[16];
            new Random().NextBytes(user.Salt);
            user.Password = passwordHasher.HashPassword(registerDto.Password, user.Salt);
            user.CreatedDate = DateTime.UtcNow;
            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();
            result.Success = true;
            result.Message = "Пользователь успешно зарегистрирован.";
            return result;
        }
    }
}
