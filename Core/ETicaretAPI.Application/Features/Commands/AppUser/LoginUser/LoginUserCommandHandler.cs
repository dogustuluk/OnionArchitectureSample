using appuser = ETicaretAPI.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ETicaretAPI.Application.Exceptions;
using ETicaretAPI.Application.Abstractions.Token;
using ETicaretAPI.Application.DTOs;

namespace ETicaretAPI.Application.Features.Commands.AppUser.LoginUser
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommandRequest, LoginUserCommandResponse>
    {
        readonly UserManager<appuser.AppUser> _userManager;
        readonly SignInManager<appuser.AppUser> _signInManager;
        readonly ITokenHandler _tokenHandler;

        public LoginUserCommandHandler(UserManager<appuser.AppUser> userManager, SignInManager<appuser.AppUser> signInManager, ITokenHandler tokenHandler)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenHandler = tokenHandler;
        }

        public async Task<LoginUserCommandResponse> Handle(LoginUserCommandRequest request, CancellationToken cancellationToken)
        {
            appuser.AppUser user = await _userManager.FindByNameAsync(request.UsernameOrEmail);
            if (user == null)
                user = await _userManager.FindByEmailAsync(request.UsernameOrEmail);

            if (user == null)
                throw new UserNotFoundException();

            SignInResult result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (result.Succeeded)
            {
                Token token = _tokenHandler.CreateAccessToken(60);

                return new LoginUserSuccessCommandResponse() { Token = token };
            }

            //1.yol
            //return new LoginUserErrorCommandResponse()
            //{
            //    Message = "Giriş bilgileriniz hatalıdır, lütfen kontrol ediniz"
            //};
            //2.yol
            throw new AuthenticationErrorException();
        }
    }
}
