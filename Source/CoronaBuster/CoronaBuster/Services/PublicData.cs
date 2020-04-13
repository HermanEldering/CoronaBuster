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

        List<(PublicRecord, LocalData.LocalKey)> _hits = new List<(PublicRecord, LocalData.LocalKey)>();
        HttpClient _client = new HttpClient();

        Dictionary<uint, long> _lastPublicationTime = new Dictionary<uint, long>();

        public event Action<Hit> HitFound;

        //public PublicData(LocalData localData) {
        //    _localData = localData;
        //}

        public IEnumerable<(PublicRecord, LocalData.LocalKey)> FindHits(IEnumerable<PublicRecord> data) {
            return data
                .SelectMany(r => _localData.LocalKeyLookup[r.Id]
                .Where(lk => IsMatch(r, lk))
                .Select(lk => (r, lk)));
        }

        private static bool IsMatch(PublicRecord r, LocalData.LocalKey lk) => Crypto.Hash(Crypto.GetSharedSecret(lk.PrivateKey, Convert.FromBase64String(r.PublicKey)), r.Id) == r.SharedSecret;

        public async Task<int> DownloadAndCheck() {
            var newHits = 0;
            foreach (var id in _localData.LocalKeyLookup.Keys) {
                var publicRecords = await Download(id);
                var hits = FindHits(publicRecords).ToList();
                _hits.AddRange(hits); // each public record should be downloaded only once, so this should not store duplicates
                if (publicRecords.Any()) _lastPublicationTime[id] = publicRecords.Max(r => r.PublicationDate.Ticks); // store time so that we don't need to download the older records again
                newHits += hits.Count;
                hits.ForEach(h => HitFound?.Invoke(new Hit(h.Item1, h.Item2)));
            }

            return newHits;
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
