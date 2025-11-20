using Microsoft.AspNetCore.SignalR;

namespace P_F.Hubs
{
    public class NotificacionHub : Hub
    {
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task SendNotificationToAll(string message, string type)
        {
            await Clients.All.SendAsync("ReceiveNotification", message, type);
        }

        public async Task SendNotificationToGroup(string groupName, string message, string type)
        {
            await Clients.Group(groupName).SendAsync("ReceiveNotification", message, type);
        }

        public async Task SendNotificationToUser(string userId, string message, string type)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message, type);
        }
    }
}