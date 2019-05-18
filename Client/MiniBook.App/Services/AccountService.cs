﻿using MiniBook.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniBook.Services
{
    public class AccountService
    {
        HttpService HttpService { get; }

        public AccountService(HttpService httpService)
        {
            HttpService = httpService;
        }
        public async Task<bool> LoginAsync(string email, string password)
        {
            var url = Configuration.ID_HOST + "/connect/token";

            var response = await HttpService.PostAsync<TokenResponse>(url, new Dictionary<string, string>()
            {
                {"client_id", "client"},
                {"client_secret", "secret"},
                {"grant_type", "password"},
                {"username", email},
                {"password", password}
            });

            if (!string.IsNullOrEmpty(response.Error))
            {
                switch (response.Error)
                {
                    case "invalid_grant":
                        return false;
                    default:
                        return false;
                }
            }

            if (!string.IsNullOrEmpty(response.AccessToken))
            {
                response.ExpiresAt = DateTime.UtcNow.AddSeconds(response.ExpiresIn);

                await Xamarin.Essentials.SecureStorage.SetAsync("Token",
                    JsonConvert.SerializeObject(response, Formatting.None));

                AppContext.Current.Token = response;

                return true;
            }

            return false;
        }

        public async Task<bool> RestoreAsync()
        {
            var token = await Xamarin.Essentials.SecureStorage.GetAsync("Token");

            if (token != null)
            {
                AppContext.Current.Token = JsonConvert.DeserializeObject<TokenResponse>(token);

                if (AppContext.Current.Token.IsExpired())
                {
                    //...

                    AppContext.Current.Token = null;

                    return false;
                }

                return true;
            }

            return false;
        }
        public Task<ApiResponse<object>> RegisterAsync(User user, string password)
        {
            var url = Configuration.ID_HOST + "/api/account";

            return HttpService.PostApiAsync<object>(url, new
            {
                user.Firstname,
                user.Lastname,
                user.Gender,
                user.Email,
                user.BirthDate,
                Password = password
            });
        }
    }
}
