using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoronaBuster.Models;

namespace CoronaBuster.Services {
    public class ForeignData {
        public static readonly TimeSpan MEMORY_SPAN = TimeSpan.FromDays(14);

        public List<ForeignRecord> Records { get; private set; } = new List<ForeignRecord>();
        public Dictionary<(uint, string), ForeignRecord> Lookup { get; private set; } = new Dictionary<(uint, string), ForeignRecord>();

        public ForeignRecord StoreForeignData(uint id, byte[] foreignKey, byte[] localPrivateKey, byte[] localPublicKey, int rssi, int txPower) {
            var lookupKey = (id, Convert.ToBase64String(foreignKey));
            if (Lookup.TryGetValue(lookupKey, out var record)) {
                record.LastTime = Helpers.GetExactTime();
                if (record.UpdatePathLoss(rssi, txPower)) return record;
            } else {
                var sharedSecret = Crypto.GetShortSharedSecret(localPrivateKey, foreignKey, id);
                record = new ForeignRecord(id, Convert.ToBase64String(localPublicKey), sharedSecret, rssi, txPower, Helpers.GetExactTime());
                Records.Add(record);
                Lookup.Add(lookupKey, record);
                return record;
            }
            return null;
        }

        public void Prune() {
            // find first relevant key
            var cutoff = Helpers.EPOCH - (DateTime.UtcNow - MEMORY_SPAN);
            int i = 0;
            for (; i < Records.Count; i++) {
                if (Records[i].ScanTime > cutoff) break;
            }

            if (i != 0) {
                // remove keys by replacing entire list, since removing many items from original list will need many copies
                var newList = new List<ForeignRecord>(Records.Count - i);
                newList.AddRange(Records.Skip(i));
                Records = newList;
            }

            // also remove the old items from the lookup
            Lookup.AsQueryable().Where(kv => kv.Value.ScanTime < cutoff).ToList().ForEach(kv => Lookup.Remove(kv.Key));
        }

    }
}
