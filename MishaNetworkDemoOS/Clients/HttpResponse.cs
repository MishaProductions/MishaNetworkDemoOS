using System.Collections.Generic;
using System.Text;

namespace MishaNetworkDemoOS.Clients
{
    public class HttpResponse
    {
        public int StatusCode { get; internal set; }
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public byte[] ResponseAsBinary { get; internal set; }
        public int ContentStartIndex { get; internal set; }
        public string GetContentAsASCII()
        {
            return Encoding.ASCII.GetString(ResponseAsBinary); //, ContentStartIndex, ResponseAsBinary.Length - ContentStartIndex);
        }
    }
}