using System;

using CoronaBuster.Models;

namespace CoronaBuster.ViewModels {
    public class TestDetailViewModel: BaseViewModel {
        public Contact Item { get; set; }
        public TestDetailViewModel(Contact item = null) {
            Title = "Connection"; // item?.Text;
            Item = item;
        }
    }
}
