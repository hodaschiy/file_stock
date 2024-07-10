using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Server.Data;
using Server.Models;
using Server.Services;

namespace Server.Controllers
{
    [Authorize]
    [Route("[Action]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ServerContext _context;
        private readonly ILogger<ValuesController> _logger;

        public ValuesController(ServerContext context, ICryptService crypt, ILogger<ValuesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public List<Tuple<int, string>> GetUsersList()
        {
            string name = HttpContext.User.Claims.Where(x => x.Subject.IsAuthenticated).FirstOrDefault().Value;
            return _context.User.Where(x => x.Name != name).Select(usr => new Tuple<int, string>(usr.Id, usr.Name)).ToList();
        }
    }
}
