using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PimmPin.TrueIP
{
    internal class Program
    {
        private const string FilePath = ".\\LastIP.txt";

        public async static Task Main()
        {
            var hostAddresses = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (var hostAddress in hostAddresses)
                Console.WriteLine($"Found host address: {hostAddress.ToString()}");

            var ip = hostAddresses
                .FirstOrDefault<IPAddress>(ipAddress => ipAddress != null && ipAddress.ToString().StartsWith("192.168."));
            if (ip == null)
            {
                Console.WriteLine("No local IP addressed NIC found.");
                return;
            }

            Console.WriteLine($"Request will originate from: {ip}");
            Console.WriteLine($"Last IP file path: .\\LastIP.txt");

            var lastIp = File.Exists(".\\LastIP.txt")
                ? File.ReadAllText(".\\LastIP.txt").Trim()
                : (string)null;
            if (!string.IsNullOrEmpty(lastIp))
                Console.WriteLine($"Last IP: {lastIp}");
            else
                Console.WriteLine("Could not find last IP");

            var ipifyWebRequest = (HttpWebRequest)WebRequest.Create("https://api.ipify.org");
            ipifyWebRequest.UserAgent = "Chrome/41.0";
            ipifyWebRequest.Method = "GET";
            
            var httpClient = GetHttpClient(ip);
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Chrome/41.0");

            var ipifyHttpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.ipify.org");
            var ipifyResponse = await GetResponseAsync(httpClient, ipifyHttpRequestMessage);

            Console.WriteLine($"Public IP: {ipifyResponse}");

            if (ipifyResponse.Equals(lastIp))
            {
                Console.WriteLine("Current IP address matches old IP address. No update required.");
                return;
            }

            var domainHttpRequestMessage1 = new HttpRequestMessage(HttpMethod.Get, "https://domains.google.com/nic/update?hostname=@.shawnandjosh.com");
            domainHttpRequestMessage1.Headers.Authorization = new AuthenticationHeaderValue("Basic",  Convert.ToBase64String(Encoding.ASCII.GetBytes("xzOULtFjUj19dbtq:7PFPyQKuoJLBiBiL")));
            var domainResponse1 = await GetResponseAsync(httpClient, domainHttpRequestMessage1);
            
            var domainHttpRequestMessage2 = new HttpRequestMessage(HttpMethod.Get, "https://domains.google.com/nic/update?hostname=@.codyhink.com");
            domainHttpRequestMessage2.Headers.Authorization = new AuthenticationHeaderValue("Basic",  Convert.ToBase64String(Encoding.ASCII.GetBytes("ZdngFwrti6HeoGg3:JI3e2zfaEFXexKTg")));
            var domainResponse2 = await GetResponseAsync(httpClient, domainHttpRequestMessage2);
            
            var domainHttpRequestMessage3 = new HttpRequestMessage(HttpMethod.Get, "https://domains.google.com/nic/update?hostname=@.hotdoghallways.com");
            domainHttpRequestMessage3.Headers.Authorization = new AuthenticationHeaderValue("Basic",  Convert.ToBase64String(Encoding.ASCII.GetBytes("3Gh9cEzj9nBL6b4m:5RJWB9SWwdx78SqI")));
            var domainResponse3 = await GetResponseAsync(httpClient, domainHttpRequestMessage3);

            Console.WriteLine($"Domains response for shawnandjosh.com: {domainResponse1}");
            Console.WriteLine($"Domains response for codyhink.com: {domainResponse2}");
            Console.WriteLine($"Domains response for hotdoghallways.com: {domainResponse3}");

            using (var streamWriter = new StreamWriter(".\\LastIP.txt", false))
                streamWriter.WriteLine(ipifyResponse);

            Console.WriteLine($"Updated last IP file with: {ipifyResponse}");
        }

        public static HttpClient GetHttpClient(IPAddress address)
        {
            if (IPAddress.Any.Equals(address))
                return new HttpClient();

            var handler = new SocketsHttpHandler();

            handler.ConnectCallback = async (context, cancellationToken) =>
            {
                var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(address, 0));
                socket.NoDelay = true;

                try
                {
                    await socket.ConnectAsync(context.DnsEndPoint, cancellationToken).ConfigureAwait(false);

                    return new NetworkStream(socket, true);
                }
                catch
                {
                    socket.Dispose();

                    throw;
                }
            };

            return new HttpClient(handler);
        }

        private async static Task<string> GetResponseAsync(HttpClient httpClient, HttpRequestMessage request)
        {
            if (httpClient == null || request == null)
                return string.Empty;

            using (var response = await httpClient.SendAsync(request))
            {
                var contentString = await response.Content.ReadAsStringAsync();

                return contentString;
            }
        }
    }
}
