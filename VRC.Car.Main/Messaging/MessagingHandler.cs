﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using VRC.Shared.Messaging;

namespace VRC.Car.Main.Messaging
{
    public class MessagingHandler
    {
        private readonly string _hubUrl = "https://localhost:5001/messaginghub";
        private HubConnection _hubConnection;

        public int CarNumber { get; set; }

        public MessagingHandler()
        {
            Initialise();
        }

        public bool IsConnected =>
        _hubConnection.State == HubConnectionState.Connected;

        private void Initialise()
        {
            _hubConnection = new HubConnectionBuilder()
            .WithUrl(_hubUrl)
            .Build();

            _hubConnection.On<CarCommand>("ReceiveCarCommand", (command) =>
            {
                Console.WriteLine($"Car number: {command.CarNumber} Car throttle: {command.Throttle} Car direction: {command.Direction}");
            });

            _hubConnection.On<int>("AssignCarNumber", (carNumber) =>
            {
                CarNumber = carNumber;
                Console.WriteLine($"Our new car number: {carNumber}");
            });

            _hubConnection.Closed += async (error) =>
            {
                Console.WriteLine("Connection lost");
                await ConnectAsync();
            };

            _hubConnection.Reconnected += async (newConnectionId) =>
            {
                await _hubConnection.SendAsync("ReclaimCarNumber", CarNumber);
            };
        }

        public async Task ConnectAsync()
        {
            while (!IsConnected)
            {
                try
                {
                    await _hubConnection.StartAsync();
                    Console.WriteLine("Connected");
                    await RequestCarNumberAsync();
                    while (CarNumber == 0)
                    {
                        await Task.Delay(50);
                    }
                }
                catch (System.Exception)
                {
                    Console.WriteLine("Failed to connect...");
                    Console.WriteLine("Retrying");
                    await Task.Delay(2000);
                }
            }
        }

        public async Task SendCarCommandAsync(int carNumber, CarCommand command) =>
            await _hubConnection.SendAsync("SendCarCommand", carNumber, command);

        private async Task RequestCarNumberAsync() =>
            await _hubConnection.SendAsync("RequestCarNumber");
    }
}
