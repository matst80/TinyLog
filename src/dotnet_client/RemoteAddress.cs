using System.Net;

namespace TinyLog
{
    internal struct RemoteAddress
    {
        public IPAddress Address;
        public int Port;

        public byte[] AuthKey { get; internal set; }
    }
}
