using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace CoronaBuster.Models {

    [ProtoContract(SkipConstructor = true)]
    public class ForeignRecord: PublicRecordBase {
        [ProtoMember(100)] public TimeSpan ScanTime { get; private set; }
        [ProtoIgnore] public TimeSpan LastTime {
            get => ScanTime + TimeSpan.FromSeconds(DurationSeconds);
            set => DurationSeconds = (int)(value - ScanTime).TotalSeconds;
        }

        public ForeignRecord(uint id, string publicKey, uint sharedSecret, int rssi, int txPower, TimeSpan time) : base(id, publicKey, sharedSecret, rssi, txPower, 0) {

            ScanTime = time;
        }

        public override bool Equals(object obj) => (obj is ForeignRecord r) && Id == r.Id && SharedSecret == r.SharedSecret && PublicKey == r.PublicKey;
        public override int GetHashCode() => Id.GetHashCode() + SharedSecret.GetHashCode() * 13 + PublicKey.GetHashCode() * 27;

        internal bool UpdatePathLoss(int rssi, int txPower) {
            var currentPathLoss = txPower - rssi;
            if (currentPathLoss < MinimumPathLoss) {
                MinimumPathLoss = currentPathLoss;
                return true;
            }
            return false;
        }
    }
}
