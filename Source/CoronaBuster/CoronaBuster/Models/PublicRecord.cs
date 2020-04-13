using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using ProtoBuf;

namespace CoronaBuster.Models {
    [ProtoContract(SkipConstructor = true)]
    public class PublicRecord: PublicRecordBase {
        [JsonIgnore] [ProtoBuf.ProtoIgnore] [System.Text.Json.Serialization.JsonIgnore] public TimeSpan PublicationDate { get; set; }
        [ProtoMember(50)] public long PublicationTicks { get => PublicationDate.Ticks; set => PublicationDate = TimeSpan.FromTicks(value); }

        public PublicRecord() {

        }

        public PublicRecord(PublicRecordBase r, TimeSpan publication): base(r) {
            PublicationDate = publication;
        }

        public override string ToString() => $"PR-{Id}-{PublicationDate:g}-{SharedSecret}-{PublicKey}";
    }

}
