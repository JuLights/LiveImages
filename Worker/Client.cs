using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Worker;

public class Client
{
    bool isConnected = false;
    HubConnection connection;
    public Client()
    {
        connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5049/mainhub")
                .Build();
                
        connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0,5) * 1000);
                await connection.StartAsync();
            };
    }

    public async Task ConnectAsync(string base64)
    {
        //if(!isConnected){
        //    await connection.StartAsync();
        //    isConnected= true;
        //}
        await connection.InvokeAsync("ReceiveStream", base64);
    }

    public async Task AudioTransferAsync(string data)
    {
        if (!isConnected)
        {
            await connection.StartAsync();
            isConnected = true;
        }

        //var buff = Convert.FromBase64String(data);

        //foreach (var item in buff)
        //{
        //    Console.Write(item);
        //}

        await connection.InvokeAsync("ReceiveAudioStream", data);
    }
}