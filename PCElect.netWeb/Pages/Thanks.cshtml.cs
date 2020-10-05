using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace WebApplication1.Pages
{
    public class ThanksModel : PageModel
    {
        private readonly ILogger<ThanksModel> _logger;

        public ThanksModel(ILogger<ThanksModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}
