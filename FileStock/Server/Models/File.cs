using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace Server.Models
{
    public class FileModel
    {
        public static readonly FileModel Empty = new();

        public int Id {  get; set; } 
        public byte[] Data { get; set; } = Enumerable.Empty<byte>().ToArray();
        public string Name { get; set; } = String.Empty;
        [NotMapped]
        public string Size { get => GetSize(); }
        [NotMapped]
        public string FileExt { get => GetExt(); }
        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        private string GetExt()
        {
            if (Name.IndexOf('.') == -1)
                return String.Empty;
            else 
                return Name[Name.IndexOf('.')..];
        }
        private string GetSize() {
            int i = 0;
            float len = Data.Length;
            while (len > 512 & i <= 4)
            {
                i++;
                len = len / 1024;
            }
            switch (i)
            {
                case 0:
                    return String.Format("{0:0.0}", len) + "bytes";
                    break;
                case 1:
                    return String.Format("{0:0.0}", len) + "kb";
                    break;
                case 2:
                    return String.Format("{0:0.0}", len) + "Mb";
                    break;
                case 3:
                    return String.Format("{0:0.0}", len) + "Gb";
                    break;
                case 4:
                    return String.Format("{0:0.0}", len) + "Tb";
                    break;
                default:
                    return Data.Length + "bytes";
                    break;
            }
        }

        public FileModel() { }
    }

    public class FileInfoModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
        public string FileExt { get; set; }
    }
}
