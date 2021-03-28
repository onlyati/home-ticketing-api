using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HomeTicketWeb.Model
{
    public static class RefreshService
    {
        public static async Task<bool> RefreshToken(IJSInProcessRuntime js, UserInfo User, string link, HttpClient Http, NavigationManager NavManager)
        {
            JSCalls call = new JSCalls(js);

            // Check that user already logon
            TokenModel refresh = new TokenModel();
            refresh.AuthToken = call.GetLocalStorage("AuthToken");
            refresh.RefreshToken = call.GetLocalStorage("RefreshToken");
            if (refresh.AuthToken == null || refresh.RefreshToken == null)
                return false;

            Console.WriteLine("Token was not null");
            var tokenJson = JsonSerializer.Serialize<TokenModel>(refresh);

            Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", refresh.AuthToken);
            var newTokenRequest = await Http.PostAsync($"{link}/user/refresh-token", new StringContent(tokenJson, Encoding.UTF8, "application/json"));
            switch (newTokenRequest.StatusCode)
            {
                case HttpStatusCode.OK:
                    // No action, token is OK
                    // Update UserInfo
                    var updUserInfo1 = await Http.GetAsync($"{link}/user/info");
                    if (updUserInfo1.StatusCode == HttpStatusCode.OK)
                    {
                        var userTemp = JsonSerializer.Deserialize<UserInfo>(await updUserInfo1.Content.ReadAsStringAsync());
                        User.UserName = userTemp.UserName;
                        User.Email = userTemp.Email;
                        User.Role = userTemp.Role;
                    }
                    return true;
                case HttpStatusCode.Created:
                    // New token was created
                    var newToken = JsonSerializer.Deserialize<TokenModel>(await newTokenRequest.Content.ReadAsStringAsync());
                    call.SetLocalStorage("AuthToken", newToken.AuthToken);
                    call.SetLocalStorage("RefreshToken", newToken.RefreshToken);
                    Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newToken.AuthToken);
                    // Update UserInfo
                    var updUserInfo2 = await Http.GetAsync($"{link}/user/info");
                    if (updUserInfo2.StatusCode == HttpStatusCode.OK)
                    {
                        var userTemp = JsonSerializer.Deserialize<UserInfo>(await updUserInfo2.Content.ReadAsStringAsync());
                        User.UserName = userTemp.UserName;
                        User.Email = userTemp.Email;
                        User.Role = userTemp.Role;
                    }
                    return true;
                case HttpStatusCode.Unauthorized:
                    // Authorization error, go the main page
                    call.RemoveLocalStorage("AuthToken");
                    call.RemoveLocalStorage("RefreshToken");
                    User.UserName = null;
                    User.Email = null;
                    User.Role = null;
                    return false;
                case HttpStatusCode.NotFound:
                    // Authorization error, go the main page
                    call.RemoveLocalStorage("AuthToken");
                    call.RemoveLocalStorage("RefreshToken");
                    User.UserName = null;
                    User.Email = null;
                    User.Role = null;
                    return false;
                default:
                    call.RemoveLocalStorage("AuthToken");
                    call.RemoveLocalStorage("RefreshToken");
                    User.UserName = null;
                    User.Email = null;
                    User.Role = null;
                    return false;
            }
        }
    }
}
