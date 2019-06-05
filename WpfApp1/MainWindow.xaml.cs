using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
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
        private static BluetoothClient client;
        private static NetworkStream stream = null;
        
        private static BluetoothEndPoint endPoint;

        public MainWindow()
        {

            try
            {
                endPoint = new BluetoothEndPoint(BluetoothRadio.PrimaryRadio.LocalAddress,
                    BluetoothService.BluetoothBase);
            } catch (Exception e) {
                System.Windows.Forms.MessageBox.Show("Please enable Bluetooth before using this application");

                if (System.Windows.Forms.Application.MessageLoop)
                {
                    // WinForms app
                    System.Windows.Forms.Application.Exit();
                }
                else
                {
                    // Console app
                    System.Environment.Exit(1);
                }
            }
            client = new BluetoothClient(endPoint);

            InitializeComponent();

            Refresh();
        }

        public void Refresh() {
            items = new List<string>();
            client = new BluetoothClient(endPoint);
            devices = client.DiscoverDevicesInRange().ToList();

            foreach (BluetoothDeviceInfo d in devices)
            {
                items.Add(d.DeviceName);
            }

            list.ItemsSource = items;
        }

        /*
        public void AttemptConnect()
        {
            device = devices[list.SelectedIndex];
            bool _isConnected = false;

            var serviceClass = BluetoothService.SerialPort;

            BluetoothClient _cli = new BluetoothClient();
            //items = new List<string>();
            
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
        */
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void List_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            device = devices[list.SelectedIndex];
            dev.Text = device.DeviceName;
            //AttemptConnect();

            conn.Text = "Attempting pairing...";

            if (CanPair())
            {

                BluetoothAddress address = device.DeviceAddress;
                //endPoint = new BluetoothEndPoint(address, BluetoothService.BluetoothBase);
                client = new BluetoothClient(endPoint);

                conn.Text = "Estabilishing connection...";


                try
                {
                    client.BeginConnect(endPoint, new AsyncCallback(Connect), device);
                }
                catch (Exception exc)
                {
                    client.Close();
                    Dispatcher.Invoke(() => {
                        conn.Text = "Connection failed";
                    });
                    Debug.WriteLine(exc.ToString());
                    return;
                }

               

            }
            else
            {
                //UpdateStatus("Pair failed");
                conn.Text = "Connection failed";

            }
        }


        private bool CanPair()
        {
            if (!device.Authenticated)
            {
                if (!BluetoothSecurity.PairRequest(device.DeviceAddress, null))
                {
                    return false;
                }
            }
            client.SetPin(null);
            return true;
        }

        void Connect(IAsyncResult result)
        {


            if (result.IsCompleted)
            {
                Dispatcher.Invoke(() => {
                    conn.Text = "Connected";
                } );

                stream = client.GetStream(); //powinno być połączone
                        //ale może sypać wyjątkiem że socket nie jest połączony

                if (stream.CanRead)
                {
                    byte[] myReadBuffer = new byte[1024];
                    StringBuilder myCompleteMessage = new StringBuilder();
                    int numberOfBytesRead = 0;

                    // Incoming message may be larger than the buffer size. 
                    do
                    {
                        numberOfBytesRead = stream.Read(myReadBuffer, 0, myReadBuffer.Length);

                        for (int i = 0; i < numberOfBytesRead; i++)
                            myCompleteMessage.AppendFormat("0x{0:X2} ", myReadBuffer[i]);
                    }
                    while (stream.DataAvailable);

                    // Print out the received message to the console.
                    //Console.WriteLine("You received the following message : " + myCompleteMessage);
                    Dispatcher.Invoke(() => {
                        DataBox.Text = myCompleteMessage.ToString();
                    });
                }
                else
                {
                    Dispatcher.Invoke(() => {
                        DataBox.Text = "Sorry.  \nYou cannot read \nfrom this NetworkStream";
                    });
                    //Console.WriteLine("Sorry.  You cannot read from this NetworkStream.");
                }
                /*
                int time = 0;
                while (true) {
                    if (time++ > 10000000000)
                        break;
                    //zastąpić kodem komunikacji
                }
                */
                // client is connected now :)
            }


            Dispatcher.Invoke(() => {
                conn.Text = "Connection ended";
            });
        }

        /*
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
                /
            }
            
        }
        */
    }


}
