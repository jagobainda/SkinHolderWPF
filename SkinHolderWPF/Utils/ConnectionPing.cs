using System.Net.NetworkInformation;
using System.Windows;

namespace SkinHolderWPF.Utils
{
    public static class ConnectionPing
    {
        public static long GetPingTime(string url)
        {
            try
            {
                if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                {
                    url = url[7..];
                }
                else if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    url = url[8..];
                }

                using var ping = new Ping();

                var reply = ping.Send(url);
                if (reply.Status == IPStatus.Success)
                {
                    return reply.RoundtripTime;
                }

                return -1;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }
    }
}
