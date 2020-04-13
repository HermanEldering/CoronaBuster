﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using CoronaBuster.Models;
using CoronaBuster.Views;
using System.Collections.Specialized;
using System.Linq;

namespace CoronaBuster.ViewModels {
    public class TestViewModel: BaseViewModel {
        public ObservableCollection<Hit> Items { get; } = new ObservableCollection<Hit>();
        public Command LoadItemsCommand { get; set; }

        public TestViewModel() {
            Title = "Connections";
            DataStore.Hits.CollectionChanged += Hits_CollectionChanged;
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
                var items = DataStore.Hits.OrderByDescending(x => x.PublicationDate);
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