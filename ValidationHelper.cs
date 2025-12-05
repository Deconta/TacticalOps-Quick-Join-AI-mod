using System;
using System.Net;

namespace TacticalOpsQuickJoin
{
    internal static class ValidationHelper
    {
        public static bool IsValidIPAddress(string ip)
        {
            return !string.IsNullOrWhiteSpace(ip) && IPAddress.TryParse(ip, out _);
        }
        
        public static bool IsValidPort(int port)
        {
            return port >= Constants.MIN_PORT && port <= Constants.MAX_PORT;
        }
        
        public static bool IsValidServerAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return false;
                
            var parts = address.Split(':');
            if (parts.Length != 2)
                return false;
                
            return IsValidIPAddress(parts[0]) && 
                   int.TryParse(parts[1], out int port) && 
                   IsValidPort(port);
        }
        
        public static bool IsValidRefreshInterval(int seconds)
        {
            return seconds == 0 || 
                   (seconds >= Constants.MIN_REFRESH_INTERVAL && 
                    seconds <= Constants.MAX_REFRESH_INTERVAL);
        }
    }
}
