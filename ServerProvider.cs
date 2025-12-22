#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TacticalOpsQuickJoin
{
    public class ServerProvider : IServerProvider
    {
        private readonly ISettingsService _settingsService;
        private readonly List<MasterServer.MasterServerInfo> _masterServers = [];
        private readonly SemaphoreSlim _pingSemaphore = new(Constants.MAX_CONCURRENT_PINGS);

        public ServerProvider(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            LoadMasterServersFromSettings();
        }

        public async Task GetServerListAsync(Action<ServerData> onServerFound)
        {
            var allIPs = new HashSet<string>();
            var downloadTasks = _masterServers.Select(ms => MasterServer.DownloadServerListAsync(ms));
            var responses = await Task.WhenAll(downloadTasks);
            foreach (var ip in responses.Where(r => r.errorCode == 0 && r.serverList != null).SelectMany(r => r.serverList!))
            {
                allIPs.Add(ip);
            }

            var pingTasks = allIPs.Select(async (ip, index) =>
            {
                var serverData = await QueryServerInfoAsync(index, ip);
                if (serverData != null)
                {
                    onServerFound(serverData);
                }
            });
            await Task.WhenAll(pingTasks);
        }

        public async Task GetServerDetailsAsync(ServerData serverData)
        {
            try
            {
                using var udp = new UdpClient();
                udp.Connect(serverData.ServerIP, serverData.ServerPort);

                // Initial status request
                var dataStatus = Encoding.UTF8.GetBytes(@"\status\");
                await udp.SendAsync(dataStatus, dataStatus.Length);
                
                var (statusResponseData, statusLogBytes) = await ReceiveDataUntilTimeout(udp);
                serverData.UpdateInfo(statusResponseData);

                // If not all info received or players info needed, send players request
                // This mimics the original project's logic of sending players after status data might have been received
                if (!statusResponseData.Contains(@"\final\") && !string.IsNullOrEmpty(statusResponseData))
                {
                    var dataPlayers = Encoding.UTF8.GetBytes(@"\players\");
                    await udp.SendAsync(dataPlayers, dataPlayers.Length);
                    var (playersResponseData, playersLogBytes) = await ReceiveDataUntilTimeout(udp);
                    serverData.UpdateInfo(playersResponseData);

                    // Append player log bytes to status log bytes for a combined view
                    statusLogBytes = statusLogBytes.Concat(playersLogBytes).ToArray();
                }

                // Log raw bytes for debugging purposes
                try
                {
                    File.WriteAllBytes("server_response.log", statusLogBytes);
                }
                catch { /* Ignore logging errors */ }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting details for {serverData.ServerIP}:{serverData.ServerPort}: {ex.Message}");
            }
        }

        private async Task<(string, byte[])> ReceiveDataUntilTimeout(UdpClient udp)
        {
            var allData = new StringBuilder();
            var allBytes = new List<byte>();
            
            // Give the server up to 3 seconds to respond in total for this part
            var overallTimeoutCts = new CancellationTokenSource(3000); 

            while (!overallTimeoutCts.IsCancellationRequested)
            {
                try
                {
                    // Wait for a packet for up to 500ms
                    var receiveTask = udp.ReceiveAsync(overallTimeoutCts.Token);
                    var completedTask = await Task.WhenAny(receiveTask.AsTask(), Task.Delay(500, overallTimeoutCts.Token));

                    if (completedTask == receiveTask.AsTask())
                    {
                        var result = await receiveTask;
                        allBytes.AddRange(result.Buffer);
                        string responsePart = Encoding.UTF8.GetString(result.Buffer);
                        allData.Append(responsePart);
                    }
                    else
                    {
                        // 500ms passed without a new packet, assume we are done for this phase
                        break; 
                    }
                }
                catch (OperationCanceledException)
                {
                    // Overall timeout reached
                    break;
                }
                catch (SocketException)
                {
                    // Connection reset by peer or other socket error, stop receiving
                    break;
                }
            }
            return (allData.ToString(), allBytes.ToArray());
        }

        private async Task<ServerData?> QueryServerInfoAsync(int id, string ipStr)
        {
            if (!ValidationHelper.IsValidServerAddress(ipStr)) return null;

            await _pingSemaphore.WaitAsync();
            try
            {
                var parts = ipStr.Split(':');
                var serverData = new ServerData(id, ipStr);
                using var udp = new UdpClient();
                udp.Connect(parts[0], int.Parse(parts[1]));

                var data = Encoding.UTF8.GetBytes(@"\info\");

                var sw = Stopwatch.StartNew();
                await udp.SendAsync(data, data.Length);
                var receiveTask = udp.ReceiveAsync();

                if (await Task.WhenAny(receiveTask, Task.Delay(Constants.DEFAULT_UDP_TIMEOUT)) == receiveTask)
                {
                    sw.Stop();
                    var result = await receiveTask;
                    serverData.Ping = Math.Max(1, (int)sw.ElapsedMilliseconds);
                    serverData.SetInfo(Encoding.UTF8.GetString(result.Buffer));

                    if (serverData.IsTO220 || serverData.IsTO340 || serverData.IsTO350)
                    {
                        return serverData;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error querying server {ipStr}: {ex.Message}");
            }
            finally
            {
                _pingSemaphore.Release();
            }
            return null;
        }

        private void LoadMasterServersFromSettings()
        {
            if (string.IsNullOrWhiteSpace(_settingsService.MasterServers)) return;
            foreach (var line in _settingsService.MasterServers.Split('\n'))
            {
                var parts = line.Split(':');
                if (parts.Length == 2) _masterServers.Add(new MasterServer.MasterServerInfo { Address = parts[0], Port = Convert.ToInt16(parts[1]) });
            }
        }
    }
}
