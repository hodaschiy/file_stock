using NLog;
using System;
using System.IO;
using System.IO.Compression;

namespace Client
{
    public interface IArchiverService
    {
        byte[] Compress(Stream data, CompressAlg ca);
        byte[] Decompress(Stream data, CompressAlg ca);
    }
    
    public class ArchiverService : IArchiverService
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();
        public byte[] Compress(Stream data, CompressAlg ca)
        {
            byte[] output;
            MemoryStream outstream = new MemoryStream();
            DateTime dateTime = DateTime.Now;
            switch (ca)
            {
                case CompressAlg.GZip:
                    using (GZipStream gZipStream = new GZipStream(outstream, CompressionLevel.Optimal))
                        data.CopyTo(gZipStream); 
                    output = outstream.ToArray();
                    break;
                case CompressAlg.Deflate:
                    using (DeflateStream deflateStream = new DeflateStream(outstream, CompressionLevel.Optimal))
                        data.CopyTo(deflateStream);
                    output = outstream.ToArray();
                    break;
                /*case CompressAlg.Brotli:
                    var brotliStream = new BrotliStream(data, CompressionLevel.Optimal);
                    brotliStream.CopyTo(input);
                    output = input.ToArray();
                    break;
                case CompressAlg.ZLib:
                    var zLibStream = new ZLibStream(data, CompressionLevel.Optimal);
                    zLibStream.CopyTo(input);
                    output = input.ToArray();
                    break;*/
                default:
                    data.CopyTo(outstream);
                    output = outstream.ToArray();
                    break;
            }
            _logger.Info("Время сжатия: " + (dateTime - DateTime.Now).TotalSeconds.ToString());
            return output;
        }

        public byte[] Decompress(Stream data, CompressAlg ca)
        {
            MemoryStream outstream = new MemoryStream();
            byte[] output;
            DateTime dateTime = DateTime.Now;
            switch (ca)
            {
                case CompressAlg.GZip:
                    using (GZipStream gZipStream = new GZipStream(data, CompressionMode.Decompress))
                        gZipStream.CopyTo(outstream);
                    output = outstream.ToArray();
                    break;
                case CompressAlg.Deflate:
                    using (DeflateStream deflateStream = new DeflateStream(data, CompressionMode.Decompress))
                        deflateStream.CopyTo(outstream);
                    output = outstream.ToArray();
                    break;
                /*case CompressAlg.Brotli:
                    output = new byte[1024];
                    break;
                case CompressAlg.ZLib:
                    output = new byte[1024];
                    break;*/
                default:
                    data.CopyTo(outstream);
                    output = outstream.ToArray();
                    break;
            }
            _logger.Info("Время распаковки: " + (dateTime - DateTime.Now).TotalSeconds.ToString());
            return output;
        }
    }

    public enum CompressAlg
    {
        None = 0,
        GZip = 1,
        Deflate = 2,
        Brotli = 3,
        ZLib = 4
    }
}
