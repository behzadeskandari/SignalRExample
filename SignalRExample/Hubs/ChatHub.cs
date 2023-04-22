using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SignalRExample.Models;

namespace SignalRExample.Hubs
{
    public class ChatHub : Hub
    {
        private static List<ClientConnected> _clientConnecteds;
        private readonly UserManager<IdentityUser> _userManager;


        public ChatHub(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
            if (_clientConnecteds == null)
            {
                _clientConnecteds = new List<ClientConnected>();
            }
        }
        public override async Task OnConnectedAsync()
        {
        
            
            var connectionId = Context.ConnectionId;
            var email = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(email))
                return;

            foreach (var clientConnected in _clientConnecteds.Where(a => a.Email != email).ToList())
            {
                await Clients.Caller.SendAsync("OnUserConnect",clientConnected);
            }


            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == email);

            var client = new ClientConnected()
            {
                Id = user.Id,
                Email = user.Email,
                ConnectionId = connectionId,
            };
            _clientConnecteds.Add(client);

            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{client.Id}");


            await Clients.AllExcept(_clientConnecteds.Where(a => a.Email == email).Select(a => a.ConnectionId)).SendAsync("OnlineUsers", client);
            

            //Clients.Caller.SendAsync("Log", "Trigger from Caller");

            //Clients.Client(connectionId).SendAsync("Log", "Trigger from Clients.Client");
            //Groups.AddToGroupAsync(connectionId, $"user-{email}");

            //Clients.Group("user-123").SendAsync("Log", "Trigger from Gruop");

            await base.OnConnectedAsync();
        
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var client = _clientConnecteds.FirstOrDefault(a => a.ConnectionId == Context.ConnectionId);
            if (client != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{client.Id}");
                await Clients.AllExcept(Context.ConnectionId).SendAsync("OnUserDisconnected", client);
                _clientConnecteds.RemoveAll(a => a.ConnectionId == Context.ConnectionId);

            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string toUser,string message)
        {
            var client = _clientConnecteds.First(a => a.ConnectionId == Context.ConnectionId);
            var fromName = client.Email;
            await Clients.Group($"user-{toUser}").SendAsync("OnReceiveMessage", client.Id, fromName, message);
        }
    }
}
