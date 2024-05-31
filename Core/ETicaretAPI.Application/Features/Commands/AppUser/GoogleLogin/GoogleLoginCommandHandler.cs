﻿using ETicaretAPI.Application.Abstractions.Services.Authentications;
using MediatR;

namespace ETicaretAPI.Application.Features.Commands.AppUser.GoogleLogin
{
    public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommandRequest, GoogleLoginCommandResponse>
    {
        readonly IExternalAuthentication _externalAuthentication;

        public GoogleLoginCommandHandler(IExternalAuthentication externalAuthentication)
        {
            _externalAuthentication = externalAuthentication;
        }

        public async Task<GoogleLoginCommandResponse> Handle(GoogleLoginCommandRequest request, CancellationToken cancellationToken)
        {
            var token = await _externalAuthentication.GoogleLoginAsync(request.IdToken, 60);
            return new()
            {
                Token = token,
            };
        }
    }
}
