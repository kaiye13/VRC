﻿using System;
using System.Collections.Generic;
using System.Linq;
using InTheHand.Net.Sockets;
using System.Net.Sockets;
using System.Threading.Tasks;
using InTheHand.Net.Bluetooth;

namespace VRC.Shared
{
    public class ConnectionDevice
    {
        private BluetoothClient bluetooth;
        private NetworkStream bluetoothStream;

        public string DeviceName { get; set; }
        public string DeviceConnectionAdress { get; set; }

        public bool ConnectDevice(string DeviceName)
        {
            var gadgeteerDevice = bluetooth.DiscoverDevices().Where(d => d.DeviceName == DeviceName).FirstOrDefault();
            if (gadgeteerDevice != null)
            {
                bluetooth.SetPin("1234");
                bluetooth.Connect(gadgeteerDevice.DeviceAddress, BluetoothService.SerialPort);
                bluetoothStream = bluetooth.GetStream();
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool SendMessage(string Message)
        {
            if (bluetooth.Connected && bluetoothStream != null)
            {
                var buffer = System.Text.Encoding.UTF8.GetBytes(Message);
                bluetoothStream.Write(buffer, 0, buffer.Length);
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task<string> ReceiveMessageAsync()
        {
            byte[] data = new byte[1024];
            string message = "";
            while (bluetoothStream.DataAvailable)
            {
                await Task.Run(() => bluetoothStream.Read(data, 0, 16));
                message = System.Text.Encoding.UTF8.GetString(data);
            }
            return message;
        }

        public async Task<IEnumerable<ConnectionDevice>> DiscoverDevicesAsync()
        {
            List<ConnectionDevice> possibleDevices = new List<ConnectionDevice>();
            bluetooth = new BluetoothClient();
            var bluetoothDevices = await Task.Run(() => bluetooth.DiscoverDevicesInRange());
            foreach (var device in bluetoothDevices)
            {
                ConnectionDevice connectionDevice = new ConnectionDevice();
                connectionDevice.DeviceName = device.DeviceName;
                connectionDevice.DeviceConnectionAdress = Convert.ToString(device.DeviceAddress);
                possibleDevices.Add(connectionDevice);
            }
            return possibleDevices;
        }
    }
}
