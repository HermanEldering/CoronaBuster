﻿using System;
using System.Collections.Generic;
using System.Text;

using static System.Math;

namespace CoronaBuster.Models {
    public class Hit: ModelBase {
        public PublicRecord PublicData { get; private set; }
        public Services.LocalData.LocalKey LocalData { get; private set; }

        bool _read;
        public bool Read { 
            get => _read; 
            set => SetProperty(ref _read, value); 
        }

        public DateTime PublicationDate => Helpers.EPOCH + PublicData.PublicationDate;
        public DateTime MeetingTime => Helpers.EPOCH + LocalData.Time;
        public string Duration => PublicData.DurationSeconds < 60 ? $"{PublicData.DurationSeconds} seconds" : $"{(int)Ceiling(PublicData.DurationSeconds / 60d)} minutes";
        public int PathLoss => PublicData.MinimumPathLoss;
        public float Distance => 100f / PublicData.MinimumPathLoss;

        public Hit(PublicRecord publicRecord, Services.LocalData.LocalKey localRecord) {
            PublicData = publicRecord;
            LocalData = localRecord;
        }
    }
}
