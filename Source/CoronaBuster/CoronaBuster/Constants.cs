using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace CoronaBuster {
    public class Constants {
#if LIVE
        public static readonly string BaseAddress = "https://coronaserver.nl";
        public static readonly string DownloadUrl = BaseAddress + "/public/{0}";
        public static readonly string UploadUrl = BaseAddress + "/api/buster";

        // How often are new local keys generated:
        public static readonly TimeSpan KEY_REGENERATION_INTERVAL = TimeSpan.FromMinutes(5);
        // How often is the server contacted:
        public static readonly TimeSpan LONG_INTERVAL = TimeSpan.FromHours(2);

        // How long are foreign records stored on the device:
        public static readonly TimeSpan FOREIGN_RECORD_MEMORY = TimeSpan.FromDays(14);
        // How long is the delay before foreign records are written the file
        public static readonly TimeSpan FOREIGN_RECORD_PERSIST = KEY_REGENERATION_INTERVAL + TimeSpan.FromSeconds(30);
        // How long are local records stored on the device:
        public static readonly TimeSpan LOCAL_RECORD_MEMORY = FOREIGN_RECORD_MEMORY + TimeSpan.FromDays(1); // bit longer time so there is some time to download
#else
        public static readonly string BaseAddress = "http://10.1.1.196:5000"; //"https://10.1.1.196:5001";
        public static readonly string DownloadUrl = BaseAddress + "/public/{0}";
        public static readonly string UploadUrl = BaseAddress + "/api/buster";

        // How often are new local keys generated:
        public static readonly TimeSpan KEY_REGENERATION_INTERVAL = TimeSpan.FromMinutes(0.5);
        // How often is the server contacted:
        public static readonly TimeSpan LONG_INTERVAL = TimeSpan.FromHours(2);

        // How long are foreign records stored on the device:
        public static readonly TimeSpan FOREIGN_RECORD_MEMORY = TimeSpan.FromDays(14);
        // How long is the delay before foreign records are written the file
        public static readonly TimeSpan FOREIGN_RECORD_PERSIST = KEY_REGENERATION_INTERVAL + TimeSpan.FromSeconds(30);
        // How long are local records stored on the device:
        public static readonly TimeSpan LOCAL_RECORD_MEMORY = FOREIGN_RECORD_MEMORY + TimeSpan.FromDays(1); // bit longer time so there is some time to download
#endif
    }
}
