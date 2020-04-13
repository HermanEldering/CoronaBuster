using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CoronaBuster.Models;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace CoronaBuster.Services {
    class InfectionReporter {
        private ForeignData _foreignData = DependencyService.Get<ForeignData>();
        private HttpClient _client = new HttpClient();

        public async Task<bool> Report() {
            _foreignData.Prune();
            return await Upload();
        }

        public async Task<bool> Upload() {
            var uri = new Uri(string.Format(Constants.UploadUrl, string.Empty));

            var publicRecords = _foreignData.Records.Select(r => new PublicRecordBase(r)).ToArray();

            //TODO: must upload smarter in a real app, can be a lot of data so binary would be better and upload in multiple chunks
            //TODO: upload chunks over longer time with random delays so that chunks cannot be grouped based on publication date
            //TODO: handle retries in case of a problem
            var json = JsonConvert.SerializeObject(publicRecords);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(uri, content);

            return response.IsSuccessStatusCode;
        }

    }
}
