using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms;

namespace CoronaBuster.Services {
    public class ContactData {
        public ObservableCollection<Models.Contact> Hits { get; private set; } = new ObservableCollection<Models.Contact>();

        private PublicData _publicData = DependencyService.Get<PublicData>();

        public ContactData() {
            _publicData.HitFound += HitFound;
        }

        private void HitFound(Models.Contact hit) {
            Hits.Add(hit);
        }
    }
}
