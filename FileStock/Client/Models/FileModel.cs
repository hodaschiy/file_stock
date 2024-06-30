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
    }
}
