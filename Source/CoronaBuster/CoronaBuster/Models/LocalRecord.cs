using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace CoronaBuster.Models {

    [ProtoContract(SkipConstructor =true)]
    public class LocalRecord {
        [ProtoMember(1)] public uint Id { get; private set; }
        [ProtoMember(2)] public byte[] PrivateKey { get; private set; }
        [ProtoMember(3)] public TimeSpan Time { get; private set; }
        [ProtoIgnore] public TimeSpan TimeOfDay => Time - TimeSpan.FromDays((int)Time.TotalDays);


        public LocalRecord(uint id, byte[] privateKey, TimeSpan time) {
            Id = id;
            PrivateKey = privateKey ?? throw new ArgumentNullException(nameof(privateKey));
            Time = time;
        }

        public override string ToString() => $"LK-{Id}-{Convert.ToBase64String(PrivateKey)}";
    }
}
