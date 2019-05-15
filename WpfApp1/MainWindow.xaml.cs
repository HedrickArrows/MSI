using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Guid uId = new Guid("0000110E-0000-1000-8000-00805f9b34fb");

        public List<string> items;
        public List<BluetoothDeviceInfo> devices;
        BluetoothDeviceInfo device;
        public MainWindow()
        {

            InitializeComponent();

            Refresh();
        }

        public void Refresh() {
            items = new List<string>();
            BluetoothClient _cli = new BluetoothClient();
            devices = _cli.DiscoverDevicesInRange().ToList();

            foreach (BluetoothDeviceInfo d in devices)
            {
                items.Add(d.DeviceName);
            }

            list.ItemsSource = items;
        }


        public void AttemptConnect()
        {
            device = devices[list.SelectedIndex];
            bool _isConnected = false;

            var serviceClass = BluetoothService.SerialPort;

            BluetoothClient _cli = new BluetoothClient();
            items = new List<string>();
            
            if (device == null)
            {

                conn.Text = "not found";
                return;
            }

            var ep = new BluetoothEndPoint(device.DeviceAddress, serviceClass);

            try
            {
                if (!device.Connected)
                {
                    _cli.Connect(ep);
                }
            }
            catch (System.Net.Sockets.SocketException e)
            {
                _cli.Close();
                _isConnected = false;
                conn.Text = "not connected";

                return;
            }

            _isConnected = true;

            if (!_isConnected)
                conn.Text = "not connected";
            else
                conn.Text = "connected";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void List_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            device = devices[list.SelectedIndex];
            dev.Text = device.DeviceName;
            //AttemptConnect();
            
            if (CanPair())
            {
                conn.Text = "attempting pairing...";
                Thread senderThread = new Thread(new ThreadStart(ClientConnectThread));
                senderThread.Start();
            }
            else
            {
                //UpdateStatus("Pair failed");
                conn.Text = "pair failed";

            }
        }


        private bool CanPair()
        {
            if (!device.Authenticated)
            {
                if (!BluetoothSecurity.PairRequest(device.DeviceAddress, ""))
                {
                    return false;
                }
            }
            return true;
        }

        private void ClientConnectThread()
        {
            BluetoothClient sender = new BluetoothClient();
            BluetoothAddress address = device.DeviceAddress;
            //sender.SetPin(deviceInfo.DeviceAddress, myPin);
            var endPoint = new BluetoothEndPoint(address, uId);

            try
            {
                if (!sender.Connected)
                {
                    sender.Connect(endPoint);
                }
            }
            catch (System.Net.Sockets.SocketException e)
            {
                sender.Close();
                //conn.Text = "not connected";

                return;
            }
            
            BluetoothClient client = new BluetoothClient();
            client.Connect(device.DeviceAddress, uId);
            client.BeginConnect(device.DeviceAddress, uId, this.BluetoothClientConnectCallback, client);
        }

        void BluetoothClientConnectCallback(IAsyncResult result)
        {
            BluetoothClient senderE = (BluetoothClient)result.AsyncState;
            senderE.EndConnect(result);

            conn.Text = "connected";
            Stream stream = senderE.GetStream();
            
            while (true)
            {
                /*
                while (!ready) ;
                byte[] message = Encoding.ASCII.GetBytes(txtSenderMessage.Text);

                stream.Write(message, 0, message.Length);
                */
            }
            
        }

    }


}
