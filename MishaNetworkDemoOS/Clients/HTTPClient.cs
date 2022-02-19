using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MishaNetworkDemoOS.Clients
{
    public class HTTPClient
    {
        private TcpClient client;
        private Address address;
        private string HostName;
        public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.69 Safari/537.36";
        public HTTPClient(string hostName)
        {
            this.address = new Address(99, 198, 101, 250);//Utils.GetAddressFromName(hostName);
            this.HostName = hostName;
            client = new TcpClient(80);
        }
        
        public string GET(string url = "/")
        {
            Console.WriteLine("Connecting to: " + this.address.ToString() + " (" + HostName + ")");
            client.Connect(this.address, 80);
            string httpget = $"GET {url} HTTP/1.1\r\n" +
                                 $"User-Agent: Wget {UserAgent}\r\n" +
                                 "Accept: */*\r\n" +
                                 "Accept-Encoding: identity\r\n" +
                                 "Host: " + HostName + "\r\n" +
                                 "Connection: Keep-Alive\r\n\r\n";

            client.Send(Encoding.ASCII.GetBytes(httpget));

            var r = GetRequest(client);

            Console.WriteLine("Headers: ");
            foreach (var item in r.Headers)
            {
                Console.WriteLine(item.Key + "=" + item.Value);
            }


            try
            {
                client.Close();
            }
            catch (Exception x)
            {
                Console.WriteLine(x.Message);
            }


            return r.GetContentAsASCII();
        }


        //From: https://github.com/aura-systems/Aura-Operating-System/blob/68e20a133302649af36ac23b668ea870f097dd30/Aura%20Operating%20System/Aura_OS/System/Network/SimpleHttpServer/HttpProcessor.cs#L60
        private HttpResponse GetRequest(TcpClient client)
        {
            var ep = new EndPoint(Address.Zero, 0);

            //Read Request Line
            var bin = client.Receive(ref ep);
            string request = Encoding.ASCII.GetString(bin);

            var lines = request.Split("\r\n");

            string[] tokens = lines[0].Split(' ');

            if (tokens.Length != 3)
            {
                //throw new Exception("invalid http request line");
            }

            string method = tokens[0].ToUpper(); //http version
            string url = tokens[1]; //status code
            string protocolVersion = tokens[2]; //OK
            Console.WriteLine($"method: {method}, url: {url}, protocolVersion: {protocolVersion}");

            if(!int.TryParse(url, out int StatusCode))
            {
                throw new Exception("Failed to parse status code.");
            }

            //Read Headers
            Dictionary<string, string> headers = new Dictionary<string, string>();

            var lineNum = 0;
            List<string> lines2 = new List<string>();
            string currentLine = "";
            for (int i = 0; i < request.Length; i++)
            {
                if (i != request.Length - 1)
                {
                    var c = request[i];
                    var next = request[i + 1];

                    currentLine += c;

                    if (c == '\r' && next == '\n')
                    {
                        lines2.Add(currentLine);
                        lineNum++;
                    }
                }



            }


            //int i = 0;
            //int b = 0;
            //for (i = 1; i < lines.Length; i++)
            //{
            //    if (lines[i].Equals(""))
            //    {
            //        break;
            //    }

            //    int separator = lines[i].IndexOf(':');
            //    if (separator == -1)
            //    {
            //        throw new Exception("invalid http header line: " + lines[i]);
            //    }
            //    string name = lines[i].Substring(0, separator);
            //    int pos = separator + 1;
            //    while ((pos < lines[i].Length) && (lines[i][pos] == ' '))
            //    {
            //        pos++;
            //    }

            //    string value = lines[i].Substring(pos, lines[i].Length - pos);
            //    headers.Add(name, value);
            //    b++;
            //}
            //Console.WriteLine("i is " + i);

            return new HttpResponse()
            {
                Headers = headers,
                ResponseAsBinary = bin,
                ContentStartIndex = 0,
                StatusCode = StatusCode
            };
        }
    }
}