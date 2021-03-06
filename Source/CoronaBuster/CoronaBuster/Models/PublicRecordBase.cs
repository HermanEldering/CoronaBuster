﻿using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace CoronaBuster.Models {
    [ProtoContract(SkipConstructor = true)]
    [ProtoInclude(200, typeof(PublicRecord))]
    [ProtoInclude(201, typeof(ForeignRecord))]
    public class PublicRecordBase {
        [ProtoMember(1)] public uint Id { get; set; }
        [ProtoMember(2)] public string PublicKey { get; set; }    //TODO: change back to bytes for smaller protobuf messages
        [ProtoMember(3)] public uint SharedSecret { get; set; }
        [ProtoMember(4)] public int Rssi { get; set; }            // TODO: change to sbyte/short
        [ProtoMember(5)] public int TxPower { get; set; }         // TODO: change to sbyte
        [ProtoMember(6)] public int MinimumPathLoss { get; set; } // TODO: change to short
        [ProtoMember(7)] public int DurationSeconds { get; set; } // TODO: change to short

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
