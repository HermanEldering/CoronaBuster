using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static System.Math;

namespace CoronaBuster.Services {
    public class LocalData {
        public static readonly TimeSpan MEMORY_SPAN = TimeSpan.FromDays(21); // store a bit longer than foreign data in case there is a delay in checking

        public List<LocalKey> LocalKeys { get; private set; } = new List<LocalKey>();
        public Dictionary<uint, List<LocalKey>> LocalKeyLookup { get; private set; } = new Dictionary<uint, List<LocalKey>>();

        public void StoreLocalKey(uint id, byte[] privateKey) {
            // maintain a list sorted by time
            var item = new LocalKey(id, privateKey, Helpers.GetApproximateHour());
            LocalKeys.Add(item);

            // also store a reference in a lookup table
            if (!LocalKeyLookup.TryGetValue(id, out var list)) {
                list = new List<LocalKey>();
                LocalKeyLookup.Add(id, list);
            }
            list.Add(item);
        }

        public void Prune() {
            // find first relevant key
            var cutoff = Helpers.EPOCH - (DateTime.UtcNow - MEMORY_SPAN);
            int i = 0;
            for (; i < LocalKeys.Count; i++) {
                if (LocalKeys[i].Time > cutoff) break;

                // remove old keys from lookup table
                if (LocalKeyLookup.TryGetValue(LocalKeys[i].Id, out var keys)) {
                    keys.Remove(LocalKeys[i]);
                }
            }

            if (i != 0) {
                // remove keys by replacing entire list, since removing many items from original list will need many copies
                var newList = new List<LocalKey>(LocalKeys.Count - i);
                newList.AddRange(LocalKeys.Skip(i));
                LocalKeys = newList;
            }
        }

        public class LocalKey {
            public uint Id { get; private set; }
            public byte[] PrivateKey { get; private set; }
            public TimeSpan Time { get; private set; }
            public TimeSpan TimeOfDay => Time - TimeSpan.FromDays((int)Time.TotalDays);

            public LocalKey(uint id, byte[] privateKey, TimeSpan time) {
                Id = id;
                PrivateKey = privateKey ?? throw new ArgumentNullException(nameof(privateKey));
                Time = time;
            }

            public override string ToString() => $"LK-{Id}-{Convert.ToBase64String(PrivateKey)}";
        }
    }
}
