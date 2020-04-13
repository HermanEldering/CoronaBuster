using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using CoronaBuster.Models;
using CoronaBuster.Views;
using System.Collections.Specialized;
using System.Linq;

namespace CoronaBuster.ViewModels {
    public class ItemsViewModel: BaseViewModel {
        public ObservableCollection<ItemDetailViewModel> Items { get; } = new ObservableCollection<ItemDetailViewModel>();
        public Command LoadItemsCommand { get; set; }

        public ItemsViewModel() {
            Title = "Infection reports";
            DataStore.Contacts.CollectionChanged += Hits_CollectionChanged;
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            //MessagingCenter.Subscribe<NewItemPage, Item>(this, "AddItem", async (obj, item) => {
            //    var newItem = item as Item;
            //    Items.Add(newItem);
            //    await DataStore.AddItemAsync(newItem);
            //});
        }

        private void Hits_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => ExecuteLoadItemsCommand();

        async Task ExecuteLoadItemsCommand() {
            if (IsBusy)
                return;

            IsBusy = true;

            try {
                Items.Clear();
                var items = DataStore.Contacts.GroupBy(x => x.PublicationDate, x => x, (a, b) => new ItemDetailViewModel {
                    ReportDate = a,
                    Duration = TimeSpan.FromSeconds(b.Sum(x => x.PublicData.DurationSeconds)),
                    RiskFactor = b.Max(x => x.Distance),
                    Dates = b.Select(x => Helpers.ToDateTime(x.LocalData.Time)).Distinct().OrderBy(x => x).ToList(), // TODO: add duration per time bin
                });
                foreach (var item in items) {
                    Items.Add(item);
                }
            } catch (Exception ex) {
                Debug.WriteLine(ex);
            } finally {
                IsBusy = false;
            }
        }
    }
}