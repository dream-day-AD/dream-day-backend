using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class ChatHub : Hub
{
    public async Task SendMessage(string weddingId, string sender, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", weddingId, sender, message);
    }
}