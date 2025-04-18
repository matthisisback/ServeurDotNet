using Microsoft.AspNetCore.SignalR;
using BlazorSignalRApp.Models;
using BlazorSignalRApp.Services;

namespace BlazorSignalRApp.Hubs
{
    public class ChatHub : Hub
    {
        private static int _userCount = 0;
        private static Dictionary<string, string> _users = new();
        private static List<(string user, string message)> _history = new();

        private readonly ChatStorageService _chatStorage;

        public ChatHub(ChatStorageService chatStorage)
        {
            _chatStorage = chatStorage;
        }

        public override async Task OnConnectedAsync()
        {
            _userCount++;
            string newUserId = $"User_{_userCount}";
            _users[Context.ConnectionId] = newUserId;

            string systemMessage = $"<----------- utilisateur {newUserId} connecté ------------->";
            await Clients.All.SendAsync("ReceiveMessage", "System", systemMessage);

            // Envoie l'historique complet uniquement au client qui vient de se connecter
            foreach (var (user, msg) in _history)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", user, msg);
            }

            // Récupère les messages enregistrés en base de données
            var databaseHistory = _chatStorage.GetAllMessages();
            foreach (var dbMessage in databaseHistory)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", dbMessage.Author, dbMessage.Content);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_users.TryGetValue(Context.ConnectionId, out var userId))
            {
                string systemMessage = $"<----------- utilisateur {userId} déconnecté ------------->";
                await Clients.All.SendAsync("ReceiveMessage", "System", systemMessage);

                _users.Remove(Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string user, string message)
        {
            // Sauvegarde dans l’historique en mémoire (max 100 messages)
            _history.Add((user, message));
            if (_history.Count > 100)
                _history.RemoveAt(0);

            // Enregistre le message dans la base de données
            _chatStorage.SaveMessage(new Message
            {
                Author = user,
                Content = message
            });

            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public List<Message> GetHistory()
        {
            // Retourne les messages enregistrés dans la base de données
            return _chatStorage.GetAllMessages();
        }
    }
}
