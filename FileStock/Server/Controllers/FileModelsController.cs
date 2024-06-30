using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using NuGet.Common;
using Server.Data;
using Server.Models;
using Server.Services;
using static Server.Controllers.FileModelsController;

namespace Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FileModelsController : ControllerBase
    {
        private readonly ServerContext _context;
        private readonly ICryptService _crypt;
        private readonly IArchiverService _archiver;

        public FileModelsController(ServerContext context, ICryptService crypt, IArchiverService archiver)
        {
            _context = context;
            _crypt = crypt;
            _archiver = archiver;
        }

        // GET: /FileModels/Get
        [HttpGet("Get")]
        public async Task<ActionResult<List<FileInfoModel>>> GetFileModel(string Login, string Token)
        {
            User? usr;
            ActionResult? res;
            if (!IsAuthorized(new Req { Login = Login, Token = Token}, out usr, out res))
            {
                return res!;
            }

            var fileModelList = _context.FileModel.Where(fl => fl.UserID == usr.Id).Select(fl => new FileInfoModel { Id = fl.Id, Name = fl.Name, Size = fl.Size, FileExt = fl.FileExt}).ToList();

            if (fileModelList == null)
            {
                return NotFound();
            }

            return Ok(fileModelList);
        }
        // POST: /FileModels/Add
        [HttpPost("Add")]
        public async Task<ActionResult<FileModel>> Add(AddReq req)
        {
            User? usr;
            ActionResult? res;
            if (!IsAuthorized(req, out usr, out res))
            {
                return res!;
            }

            if (_context.FileModel.Any(x => x.Name == req.Name))
            {
                return BadRequest("File almost exists");
            }
            req.Data = _archiver.Compress(req.Data);

            _context.FileModel.Add(new FileModel() { Data = req.Data, Name = req.Name, User = usr });
            _context.SaveChanges();

            FileModel newfl = _context.FileModel.Where(x => x.UserID == usr.Id && x.Name == req.Name && x.Data == req.Data).OrderByDescending(x => x.Id).First(); //Id создается в базе данных, поэтому надо сходить

            return Ok(newfl);
        }

        // DELETE: /FileModels/Delete
        [HttpPost("Delete")]
        public async Task<IActionResult> DeleteFileModel(DeleteReq req)
        {
            User? usr;
            ActionResult? res;
            if (!IsAuthorized(req, out usr, out res))
            {
                return res!;
            }

            var fileModel = await _context.FileModel.FindAsync(req.Id);
            if (fileModel == null)
            {
                return NotFound("false");
            }

            _context.FileModel.Remove(fileModel);
            await _context.SaveChangesAsync();

            return Ok("true");
        }

        [HttpPost("Download")]
        public async Task<ActionResult<Stream>> Download(DownloadReq req)
        {
            User? usr;
            ActionResult? res;
            if (!IsAuthorized(req, out usr, out res))
            {
                return res!;
            }

            return new MemoryStream(_context.FileModel.Where(fl => fl.Id == req.Id).Select(fl => _archiver.Decompress(fl.Data)).FirstOrDefault());
        }

        private bool FileModelExists(int id)
        {
            return _context.FileModel.Any(e => e.Id == id);
        }

        // TODO : Класс-сервис авторизации, которым могут пользоваться контроллдлеры (AuthController и FileModelController)
        private bool IsAuthorized(Req req, out User? usr, out ActionResult? res) //TODO : токен дествует только некоторе время, при просрчивании просит клиента авторизоваться заново
        {
            usr = _context.User.Where(usr => usr.Name == req.Login).FirstOrDefault();

            if (usr == null)
            {
                res = NotFound("User doesn`t exists");
                return false;
            }

            byte[] f_step = Convert.FromBase64String(req.Token.Replace(' ', '+'));
            byte[] s_step = _crypt.DecryptData(f_step, usr.PublicKey);
            string t_step = Encoding.UTF8.GetString(s_step);

            if (usr.Token != t_step)
            {
                res = Forbid();
                return false;
            }

            res = null;
            return true;
        }

        /*private bool FakeAuthorized(Req req, out User? usr, out ActionResult? res)
        {
            usr = _context.User.Where(usr => usr.Name == req.Login).FirstOrDefault();
            res = null;
            return true;
        }*/

        public class Req
        {
            public string Login { get; set; }
            public string Token { get; set; }
        }

        public class DownloadReq : Req
        {
            public int Id { get; set; }
        }

        public class DeleteReq : Req
        {
            public int Id { get; set; }
        }

        public class AddReq : Req
        {
            public string Name { get; set; }
            public byte[] Data {  get; set; }
        }
    }
}
