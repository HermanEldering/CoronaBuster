using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CoronaBuster.Services {
    class GenericFileIO: IFileIO {
        private string GetFullPath(string filename) => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filename);
        public Stream OpenAppend(string filename) => new FileStream(GetFullPath(filename), FileMode.Append | FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
        public Stream OpenRead(string filename) => new FileStream(GetFullPath(filename), FileMode.Open, FileAccess.Read, FileShare.Read);
        public Stream OpenWrite(string filename, bool truncate = false) => new FileStream(GetFullPath(filename), (truncate ? FileMode.Truncate : 0) | FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        public bool FileExists(string filename) => File.Exists(GetFullPath(filename));
    }
}
