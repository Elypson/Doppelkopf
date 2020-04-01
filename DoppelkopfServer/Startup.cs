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
using DoppelkopfServer.Controllers;

namespace DoppelkopfServer
{
    public class Startup
    {
        ~Startup()
        {
            Debug.WriteLine("Done");
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IMainController, MainController>();
        }

        private List<WebSocket> webSockets = new List<WebSocket>();

        public async Task MyWebSocketHandler(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                foreach (var receiver in webSockets)
                {
                    if (receiver.State == WebSocketState.Open || receiver.State == WebSocketState.CloseReceived)
                    {
                        await receiver.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    }
                }

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
