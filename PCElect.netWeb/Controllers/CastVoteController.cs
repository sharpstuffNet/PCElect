using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PCElect.netWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CastVoteController : ControllerBase
    {
        IConfiguration _cfg;

        public CastVoteController(IConfiguration cfg)
        {
            _cfg = cfg;
        }

        public class Voteing
        {
            public string VID { get; set; }
            public string[] Vote { get; set; }
        }

        [HttpGet]
        public string GetIt() => "Test";

        [HttpPost]
        public void Vote([FromBody] Voteing vote)
        {
            var dir = _cfg.GetValue("Dir", "");
            var pce = new PCElect.Lib.PCElect(dir, _cfg, null);
            pce.Vote(vote.VID, vote.Vote);
        }
    }
}
