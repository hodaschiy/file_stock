using NLog;
using System;
using System.IO;
using System.IO.Compression;

namespace Client
{
    public interface IArchiverService
    {
        MemoryStream Compress(Stream data, CompressAlg ca);
        MemoryStream Decompress(Stream data, CompressAlg ca);
    }
    
    public class ArchiverService : IArchiverService
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();
        public MemoryStream Compress(Stream data, CompressAlg ca)
        {
            MemoryStream outstream = new MemoryStream();
            DateTime dateTime = DateTime.Now;
            switch (ca)
            {
                case CompressAlg.GZip:
                    using (GZipStream gZipStream = new GZipStream(outstream, CompressionLevel.Optimal, true))
                        data.CopyTo(gZipStream); 
                    break;
                case CompressAlg.Deflate:
                    using (DeflateStream deflateStream = new DeflateStream(outstream, CompressionLevel.Optimal, true))
                        data.CopyTo(deflateStream);
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
                    break;
            }
            _logger.Info("Время сжатия: " + (DateTime.Now - dateTime).TotalMilliseconds.ToString()+" миллисекунд");
            outstream.Seek(0, SeekOrigin.Begin);
            return outstream;
        }

        public MemoryStream Decompress(Stream data, CompressAlg ca)
        {
            MemoryStream outstream = new MemoryStream();
            DateTime dateTime = DateTime.Now;
            switch (ca)
            {
                case CompressAlg.GZip:
                    using (GZipStream gZipStream = new GZipStream(data, CompressionMode.Decompress))
                        gZipStream.CopyTo(outstream);
                    break;
                case CompressAlg.Deflate:
                    using (DeflateStream deflateStream = new DeflateStream(data, CompressionMode.Decompress))
                        deflateStream.CopyTo(outstream);
                    break;
                /*case CompressAlg.Brotli:
                    output = new byte[1024];
                    break;
                case CompressAlg.ZLib:
                    output = new byte[1024];
                    break;*/
                default:
                    data.CopyTo(outstream);
                    break;
            }
            _logger.Info("Время распаковки: " + (DateTime.Now - dateTime).TotalSeconds.ToString()+" миллисекунд");
            outstream.Seek(0, SeekOrigin.Begin);
            return outstream;
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
