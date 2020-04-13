using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms;

namespace CoronaBuster.Services {
    public class HitsData {
        public ObservableCollection<Models.Hit> Hits { get; private set; } = new ObservableCollection<Models.Hit>();

        private PublicData _publicData = DependencyService.Get<PublicData>();

        //public HitsData(PublicData publicData) {
        public HitsData() {
            _publicData.HitFound += HitFound;
        }

        private void HitFound(Models.Hit hit) {
            Hits.Add(hit);
        }
    }
}
