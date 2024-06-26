using System.Buffers.Text;
using System.Diagnostics;
using API.Controllers;
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
        await Clients.All.SendAsync("ReceiveLiveStream", base64); // base64 to all clients
    }

    public async Task ReceiveAudioStream(string audioData)
    {
        Debug.WriteLine($"Received: {audioData}");
        await Clients.All.SendAsync("ReceiveAudioStream", audioData);
    }
}