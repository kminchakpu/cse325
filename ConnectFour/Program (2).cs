
using ConnectFour;
using ConnectFour.Components;
using ConnectFour.Hubs;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Enable detailed diagnostic logging.
builder.Logging.AddFilter(
    "Microsoft.AspNetCore.Components.Server.Circuits",
    LogLevel.Debug);

builder.Logging.AddFilter(
    "Microsoft.AspNetCore.SignalR",
    LogLevel.Debug);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = true;
    });

builder.Services.AddSingleton<GameState>();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<MultiplayerHub>("/multiplayerhub");

app.Run();
