using ETicaretAPI.Application.Abstractions.Services;
using ETicaretAPI.Application.Abstractions.Services.Authentications;
using ETicaretAPI.Application.Abstractions.Token;
using ETicaretAPI.Application.DTOs;
using ETicaretAPI.Application.DTOs.Facebook;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace ETicaretAPI.Application.Features.Commands.AppUser.FacebookLogin
{
    public class FacebookLoginCommandHandler : IRequestHandler<FacebookLoginCommandRequest, FacebookLoginCommandResponse>
    {
        readonly IExternalAuthentication _externalAuthentication;

        public FacebookLoginCommandHandler(IExternalAuthentication externalAuthentication)
        {
            _externalAuthentication = externalAuthentication;
        }

        public async Task<FacebookLoginCommandResponse> Handle(FacebookLoginCommandRequest request, CancellationToken cancellationToken)
        {
            var token = await _externalAuthentication.FacebookLoginAsync(request.AuthToken, 60);
            return new()
            {
                Token = token
            };
        }
    }
}
