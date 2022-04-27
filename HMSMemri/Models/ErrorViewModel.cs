using System;

namespace HMSMemri.Models
{
    public class ServerVar
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
