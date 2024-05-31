using ETicaretAPI.Application.Abstractions.Services;
using ETicaretAPI.Application.DTOs.User;
using ETicaretAPI.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using appuser = ETicaretAPI.Domain.Entities.Identity;

namespace ETicaretAPI.Persistence.Services
{
    public class UserService : IUserService
    {
        readonly UserManager<appuser.AppUser> _userManager;

        public UserService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<CreateUserResponse> CreateAsync(CreateUser model)
        {
            IdentityResult result = await _userManager.CreateAsync(new()
            {
                Id = Guid.NewGuid().ToString(),
                NameSurname = model.NameSurname,
                UserName = model.UserName,
                Email = model.Email,
            }, model.Password);


            CreateUserResponse response = new() { Succeeded = result.Succeeded };
            if (result.Succeeded)
                response.Message = "Kullanıcı Kaydı Başarılı";
            else
                foreach (var error in result.Errors)
                    response.Message += $"{error.Code}-{error.Description}\n";

            return response;
        }
    }
}
