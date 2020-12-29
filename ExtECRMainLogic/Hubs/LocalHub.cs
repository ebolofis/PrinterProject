using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace ExtECRMainLogic.Hubs
{
    public class LocalHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }

    public class LocalHubInvoker
    {
        private readonly IHubContext<LocalHub> hubContext;

        public LocalHubInvoker(IHubContext<LocalHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        public void UpdateGlobalHubConnectionStatus(string user, HubConnectionState state)
        {
            switch (state)
            {
                case HubConnectionState.Disconnected:
                    hubContext.Clients.All.SendAsync("ConnectionStatus", "Disconnected", user);
                    break;
                case HubConnectionState.Connected:
                    hubContext.Clients.All.SendAsync("ConnectionStatus", "Connected", user);
                    break;
                case HubConnectionState.Connecting:
                    hubContext.Clients.All.SendAsync("ConnectionStatus", "Connecting", user);
                    break;
                case HubConnectionState.Reconnecting:
                    hubContext.Clients.All.SendAsync("ConnectionStatus", "Reconnecting", user);
                    break;
                default:
                    break;
            }
        }

        public void SendError(string errorMessage)
        {
            hubContext.Clients.All.SendAsync("SendError", errorMessage);
        }
    }
}