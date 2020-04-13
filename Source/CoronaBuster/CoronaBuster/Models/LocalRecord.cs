using System;
using System.Collections.Generic;
using System.Text;

namespace CoronaBuster.Models {

    public class LocalRecord {
        public uint Id { get; private set; }
        public byte[] PrivateKey { get; private set; }
        public TimeSpan Time { get; private set; }
        public TimeSpan TimeOfDay => Time - TimeSpan.FromDays((int)Time.TotalDays);

        public LocalRecord(uint id, byte[] privateKey, TimeSpan time) {
            Id = id;
            PrivateKey = privateKey ?? throw new ArgumentNullException(nameof(privateKey));
            Time = time;
        }

        public override string ToString() => $"LK-{Id}-{Convert.ToBase64String(PrivateKey)}";
    }
}
