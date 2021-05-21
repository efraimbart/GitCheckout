namespace GitCheckout.Classes
{
    public class Protocol
    {
        public string Scheme { get; set; }
        public string Host { get; set; }
        public string Query { get; set; }

        public Protocol()
        {
        }
            
        public Protocol(string protocol)
        {
            var protocolParts = protocol.Split(',');

            Scheme = protocolParts[0];
            Host = protocolParts[1];
            Query = protocolParts[2];
        }

        public override string ToString()
        {
            return $@"{Scheme},{Host},{Query}";
        }
    }
}