using System;
using System.Collections.Generic;
using System.Text;

namespace CoronaBuster.Models {
    public class PublicRecordBase {
        public uint Id { get; set; }
        public string PublicKey { get; set; }
        public uint SharedSecret { get; set; }
        public int Rssi { get; set; }
        public int TxPower { get; set; }
        public int MinimumPathLoss { get; set; }
        public int DurationSeconds { get; set; }

        public PublicRecordBase() { }

        protected PublicRecordBase(uint id, string publicKey, uint sharedSecret, int rssi, int txPower, int duration) {
            Id = id;
            PublicKey = publicKey ?? throw new ArgumentNullException(nameof(publicKey));
            SharedSecret = sharedSecret;
            Rssi = rssi;
            TxPower = txPower;
            MinimumPathLoss = txPower - rssi;
            DurationSeconds = duration;
        }

        public PublicRecordBase(PublicRecordBase r) : this(r.Id, r.PublicKey, r.SharedSecret, r.Rssi, r.TxPower, r.DurationSeconds) { }
    }
}
