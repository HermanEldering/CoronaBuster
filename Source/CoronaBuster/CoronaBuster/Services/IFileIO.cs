using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CoronaBuster.Services {
    public interface IFileIO {

        Stream OpenWrite(string filename, bool truncate = false);
        Stream OpenRead(string filename);
        Stream OpenAppend(string filename);
        bool FileExists(string filename);
    }
}
