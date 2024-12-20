using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AgenciappHome.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace AgenciappHome.Controllers.ApiRapidM
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ReservaController : ControllerBase
    {
        private readonly databaseContext _context;
        private IWebHostEnvironment _env;
        public ReservaController(databaseContext _context, IWebHostEnvironment env)
        {
            this._context = _context;
            this._env = env;
        }

        [HttpPost]
        public async Task<IActionResult> create([FromBody]Newtonsoft.Json.Linq.JObject value)
        {
            try
            {
                string ordernumber = DateTime.Now.ToString("yMMddHHmmssff");
                //Guardo el Json en un fichero con el numero de orden
                string sWebRootFolder = _env.WebRootPath;
                string path = sWebRootFolder + Path.DirectorySeparatorChar + "FileJsonReserva";
                path = Path.Combine(path, ordernumber + ".json");
                System.IO.File.WriteAllText(path, value.ToString());


                return Ok("Success");
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }

        }
    }
}