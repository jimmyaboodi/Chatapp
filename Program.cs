using ChatApp;

var builder = WebApplication.CreateBuilder(args);

// Registrerar SignalR-tjänsterna (ingår i ASP.NET Core, inget extra paket behövs)
builder.Services.AddSignalR();

var app = builder.Build();

// Skickar all HTTP-trafik vidare till HTTPS (TLS)
app.UseHttpsRedirection();

// Serverar klienten (wwwroot/index.html)
app.UseDefaultFiles();
app.UseStaticFiles();

// Hubben är serverns ände av anslutningen. Klienter ansluter till /chat
app.MapHub<ChatHub>("/chat");

app.Run();
