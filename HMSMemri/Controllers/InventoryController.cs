using HMSMemri.BaseSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using hmsBO;
using HMSMemri.Models;

namespace HMSMemri.Controllers
{
    public class InventoryController : IMCPBase
    {
        private readonly IOptions<BaseSettings.ServerVar> _serverVar;
        public InventoryController(IOptions<BaseSettings.ServerVar> serverVar) : base(serverVar)
        {
            _serverVar = serverVar;
        }

        public IActionResult Index()
        {
            return View();

        }

        
    }
}
