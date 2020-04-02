using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading;
using System.IO;
using Doppelkopf.Controllers;
using Doppelkopf.Interfaces;
using Doppelkopf.Services;

namespace Doppelkopf
{
    public class Startup
    {
        // configure services
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ISendService, SendService>();
            services.AddSingleton<IMetaMessageService, MetaMessageService>();
            services.AddSingleton<IChatMessageService, ChatMessageService>();
            services.AddSingleton<IGameMessageService, GameMessageService>();
            services.AddSingleton<IMainController, MainController>();
        }

        // handle incoming websocket requests and http-client requests
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseWebSockets();

            app.Use(async (context, next) =>
            {
                if(context.Request.Path == "/server")
                {
                    if(context.WebSockets.IsWebSocketRequest)
                    {
                        var mainController = app.ApplicationServices.GetRequiredService<IMainController>();
                        await mainController.ManageWebSocketRequest(context);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }
            });

            app.Use(async (context, next) =>
            {
                if(context.Request.Path == "/")
                {
                    var contents = File.ReadAllText("Client/Client.html");
                    await context.Response.WriteAsync(contents);
                }
                else
                {
                    await next();
                }
            });
        }
    }
}
