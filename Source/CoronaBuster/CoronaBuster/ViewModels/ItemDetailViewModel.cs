using System;
using System.Collections.Generic;
using CoronaBuster.Models;

namespace CoronaBuster.ViewModels {
    public class ItemDetailViewModel: BaseViewModel {
        public DateTime ReportDate { get; set; }
        public TimeSpan Duration { get; set; }
        public double RiskFactor { get; set; }
        public List<DateTime> Dates { get; set; }

        public ItemDetailViewModel() {
            Title = "Contact details";
        }
    }
}
