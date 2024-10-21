using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TranzLog.Data;
using TranzLog.Exceptions;
using TranzLog.Interfaces;
using TranzLog.Models;
using TranzLog.Models.DTO;

namespace TranzLog.Services.AuthenticationServices
{
    public class AuthenticationService : IAuthenticationService
    {
        IUserRepository repo;
        private readonly IMapper mapper;
        private readonly IPasswordHasher passwordHasher;
        private readonly ITokenGenerator tokenGenerator;
        public AuthenticationService(IUserRepository repo, IMapper mapper, IPasswordHasher passwordHasher, ITokenGenerator tokenGenerator)
        {
            this.repo = repo;
            this.mapper = mapper;
            this.passwordHasher = passwordHasher;
            this.tokenGenerator = tokenGenerator;
        }
        public async Task<string> AuthenticateAsync(LoginDTO loginDTO)
        {
            User? user = await repo.GetUserEntityByNameAsync(loginDTO.UserName);
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
            if(await repo.UserExistsAsync(registerDto.UserName))
            {
                result.Message = "Имя пользователя уже занято.";
                return result;
            }
            User user = mapper.Map<User>(registerDto);           
            user.Salt = new byte[16];
            new Random().NextBytes(user.Salt);
            user.Password = passwordHasher.HashPassword(registerDto.Password, user.Salt);
            user.CreatedDate = DateTime.UtcNow;
            await repo.AddUserAsync(user);
            result.Success = true;
            result.Message = "Пользователь успешно зарегистрирован.";
            return result;
        }
        public async Task ChangeUserRole(string userName, string targetRole, string roleCurrentUser)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(targetRole) || string.IsNullOrEmpty(roleCurrentUser))
            {
                throw new ArgumentException("Параметры не могут быть пустыми.");
            }
            User? targetUser = await repo.GetUserEntityByNameAsync(userName);
            if (targetUser == null)
            {
                throw new UserNotFoundException($"Пользователь {userName} не найден.");
            }
            if (roleCurrentUser == Role.Administrator.ToString())
            {
                Role role;
                if (Enum.TryParse(targetRole, out role))
                {
                    targetUser.Role = role;
                }
                else
                {
                    throw new InvalidRoleException($"Недопустимавя роль {targetRole}");
                }
            }
            else if (roleCurrentUser == Role.Manager.ToString())
            {
                if (targetUser.Role == Role.User && targetRole == Role.Driver.ToString())
                {
                    targetUser.Role = Role.Driver; ;
                }
                else if (targetUser.Role == Role.Driver && targetRole == Role.User.ToString())
                {
                    targetUser.Role = Role.User;
                }
                else if (targetRole == Role.Administrator.ToString() || targetRole == Role.Manager.ToString())
                {
                    throw new UnauthorizedAccessException($"Недостаточно прав для данной операции.");
                }
                else
                {
                    throw new InvalidRoleException($"Неверно указаны роли.");
                }
            }
            else
            {
                throw new UnauthorizedAccessException($"Недостаточно прав для данной операции.");
            }
            await repo.UpdateUserAsync(targetUser);
        }
    }
}
