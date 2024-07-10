using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class FileModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = String.Empty;
        public string Size { get; set; }
        public string FileExt { get; set; }
        public CompressAlg CompressAlg { get; set; }
        public Nullable<int> OriginalFileId { get; set; }
        public string Holder {  get; set; }



        public static FileModel Empty = new FileModel() { Id = 0, FileExt = null, Name = String.Empty, Size = null };
        public bool IsEmpty()
        {
            return Id == Empty.Id && Name == Empty.Name && Size == Empty.Size && FileExt == Empty.FileExt;
        }
    }
}
