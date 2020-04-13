using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoronaBuster.Models;

namespace CoronaBuster.Services {
    public class ForeignData {
        public static readonly TimeSpan MEMORY_SPAN = TimeSpan.FromDays(14);
        public static readonly TimeSpan PERSIST_SPAN = Buster.KEY_REGENERATION_INTERVAL + TimeSpan.FromSeconds(30);

        public List<ForeignRecord> Records { get; private set; } = new List<ForeignRecord>();
        public Dictionary<(uint, string), ForeignRecord> Lookup { get; private set; } = new Dictionary<(uint, string), ForeignRecord>();

        private System.IO.Stream _file;
        private static readonly object _lock = new object();

        public ForeignData() {
            // don't need to read from the file here
            _file = Xamarin.Forms.DependencyService.Get<IFileIO>().OpenAppend(nameof(ForeignData));
        }

        public void PersistData() {
            lock (_lock) {
                var keepers = new List<ForeignRecord>();
                foreach (var r in Records) {
                    if (Helpers.GetExactTime() - r.ScanTime > PERSIST_SPAN) {
                        ProtoBuf.Serializer.SerializeWithLengthPrefix(_file, r, ProtoBuf.PrefixStyle.Base128, 0);
                    } else {
                        keepers.Add(r);
                    }
                }
                _file.Flush();
                Records = keepers;
            }
        }

        public void PrunePersistedData() {
            lock (_lock) {
                _file.Close();

                // TODO: this reads all foreign records into memory. Change it to stream the data.
                List<ForeignRecord> keepersList;
                using (var _ = ReadAllValidForeignRecords(out var keepers)) keepersList = keepers.ToList();

                _file = Xamarin.Forms.DependencyService.Get<IFileIO>().OpenWrite(nameof(ForeignData), true);

                foreach (var r in keepersList) {
                    ProtoBuf.Serializer.SerializeWithLengthPrefix(_file, r, ProtoBuf.PrefixStyle.Base128, 0);
                }
                

                _file.Flush();
            }
        }

        public static IDisposable ReadAllValidForeignRecords(out IEnumerable<ForeignRecord> foreignRecords) {
            lock (_lock) {
                var readFile = Xamarin.Forms.DependencyService.Get<IFileIO>().OpenRead(nameof(ForeignData));

                var now = Helpers.GetExactTime();
                foreignRecords = ProtoBuf.Serializer.DeserializeItems<ForeignRecord>(readFile, ProtoBuf.PrefixStyle.Base128, 0)
                                        .Where(r => (now - r.ScanTime) < ForeignData.MEMORY_SPAN);

                return readFile;
            }
        }

        public ForeignRecord StoreForeignData(uint id, byte[] foreignKey, byte[] localPrivateKey, byte[] localPublicKey, int rssi, int txPower) {
            var lookupKey = (id, Convert.ToBase64String(foreignKey));
            if (Lookup.TryGetValue(lookupKey, out var record)) {
                record.LastTime = Helpers.GetExactTime();
                if (record.UpdatePathLoss(rssi, txPower)) return record;
            } else {
                lock (_lock) {
                    var sharedSecret = Crypto.GetShortSharedSecret(localPrivateKey, foreignKey, id);
                    record = new ForeignRecord(id, Convert.ToBase64String(localPublicKey), sharedSecret, rssi, txPower, Helpers.GetExactTime());
                    Records.Add(record);
                    Lookup.Add(lookupKey, record);
                    return record;
                }
            }
            return null;
        }

    }
}
