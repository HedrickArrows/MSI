using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WpfApp1
{
    public static class BluetoothHandler
    {
        #region Fields

        private static BluetoothClient _cli;
        private static bool _isConnected = false;
        private static Guid uId = new Guid("f9b6d358-ecec-4231-9a98-08a62696e68f");

        #endregion Fields

        #region Methods

        public static void Close()
        {
            if (_cli != null)
            {
                _cli.Close();
            }
        }

        private static void createClient()
        {
            _cli = new BluetoothClient();
        }

        public static bool CanPair(BluetoothDeviceInfo device)
        {
            if(_cli == null)
            {
                createClient();
            }
            if (!device.Authenticated)
            {
                if (!BluetoothSecurity.PairRequest(device.DeviceAddress, null))
                {
                    return false;
                }
            }
            _cli.SetPin(null);
            return true;
        }

        public static List<BluetoothDeviceInfo> GetListOfDevices()
        {
            if(_cli == null)
            {
                createClient();
            }

            return _cli.DiscoverDevicesInRange().ToList();
        }

        public static string GetStrFromBluetooth()
        {
            Console.WriteLine("Getting message...");
            Stream peerStream = _cli.GetStream();

            byte[] buffer = new byte[1000];
            string str = string.Empty;
            byte length = (byte)peerStream.ReadByte();
            int byteRead = peerStream.ReadByte();

            for (int i = 0; i < length; ++i)
            {
                str += (char)byteRead;
                buffer[i] = (byte)byteRead;
                byteRead = peerStream.ReadByte();
            }

            Console.WriteLine("Received {0} bytes", length);

            peerStream.ReadByte();
            peerStream.Flush();

            return str;
        }

        public static bool IsConnected()
        {
            return _isConnected;
        }

        public static bool MakeConnection(BluetoothDeviceInfo bluetoothDeviceInfo)
        {
            Console.WriteLine("Trying to connect...");
            var serviceClass = BluetoothService.Handsfree;
            if (_cli != null)
            {
                _cli.Close();
            }

            _cli = new BluetoothClient();
            BluetoothDeviceInfo device = bluetoothDeviceInfo;

            if (device == null)
            {
                return false;
            }

            Console.WriteLine("Address : " + device.DeviceAddress);
            var ep = new BluetoothEndPoint(device.DeviceAddress, serviceClass);

            try
            {
                if (!device.Connected)
                {
                    _cli.Connect(ep);
                    Console.WriteLine("Connected to " + device.DeviceName);
                }
            }
            catch (System.Net.Sockets.SocketException e)
            {
                _cli.Close();
                _isConnected = false;
                Console.WriteLine(e.ToString());
                return false;
            }

            _isConnected = true;
            return true;
        }

        #endregion Methods

    }
}
