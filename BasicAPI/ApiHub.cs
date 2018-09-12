using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
namespace BasicAPI
{
    public class ApiHub : Hub
    {
        public const string GROUP = "API Clients";
        public readonly List<string> groups = new List<string>() { GROUP };
        public async Task SendMessage(string actionName,int errorCode, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", actionName, errorCode, message);
        }

        public Task SendMessageToCaller(string message)
        {
            return Clients.Caller.SendAsync("ReceiveMessage", message);
        }

        public Task SendMessageToGroups(string message)
        {
            return Clients.Groups(groups).SendAsync("ReceiveMessage", message);
        }

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GROUP);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GROUP);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
