using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public List<string> items;
        public List<BluetoothDeviceInfo> devices;
        public MainWindow()
        {

            InitializeComponent();

            Refresh();
        }

        public void Refresh() {
            items = new List<string>(); ;
            BluetoothClient _cli = new BluetoothClient();
            devices = _cli.DiscoverDevicesInRange().ToList();

            foreach (BluetoothDeviceInfo d in devices)
            {


                items.Add(d.DeviceName);
            }

            list.ItemsSource = items;
        }


        public void AttemptConnect(BluetoothDeviceInfo device) {
            bool _isConnected = false;

            var serviceClass = BluetoothService.SerialPort;

            BluetoothClient _cli = new BluetoothClient();
            items = new List<string>();

            dev.Text = device.DeviceName;

            //return;

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
            AttemptConnect(devices[list.SelectedIndex]);
        }
    }
}
