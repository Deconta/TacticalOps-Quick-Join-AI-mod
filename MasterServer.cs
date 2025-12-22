using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TacticalOpsQuickJoin
{
    public class MasterServer
    {
        public class MasterServerInfo
        {
            public string? Address { get; set; }
            public int Port { get; set; }
        }

        public struct MasterServerResponse
        {
            public string[] serverList;
            public int errorCode;
            public string errorMessage;
        }

        public static async Task<MasterServerResponse> DownloadServerListAsync(MasterServerInfo serverInfo)
        {
            MasterServerResponse masterServerResponse = new MasterServerResponse();
            
            // HIER: Timeout auf 2 Sekunden gesetzt, wie gewünscht
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2)))
            {
                try
                {
                    if (serverInfo.Address == null)
                        throw new ArgumentNullException(nameof(serverInfo.Address));

                    using (TcpClient tcp = new TcpClient())
                    {
                        // Verbinden mit Timeout Token
                        await tcp.ConnectAsync(serverInfo.Address, serverInfo.Port, cts.Token);

                        using (NetworkStream ns = tcp.GetStream())
                        {
                            byte[] buffer = new byte[8192]; 

                            // 1. Auf Initial-Daten warten (Challenge vom Masterserver)
                            int bytesRead = await ns.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                            string initialResponse = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                            if (string.IsNullOrEmpty(initialResponse))
                                throw new Exception("Masterserver sent no data.");

                            // 2. Security Code berechnen (Challenge-Response)
                            string secureCode = GetSecureCode(initialResponse);
                            if(string.IsNullOrEmpty(secureCode)) 
                                throw new Exception("Invalid secure code format.");

                            string secureResponse = MakeValidate(Encoding.ASCII.GetBytes(secureCode), Encoding.ASCII.GetBytes("Z5Nfb0"));
                            
                            // 3. Validierung senden
                            string returnString = $"\\gamename\\ut\\location\\0\\validate\\{secureResponse}\\final\\";
                            byte[] responseBytes = Encoding.ASCII.GetBytes(returnString);
                            await ns.WriteAsync(responseBytes, 0, responseBytes.Length, cts.Token);
                            
                            // 4. Serverliste anfordern
                            byte[] requestServerlist = Encoding.ASCII.GetBytes("\\list\\\\gamename\\ut\\final\\");
                            await ns.WriteAsync(requestServerlist, 0, requestServerlist.Length, cts.Token);

                            // 5. Antwort empfangen (in Chunks, bis "\final\" kommt)
                            StringBuilder serverListRaw = new StringBuilder();
                            bool containsFinal = false;

                            while (!containsFinal)
                            {
                                bytesRead = await ns.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                                if (bytesRead == 0) break; // Verbindung wurde vom Server geschlossen

                                string chunk = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                                serverListRaw.Append(chunk);
                                
                                if (serverListRaw.ToString().Contains("\\final\\"))
                                {
                                    containsFinal = true;
                                }
                            }

                            // 6. Liste parsen
                            string[] serverList = serverListRaw.ToString().Split(new string[] { "\\ip\\", "\\final\\" }, StringSplitOptions.RemoveEmptyEntries);
                            masterServerResponse.errorCode = 0;
                            masterServerResponse.serverList = serverList;
                            return masterServerResponse;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    masterServerResponse.errorCode = 2;
                    masterServerResponse.errorMessage = "Connection timed out (2s).";
                }
                catch (Exception ex)
                {
                    masterServerResponse.errorCode = 1;
                    masterServerResponse.errorMessage = ex.Message;
                }
            }

            return masterServerResponse;
        }

        // --- Hilfsmethoden für die Verschlüsselung ---

        private static string GetSecureCode(string s) {
            try {
                string[] parts = s.Split('\\');
                // Der Secure Key steht an Index 4 im Unreal-Protokoll
                return parts.Length > 4 ? parts[4] : "";
            } catch { return ""; }
        }

        private static string MakeValidate(byte[] securekey, byte[] handoff) {
            byte[] table = new byte[256];
            int[] temp = new int[4];

            // Puffer initialisieren
            for (short i = 0; i < 256; i++) table[i] = (byte)i;

            // Scramble mit Handoff
            for (short i = 0; i < 256; i++)
            {
                temp[0] = temp[0] + table[i] + handoff[i % handoff.Length] & 255;
                temp[2] = table[temp[0]];
                table[temp[0]] = table[i];
                table[i] = (byte)temp[2];
            }

            // Scramble mit SecureKey
            temp[0] = 0;
            byte[] key = new byte[securekey.Length];
            if (key.Length < 6) key = new byte[6]; // Safety Fallback

            for (byte i = 0; i < securekey.Length; i++)
            {
                key[i] = securekey[i];
                temp[0] = (temp[0] + key[i] + 1) & 255;
                temp[2] = table[temp[0]];
                temp[1] = (temp[1] + temp[2]) & 255;
                temp[3] = table[temp[1]];
                table[temp[1]] = Convert.ToByte(temp[2]);
                table[temp[0]] = Convert.ToByte(temp[3]);
                key[i] = Convert.ToByte(key[i] ^ table[(temp[2] + temp[3]) & 255]);
            }

            // Validierungsschlüssel generieren
            int length = Convert.ToInt32(securekey.Length / 3);
            StringBuilder sb = new StringBuilder();
            byte j = 0;
            while (length >= 1)
            {
                length--;
                temp[2] = key[j];
                temp[3] = key[j + 1];
                sb.Append(AddChar(temp[2] >> 2));
                sb.Append(AddChar(((temp[2] & 3) << 4) | (temp[3] >> 4)));
                temp[2] = key[j + 2];
                sb.Append(AddChar(((temp[3] & 15) << 2) | (temp[2] >> 6)));
                sb.Append(AddChar(temp[2] & 63));
                j = Convert.ToByte(j + 3);
            }
            return sb.ToString();
        }

        private static char AddChar(int value) {
            if (value < 26) return Convert.ToChar(value + 65);
            if (value < 52) return Convert.ToChar(value + 71);
            if (value < 62) return Convert.ToChar(value - 4);
            if (value == 62) return '+';
            if (value == 63) return '/';
            return Convert.ToChar(0);
        }
    }
}
