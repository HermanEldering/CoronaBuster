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
        private HttpClient _client = new HttpClient();

        public async Task<bool> Report() {
            // TODO: this has issue with locking
            using (var _ = ForeignData.ReadAllValidForeignRecords(out var foreignRecords)) {

                // TODO: ToArray call loads entire file into memory
                var publicRecords = foreignRecords
                                        .Select(r => new PublicRecordBase(r))
                                        .ToArray();

                return await Upload(publicRecords);
            }
        }

        private async Task<bool> Upload(PublicRecordBase[] publicRecords) {
            var uri = new Uri(string.Format(Constants.UploadUrl, string.Empty));
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
