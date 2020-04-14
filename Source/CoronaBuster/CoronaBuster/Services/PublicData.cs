using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CoronaBuster.Models;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace CoronaBuster.Services {
    public class PublicData {
        LocalData _localData = DependencyService.Get<LocalData>();

        HttpClient _client = new HttpClient();

        Dictionary<uint, long> _lastPublicationTime = new Dictionary<uint, long>();

        public event Action<Contact> ContactFound;

        private bool _isBusy = false;
        private readonly object _lock = new object();

        public PublicData() {
            try {
                var fileIO = DependencyService.Get<IFileIO>();
                if (fileIO.FileExists(nameof(PublicData))) {
                    using (var file = fileIO.OpenRead(nameof(PublicData))) {
                        _lastPublicationTime = ProtoBuf.Serializer.DeserializeWithLengthPrefix<Dictionary<uint, long>>(file, ProtoBuf.PrefixStyle.Base128, 0);
                    }
                }
            } catch (Exception err) {
                // if we can't read from the file just assume we need to download everthing.
                // TODO: log exception just to be sure
            }
        }

        public IEnumerable<(PublicRecord publicRecord, LocalRecord localRecord)> FindContacts(IEnumerable<PublicRecord> data) {
            return data
                .SelectMany(r => 
                    (_localData.LocalKeyLookup.TryGetValue(r.Id, out var value) ? value : Enumerable.Empty<LocalRecord>())
                        .Where(lk => IsMatch(r, lk))
                        .Select(lk => (r, lk)
                        )
                    );
        }

        private static bool IsMatch(PublicRecord r, LocalRecord lk) => Crypto.Hash(Crypto.GetSharedSecret(lk.PrivateKey, Convert.FromBase64String(r.PublicKey)), r.Id) == r.SharedSecret;

        public async Task<int> DownloadAndCheck() {
            lock (_lock) {
                if (_isBusy) return 0;
                _isBusy = true;
            }

            try {
                var newContacts = 0;

                foreach (var id in _localData.GetIds()) {
                    var (publicRecords, newOffset) = await Download(id);
                    var contacts = FindContacts(publicRecords).ToList();
                    newContacts += contacts.Count;
                    contacts.ForEach(h => ContactFound?.Invoke(new Contact(h.publicRecord, h.localRecord)));
                    _lastPublicationTime[id] = newOffset;
                }

                try {
                    using (var file = DependencyService.Get<IFileIO>().OpenWrite(nameof(PublicData))) {
                        ProtoBuf.Serializer.SerializeWithLengthPrefix(file, _lastPublicationTime, ProtoBuf.PrefixStyle.Base128, 0);
                    }
                } catch (Exception err) {
                    // if we can't write to the file just assume we need to download it again next time.
                    // TODO: log exception and inform user
                }

                return newContacts;
            } finally {
                _isBusy = false;
            }
        }

        public async Task<(IEnumerable<PublicRecord> records, long newOffset)> Download(uint id) {

            if (!_lastPublicationTime.TryGetValue(id, out var lastKnownPublication)) lastKnownPublication = 0;

            var uri = new Uri(string.Format(Constants.DownloadUrl, id, lastKnownPublication));

            var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip });
            var request = new HttpRequestMessage { RequestUri = uri };
            request.Headers.Range = new RangeHeaderValue(lastKnownPublication, null);

            Console.WriteLine(request.Headers.Range.Ranges);
            var response = await client.SendAsync(request);

            //var response = await _client.GetAsync(uri);
            if (response.IsSuccessStatusCode) {
                var content = await response.Content.ReadAsStreamAsync();
                // TODO: check if header has To field
                var contentRange = response.Content.Headers.ContentRange;
                var end = contentRange.To;
                if (contentRange.From != contentRange.To) return (ProtoBuf.Serializer.DeserializeItems<PublicRecord>(content, ProtoBuf.PrefixStyle.Base128, 0), end.Value + 1);
            }

            return (Enumerable.Empty<PublicRecord>(), lastKnownPublication);
        }

    }
}
