using HomeTicketWeb.Model;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HomeTicketWeb
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            // Save data on different pages, thus they will be there during naviagtion too
            builder.Services.AddSingleton<UserInfo>();
            builder.Services.AddSingleton<CreateTicket>();
            builder.Services.AddSingleton<AdminStatus>();
            builder.Services.AddSingleton<UserStatus>();

            // It is webassembly, so JS can run in non async mode, thus IJSProcessRuntime is used instaed of JSRuntime
            builder.Services.AddSingleton<IJSInProcessRuntime>(services => (IJSInProcessRuntime)services.GetRequiredService<IJSRuntime>());

            await builder.Build().RunAsync();
        }
    }
}
