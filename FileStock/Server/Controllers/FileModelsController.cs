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
using Server.Data;
using Server.Models;
using Server.Services;
using static Server.Controllers.FileModelsController;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.AspNetCore.Http.Extensions;

namespace Server.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class FileModelsController : ControllerBase
    {
        private readonly ServerContext _context;
        private readonly ICryptService _crypt;
        //private readonly IArchiverService _archiver;
        private readonly ILogger<FileModelsController> _logger;

        public FileModelsController(ServerContext context, ICryptService crypt, /*IArchiverService archiver,*/ ILogger<FileModelsController> logger)
        {
            _context = context;
            _crypt = crypt;
            //     _archiver = archiver;
            _logger = logger;
        }
        
        // GET: /FileModels/Get
        [HttpGet("Get")]
        public async Task<ActionResult<List<FileInfoModel>>> GetFileModel()
        {
            string name = HttpContext.User.Claims.Where(x => x.Subject.IsAuthenticated).FirstOrDefault().Value;
            User usr = _context.User.Where(u => u.Name == name).FirstOrDefault();
            var fileModelList = _context.FileModel.Where(fl => fl.UserID == usr.Id).Select(fl => new FileInfoModel { Id = fl.Id, Name = fl.Name, Size = fl.Size, FileExt = fl.FileExt, CompressAlg = fl.CompressAlg}).ToList();

            if (fileModelList == null)
            {
                return NotFound();
            }

            return Ok(fileModelList);
        }
        // POST: /FileModels/Add
        [HttpPost("Add")]
        public async Task<ActionResult<FileModel>> Add()
        {
            var x = HttpContext.Request.Form;
            CompressAlg compressAlg = (CompressAlg)Int32.Parse(HttpContext.Request.Form["CompressAlg"]);
            IFormFile req = HttpContext.Request.Form.Files["First"];
            string name = HttpContext.User.Claims.Where(x => x.Subject.IsAuthenticated).FirstOrDefault().Value;
            User usr = _context.User.Where(u => u.Name == name).FirstOrDefault();
            
            if (_context.FileModel.Any(x => x.Name == req.FileName && x.UserID == usr.Id))
            {
                return BadRequest("File almost exists");
            }
            //req.Data = _archiver.Compress(req.Data);
            byte[] data;
            using (BinaryReader br = new BinaryReader(req.OpenReadStream()))
            {
                data = br.ReadBytes((int)req.Length);
            }

            _context.FileModel.Add(new FileModel() { Data = data, Name = req.FileName, User = usr, CompressAlg = compressAlg });
            _context.SaveChanges();

            FileModel newfl = _context.FileModel.Where(x => x.UserID == usr.Id && x.Name == req.FileName).OrderByDescending(x => x.Id).First(); //Id создается в базе данных, поэтому надо сходить

            return Ok(newfl);
        }

        // DELETE: /FileModels/Delete
        [HttpPost("Delete")]
        public async Task<IActionResult> DeleteFileModel(DeleteReq req)
        {
            FileModel? fileModel = await _context.FileModel.FindAsync(req.Id);
            if (fileModel == null)
            {
                return NotFound("false");
            }

            string name = HttpContext.User.Claims.Where(x => x.Subject.IsAuthenticated).FirstOrDefault().Value;
            User usr = _context.User.Where(u => u.Name == name).FirstOrDefault();
            if (fileModel.UserID != usr.Id)
            {
                return Forbid();
            }

            _context.FileModel.Remove(fileModel);
            _context.SaveChanges();

            return Ok("true");
        }

        [HttpPost("Download")]
        public async Task<ActionResult<Stream>> Download(DownloadReq req)
        {
            string name = HttpContext.User.Claims.Where(x => x.Subject.IsAuthenticated).FirstOrDefault().Value;
            User usr = _context.User.Where(u => u.Name == name).FirstOrDefault();
            int? flUserId = _context.FileModel.Where(fl => fl.Id == req.Id).Select(x => x.UserID).FirstOrDefault();
            if (flUserId == null)
            {  
                return NotFound("false"); 
            }
            if (usr.Id != flUserId)
            {
                return Forbid();
            }
            return new MemoryStream(_context.FileModel.Where(fl => fl.Id == req.Id && fl.UserID == usr.Id).Select(fl => fl.Data/*_archiver.Decompress(fl.Data)*/).FirstOrDefault());
            
        }

        private bool FileModelExists(int id)
        {
            return _context.FileModel.Any(e => e.Id == id);
        }

        public class DeleteReq
        {
            public int Id { get; set; }
        }
        public class DownloadReq
        {
            public int Id { get; set; }
        }
        public class AddReq
        {
            public string Name { get; set; }
            public Stream Data {  get; set; }
        }
    }
}
