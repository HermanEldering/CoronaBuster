using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CoronaBuster.Models;

namespace CoronaBuster.Services {
    public class ForeignData {
        public static readonly TimeSpan MEMORY_SPAN = TimeSpan.FromDays(14);
        public static readonly TimeSpan PERSIST_SPAN = Buster.KEY_REGENERATION_INTERVAL + TimeSpan.FromSeconds(30);

        public List<ForeignRecord> Records { get; private set; } = new List<ForeignRecord>();
        public Dictionary<(uint, string), ForeignRecord> Lookup { get; private set; } = new Dictionary<(uint, string), ForeignRecord>();

        private Stream _file;
        private static readonly object _lock = new object();

        public ForeignData() {
            _file = new FileStream(nameof(ForeignData), FileMode.Append, FileAccess.Write, FileShare.Read);

            // don't need to read from the file here
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

                var keepers = ReadAllValidForeignRecords().ToList();
                
                _file = new FileStream(nameof(ForeignData), FileMode.Open | FileMode.Truncate, FileAccess.Write, FileShare.Read);

                foreach (var r in keepers) {
                    ProtoBuf.Serializer.SerializeWithLengthPrefix(_file, r, ProtoBuf.PrefixStyle.Base128, 0);
                }
            }
        }

        public static IEnumerable<ForeignRecord> ReadAllValidForeignRecords() {
            lock (_lock) {
                var readFile = new FileStream(nameof(ForeignData), FileMode.Open, FileAccess.Read, FileShare.Read);

                var now = Helpers.GetExactTime();
                var foreignRecords = ProtoBuf.Serializer.DeserializeItems<ForeignRecord>(readFile, ProtoBuf.PrefixStyle.Base128, 0)
                                        .Where(r => (now - r.ScanTime) < ForeignData.MEMORY_SPAN);
                return foreignRecords;
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
