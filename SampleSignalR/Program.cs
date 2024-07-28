
using Microsoft.AspNetCore.SignalR;
using SampleSignalR.Chat;
using System.Reflection;
using System.Text;

namespace SampleSignalR
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            //add cors
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowAnyOrigin()
                           .SetIsOriginAllowed((host) => true);
                });
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //add signalR
            builder.Services.AddCors();
            builder.Services.AddSignalR();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });


            app.MapHub<ChatHub>("/chathub");

            app.MapPost("/sendToAll", async (ChatMessage message, IHubContext<ChatHub> hubContext) =>
            {
                await hubContext.Clients.All.SendAsync("ReceiveMessage", message.User, message.Message);
                return Results.Ok();
            })
            .WithName("sendToAll")
            .WithOpenApi();

            app.MapPost("/sendToClient", async (ClientMessage message, IHubContext<ChatHub> hubContext) =>
            {
                await hubContext.Clients.Client(message.ConnectionId).SendAsync("ReceiveMessage", message.Message);
                return Results.Ok();
            })
            .WithName("SendToClient")
            .WithOpenApi();

            string htmlContent = File.ReadAllText(AppContext.BaseDirectory + "\\ClientSignalR\\index.html");
            app.MapGet("/ClientSignalR", () => Results.Content(htmlContent, "text/html"));

            app.Run();
        }
    }
}
