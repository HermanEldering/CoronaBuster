﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoronaBuster.Models;

using static System.Math;

namespace CoronaBuster.Services {
    public class LocalData {
        

        public List<LocalRecord> LocalKeys { get; private set; } = new List<LocalRecord>();
        public Dictionary<uint, List<LocalRecord>> LocalKeyLookup { get; private set; } = new Dictionary<uint, List<LocalRecord>>();

        private System.IO.Stream _file;
        private readonly object _lock = new object();

        public LocalData() {
            _file = Xamarin.Forms.DependencyService.Get<IFileIO>().OpenWrite(nameof(LocalData));

            foreach (var item in ProtoBuf.Serializer.DeserializeItems<LocalRecord>(_file, ProtoBuf.PrefixStyle.Base128, 0)) {
                LocalKeys.Add(item);
                AddToLookup(item);
            }
        }

        public void StoreLocalKey(uint id, byte[] privateKey) {
            
            var item = new LocalRecord(id, privateKey, Helpers.GetApproximateHour());

            lock (_lock) {
                // maintain a list sorted by time
                LocalKeys.Add(item);

                // also store a reference in a lookup table
                AddToLookup(item);
            }

            // and write to permanent storage
            ProtoBuf.Serializer.SerializeWithLengthPrefix(_file, item, ProtoBuf.PrefixStyle.Base128, 0);
            _file.Flush();
        }

        private void AddToLookup(LocalRecord item) {
            if (!LocalKeyLookup.TryGetValue(item.Id, out var list)) {
                list = new List<LocalRecord>();
                LocalKeyLookup.Add(item.Id, list);
            }
            list.Add(item);
        }

        public void Prune() {
            lock (_lock) {
                // find first relevant key
                var cutoff = Helpers.GetExactTime() - Constants.LOCAL_RECORD_MEMORY;
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
                    var newList = new List<LocalRecord>(LocalKeys.Count - i);
                    newList.AddRange(LocalKeys.Skip(i));
                    LocalKeys = newList;
                }
            }
        }

        public List<uint> GetIds() {
            lock (_lock) return LocalKeyLookup.Keys.ToList();
        }
    }
}
