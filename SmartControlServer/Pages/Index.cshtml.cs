using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SmartControlServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartControlServer.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IStoreContext _storeContext;

        public IList<StoreContext.Client> Clients { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IStoreContext storeContext)
        {
            _logger = logger;
            _storeContext = storeContext;
        }

        public void OnGet()
        {
            Clients = _storeContext.GetAllClients();
        }
    }
}
