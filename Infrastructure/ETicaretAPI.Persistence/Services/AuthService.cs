using ETicaretAPI.Application.Abstractions.Services;
using ETicaretAPI.Application.Abstractions.Token;
using ETicaretAPI.Application.DTOs;
using ETicaretAPI.Application.DTOs.Facebook;
using ETicaretAPI.Application.Exceptions;
using ETicaretAPI.Application.Features.Commands.AppUser.LoginUser;
using ETicaretAPI.Domain.Entities.Identity;
using Google.Apis.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace ETicaretAPI.Persistence.Services
{
    public class AuthService : IAuthService
    {
        readonly HttpClient _httpClient; //DI'dan çağırıyoruz çünkü herhangi bir bağımlılık istemiyoruz.
        readonly IConfiguration _configuration;
        readonly UserManager<AppUser> _userManager;
        readonly ITokenHandler _tokenHandler;
        readonly SignInManager<AppUser> _signInManager;

        public AuthService(HttpClient httpClient, IConfiguration configuration, IHttpClientFactory httpClientFactory, UserManager<Domain.Entities.Identity.AppUser> userManager, ITokenHandler tokenHandler, SignInManager<AppUser> signInManager)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _userManager = userManager;
            _tokenHandler = tokenHandler;
            _signInManager = signInManager;
        }

        public async Task<Token> FacebookLoginAsync(string authToken, int accessTokenLifeTime)
        {
            string accessTokenResponse = await _httpClient.GetStringAsync($"https://graph.facebook.com/oauth/access_token?client_id={_configuration["FacebookLogin:ClientId"]}&client_secret={_configuration["FacebookLogin:ClientSecret"]}&grant_type=client_credentials");

            FacebookAccessTokenResponse? facebookAccessTokenResponse = JsonSerializer.Deserialize<FacebookAccessTokenResponse>(accessTokenResponse);

            string userAccessTokenValidation = await _httpClient.GetStringAsync($"https://graph.facebook.com/debug_token?input_token={authToken}&access_token={facebookAccessTokenResponse?.AccessToken}");

            FacebookUserAccessTokenValidationData? validation = JsonSerializer.Deserialize<FacebookUserAccessTokenValidationData>(userAccessTokenValidation);

            if (validation.Data.IsValid)
            {
                string userInfoResponse = await _httpClient.GetStringAsync($"https://graph.facebook.com/me?fields=email,name&access_token={authToken}");

                FacebookUserInfoResponse? userInfo = JsonSerializer.Deserialize<FacebookUserInfoResponse>(userInfoResponse);

                var info = new UserLoginInfo("FACEBOOK", validation.Data.UserId, "FACEBOOK"); //AspNetUserLogins tablosuna kaydetmek için gerekli nesne oluşturulur.

                Domain.Entities.Identity.AppUser? user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

                return await CreateUserExternalAsync(user, userInfo.Email, userInfo.Name, info, accessTokenLifeTime);
            }
            throw new Exception("invalid external authentication");
        }

        public async Task<Token> GoogleLoginAsync(string idToken, int accessTokenLifeTime)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string> { _configuration["GoogleLogin:ClientId"] }//client-id değeri verilir.
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            var info = new UserLoginInfo("GOOGLE", payload.Subject, "GOOGLE"); //AspNetUserLogins tablosuna kaydetmek için gerekli nesne oluşturulur.

            Domain.Entities.Identity.AppUser user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            return await CreateUserExternalAsync(user, payload.Email, payload.Name, info, accessTokenLifeTime);
        }

        public async Task<Token> LoginAsync(string UserNameOrEmail, string Password, int accessTokenLifeTime)
        {
            AppUser? user = await _userManager.FindByNameAsync(UserNameOrEmail);
            if (user == null)
                user = await _userManager.FindByEmailAsync(UserNameOrEmail);

            if (user == null)
                throw new UserNotFoundException();

            SignInResult result = await _signInManager.CheckPasswordSignInAsync(user, Password, false);
            if (result.Succeeded)
            {
                Token token = _tokenHandler.CreateAccessToken(accessTokenLifeTime);

                return token;
            }
            throw new AuthenticationErrorException();
        }

        private async Task<Token> CreateUserExternalAsync(AppUser user, string email, string name, UserLoginInfo info, int accessTokenLifeTime)
        {
            bool result = user != null;
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Email = email,
                        UserName = email,
                        NameSurname = name
                    };

                    var identityResult = await _userManager.CreateAsync(user); //asp net users tablosuna kayıt edildi.
                    result = identityResult.Succeeded;
                }
            }
            if (result)
            {
                await _userManager.AddLoginAsync(user, info);
                //kullanıcıyı yetkilendirme işlemleri yapılmalı ardından
                Token token = _tokenHandler.CreateAccessToken(accessTokenLifeTime);
                return token;
            }
            throw new Exception("invalid external authentication");
        }

    }
}
