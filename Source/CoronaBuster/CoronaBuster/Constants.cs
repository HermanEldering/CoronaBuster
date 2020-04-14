using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace CoronaBuster {
    public class Constants {
        //TODO: for production use https with valid certificate
        public static readonly string BaseAddress = "http://10.1.1.196:5000"; //"https://10.1.1.196:5001";
        public static readonly string DownloadUrl = BaseAddress + "/public/{0}";
        public static readonly string UploadUrl = BaseAddress + "/api/buster";
    }
}
