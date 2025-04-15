using BlazorSignalRApp.Components;
using BlazorSignalRApp.Hubs;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSignalR();

// Compression des réponses (utile pour SignalR)
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});

// Ajout des contrôleurs (API)
builder.Services.AddControllers();

var app = builder.Build();

app.UseResponseCompression();

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Ajout de la route vers les composants Razor (Blazor)
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorSignalRApp.Client._Imports).Assembly);

// 💥 C’EST CETTE LIGNE QUI TE MANQUAIT 💥
app.MapControllers();
app.MapHub<ChatHub>("/chathub");

app.Run();
