using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HMSMemri.BaseSettings
{
    public class IMCPBase : Controller

    {
        private readonly IOptions<ServerVar> _serverVar;
        public IMCPBase(IOptions<ServerVar> serverVar)
        {
            _serverVar = serverVar;
        }
        protected GlobalData Global = null;
    }
}
