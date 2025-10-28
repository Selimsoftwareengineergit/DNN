using Microsoft.AspNetCore.SignalR;

namespace DNN.Services.Hubs
{
    public class StudentRequestHub : Hub
    {
        public async Task SendRequestUpdate(string message)
        {
            await Clients.All.SendAsync("ReceiveRequestUpdate", message);
        }
    }
}