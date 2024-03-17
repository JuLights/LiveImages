using System.Diagnostics;
using API.Controllers;
using API.Services;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;

public class MainHub : Hub
{
    private readonly ILogger<MainHub> _logger;
    public MainHub(ILogger<MainHub> logger)
    {
        _logger = logger;
    }

    public async Task ReceiveStream(string base64)
    {
        Debug.WriteLine($"Received: {base64}");
        await Clients.All.SendAsync("ReceiveLiveStream", base64); // Broadcast received base64 to all clients
    }
}