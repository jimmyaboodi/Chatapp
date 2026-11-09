using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp;

/// <summary>
/// Hubben tar emot anrop från klienterna och skickar meddelanden vidare till alla.
/// En ny instans av Hub skapas för varje anrop, därför ligger användarlistan som static.
/// </summary>
public class ChatHub : Hub
{
    // ConnectionId -> användarnamn. ConcurrentDictionary är trådsäker (flera användare samtidigt).
    private static readonly ConcurrentDictionary<string, string> Users = new();

    private const int MaxNameLength = 20;
    private const int MaxMessageLength = 500;

    /// <summary>
    /// Klienten anropar Join med önskat användarnamn.
    /// Returnerar null om det gick bra, annars ett felmeddelande.
    /// </summary>
    public async Task<string?> Join(string username)
    {
        username = (username ?? "").Trim();

        if (username.Length == 0 || username.Length > MaxNameLength)
            return $"Användarnamnet måste vara 1-{MaxNameLength} tecken.";

        if (Users.ContainsKey(Context.ConnectionId))
            return "Du har redan anslutit.";

        // Användarnamn måste vara unika (skiftlägesokänsligt)
        if (Users.Values.Any(u => u.Equals(username, StringComparison.OrdinalIgnoreCase)))
            return "Användarnamnet är upptaget.";

        Users[Context.ConnectionId] = username;

        await Clients.All.SendAsync("SystemMessage", $"{username} anslöt till chatten.");
        await Clients.All.SendAsync("UserList", Users.Values.OrderBy(u => u).ToList());
        return null;
    }

    /// <summary>
    /// Tar emot ett meddelande och skickar det till ALLA anslutna klienter i realtid.
    /// </summary>
    public async Task SendMessage(string message)
    {
        // Servern litar aldrig på klienten: kontrollera att användaren har gått med
        if (!Users.TryGetValue(Context.ConnectionId, out var username))
            throw new HubException("Du måste välja ett användarnamn först.");

        message = (message ?? "").Trim();
        if (message.Length == 0) return;
        if (message.Length > MaxMessageLength)
            throw new HubException($"Meddelandet får vara max {MaxMessageLength} tecken.");

        // Användarnamnet hämtas från servern, inte från klienten, så ingen kan låtsas vara någon annan
        await Clients.All.SendAsync("ReceiveMessage", username, message, DateTime.Now.ToString("HH:mm"));
    }

    /// <summary>
    /// Körs automatiskt när en klient kopplar ner (stänger fliken, tappar nätet m.m.).
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Users.TryRemove(Context.ConnectionId, out var username))
        {
            await Clients.All.SendAsync("SystemMessage", $"{username} lämnade chatten.");
            await Clients.All.SendAsync("UserList", Users.Values.OrderBy(u => u).ToList());
        }

        await base.OnDisconnectedAsync(exception);
    }
}
