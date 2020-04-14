using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CoronaBuster.Models;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace CoronaBuster.Services {
    class InfectionReporter {
        private HttpClient _client = new HttpClient();

        public async Task<bool> Report() {
            // TODO: this has issue with locking
            using (var _ = ForeignData.ReadAllValidForeignRecords(out var foreignRecords)) {
                using (var memoryStream = new MemoryStream()) {
                    foreignRecords
                            .Select(r => new PublicRecordBase(r))
                            .ForEach(r => ProtoBuf.Serializer.SerializeWithLengthPrefix(memoryStream, r, ProtoBuf.PrefixStyle.Base128, 0));

                    memoryStream.Position = 0;
                    return await Upload(memoryStream);
                }
            }
        }

        private async Task<bool> Upload(MemoryStream stream) {
            try {
                var uri = new Uri(string.Format(Constants.UploadUrl, string.Empty));

                //TODO: handle retries in case of a problem
                var content = new StreamContent(stream);
                var response = await _client.PostAsync(uri, content);

                return response.IsSuccessStatusCode;
            } catch (Exception err) {
                // TODO: log exception
                return false;
            }
        }

    }
}
