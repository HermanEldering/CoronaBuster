using System;

using CoronaBuster.Models;

namespace CoronaBuster.ViewModels {
    public class TestDetailViewModel: BaseViewModel {
        public Hit Item { get; set; }
        public TestDetailViewModel(Hit item = null) {
            Title = "Connection"; // item?.Text;
            Item = item;
        }
    }
}
