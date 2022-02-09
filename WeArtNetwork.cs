/**
*	WEART - Network helper
*	https://www.weart.it/
*/

using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Linq;

namespace WeArt.Core
{
    /// <summary>
    /// Utility class for networking operations
    /// </summary>
    public static class WeArtNetwork
    {

        private static string _localIpAddress;

        /// <summary>
        /// The local ip address of the main network interface
        /// </summary>
        public static string LocalIPAddress
        {
            get
            {
                if (_localIpAddress == null)
                {
                    var validAdapters = from adapter in NetworkInterface.GetAllNetworkInterfaces()
                                        let properties = adapter.GetIPProperties()
                                        from address in properties.UnicastAddresses
                                        where address.Address.AddressFamily == AddressFamily.InterNetwork &&
                                            address.DuplicateAddressDetectionState == DuplicateAddressDetectionState.Preferred &&
                                            adapter.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                            adapter.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                                            adapter.OperationalStatus == OperationalStatus.Up &&
                                            adapter.Name.StartsWith("vEthernet") == false
                                        select address.Address;

                    if (validAdapters.Count() > 0)
                        _localIpAddress = validAdapters.FirstOrDefault().ToString();
                }
                return _localIpAddress;
            }
        }
    }
}