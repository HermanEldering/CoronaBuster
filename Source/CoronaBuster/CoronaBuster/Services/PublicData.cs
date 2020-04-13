using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
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
                .SelectMany(r => _localData.LocalKeyLookup[r.Id]
                .Where(lk => IsMatch(r, lk))
                .Select(lk => (r, lk)));
        }

        private static bool IsMatch(PublicRecord r, LocalRecord lk) => Crypto.Hash(Crypto.GetSharedSecret(lk.PrivateKey, Convert.FromBase64String(r.PublicKey)), r.Id) == r.SharedSecret;

        public async Task<int> DownloadAndCheck() {
            lock (_lock) {
                if (_isBusy) return 0;
                _isBusy = true;
            }

            try {
                var newHits = 0;

                foreach (var id in _localData.GetIds()) {
                    var publicRecords = await Download(id);
                    var contacts = FindContacts(publicRecords).ToList();
                    if (publicRecords.Any()) _lastPublicationTime[id] = publicRecords.Max(r => r.PublicationDate.Ticks); // store time so that we don't need to download the older records again
                    newHits += contacts.Count;
                    contacts.ForEach(h => ContactFound?.Invoke(new Contact(h.publicRecord, h.localRecord)));
                }

                try {
                    using (var file = DependencyService.Get<IFileIO>().OpenWrite(nameof(PublicData))) {
                        ProtoBuf.Serializer.SerializeWithLengthPrefix(file, _lastPublicationTime, ProtoBuf.PrefixStyle.Base128, 0);
                    }
                } catch (Exception err) {
                    // if we can't write to the file just assume we need to download it again next time.
                    // TODO: log exception and inform user
                }

                return newHits;
            } finally {
                _isBusy = false;
            }
        }

        public async Task<IEnumerable<PublicRecord>> Download(uint id) {

            if (!_lastPublicationTime.TryGetValue(id, out var lastKnownPublication)) lastKnownPublication = 0;

            var uri = new Uri(string.Format(Constants.DownloadUrl, id, lastKnownPublication));
            
            var response = await _client.GetAsync(uri);
            if (response.IsSuccessStatusCode) {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IEnumerable<PublicRecord>>(content);
            }

            return Enumerable.Empty<PublicRecord>();
        }

    }
}
