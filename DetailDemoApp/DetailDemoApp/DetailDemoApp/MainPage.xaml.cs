using Newtonsoft.Json;
using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DetailDemoApp
{
    public partial class MainPage : ContentPage
    {
        public Nic[] Nics { get; set; }
        public MainPage()
        {
            InitializeComponent();
            LoadNics();
            BindingContext = this;
            Details.IsVisible = false;
        }
        private async void HostName_Clicked(object sender, EventArgs e)
        {
            if(HostName.Text!=null)
            {
                var value = HostName.Text;
                var status = await CrossConnectivity.Current.IsRemoteReachable(value);
                IPAddress[] ipaddress = Dns.GetHostAddresses(value);
                //var status = await CrossConnectivity.Current.IsRemoteReachable("google.com");
                Status.Text = "Status:- " + status;
                Console.WriteLine(ipaddress[0].ToString());
                Console.WriteLine("IPAddress of " + HostName + " is");
                foreach (IPAddress ip4 in ipaddress)
                {
                    Console.WriteLine(ip4);
                }
                Console.Write("IPv6 of Machine is ");
                foreach (IPAddress ip6 in ipaddress)
                {
                    Console.WriteLine(ip6);
                }
                string IPAdd = "142.250.67.68";
                //string IPAdd = "172.217.14.196";
                IPHostEntry hostEntry = Dns.GetHostEntry(IPAdd);
                Console.WriteLine(hostEntry.HostName);
            }
            else
            {
               await DisplayAlert("Error", "Enter Ip address or Website URL", "Ok");
            }
            
        }

        private void IpAddressforSUBMask_Clicked(object sender, EventArgs e)
        {
            var IpAddress = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault();
            GetSubnetMask(IpAddress);
        }

        private void ConnectedNetworkIpAddressLoacl_Clicked(object sender, EventArgs e)
        {
            string Myhost = System.Net.Dns.GetHostName();
            string myIP = Dns.GetHostAddresses(Myhost)[0].AddressFamily.ToString();

            var IPAddress=GetLocalAddress();
            ConnectedNetworkLocalIpAddress.Text = IPAddress;
            
        }

        private void MACAddress_Clicked(object sender, EventArgs e)
        {
            var macaddress=GetDeviceInfo();
            //MACAddress.Text = macaddress;
            
        }

        private string GetDeviceInfo()
        {
            string mac = string.Empty;
            string ip = string.Empty;

            foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (netInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                    netInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    var address = netInterface.GetPhysicalAddress();
                    mac = BitConverter.ToString(address.GetAddressBytes());

                    IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName());
                    if (addresses != null && addresses[0] != null)
                    {
                        ip = addresses[0].ToString();
                        break;
                    }
                }
            }

            return mac;
        }

        private string GetLocalAddress()
        {
            var IpAddress = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault();
            if (IpAddress != null)
                return IpAddress.ToString();

            return "Could not locate IP Address";
        }

        public IPAddress GetSubnetMask(IPAddress address)
        {
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (address.Equals(unicastIPAddressInformation.Address))
                        {
                            IpAddressSubMusk.Text = unicastIPAddressInformation.IPv4Mask.ToString();
                            return unicastIPAddressInformation.IPv4Mask;
                        }
                    }
                }
            }
            throw new ArgumentException(string.Format("Can't find subnetmask for IP address '{0}'", address));
        }

        private async void PublicIpAddress_Clicked(object sender, EventArgs e)
        {

            var client = new HttpClient();
            var response = await client.GetAsync("https://api.ipify.org/?format=json");
            var resultString = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<IpResult>(resultString);

            var yourIp = result.Ip;
            PublicIpAddress.Text = yourIp;
        }

        //private string getmacAddressmob()
        //{
            
        //    var macaddress = (from nic in NetworkInterface.GetAllNetworkInterfaces() where nic.OperationalStatus == OperationalStatus.Up select nic.GetPhysicalAddress().ToString()).FirstOrDefault();
        //    return macaddress.ToString();
        //}

        private void LoadNics()
        {
            var nics = new List<Nic>();
            var ifs = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var i in ifs)
            {
                var n = new Nic
                {
                    Name = i.Name,
                    Description = i.Description,
                    Status = i.OperationalStatus,
                    Type = i.NetworkInterfaceType
                };

                if (i.Supports(NetworkInterfaceComponent.IPv4))
                    n.Supports += "IPv4 ";
                if (i.Supports(NetworkInterfaceComponent.IPv6))
                    n.Supports += "IPv6 ";

                foreach (var a in i.GetIPProperties().UnicastAddresses)
                {
                    if (a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                        n.IPv6Addresses += a.Address.ToString() + " ";

                    if (a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        n.IPv4Addresses += a.Address.ToString() + " ";
                }

                nics.Add(n);
            }

            Nics = nics.ToArray();
        }

        private void MoreDetails_Clicked(object sender, EventArgs e)
        {
            Details.IsVisible = true;
        }
    }
    public class IpResult
    {
        public string Ip { get; set; }
    }

    public class Nic
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public OperationalStatus Status { get; set; }
        public NetworkInterfaceType Type { get; set; }
        public string Supports { get; set; }
        public string IPv4Addresses { get; set; }
        public string IPv6Addresses { get; set; }
    }
}
