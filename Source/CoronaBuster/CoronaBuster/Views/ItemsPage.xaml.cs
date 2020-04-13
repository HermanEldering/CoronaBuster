using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using CoronaBuster.Models;
using CoronaBuster.Views;
using CoronaBuster.ViewModels;

namespace CoronaBuster.Views {
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class ItemsPage: ContentPage {
        ItemsViewModel viewModel;

        public ItemsPage() {
            InitializeComponent();

            BindingContext = viewModel = new ItemsViewModel();
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args) {
            var item = args.SelectedItem as ItemDetailViewModel;
            if (item == null)
                return;

            await Navigation.PushAsync(new ItemDetailPage(item));

            //item.Read = true;

            // Manually deselect item.
            ItemsListView.SelectedItem = null;
        }


        protected override void OnAppearing() {
            base.OnAppearing();

            if (viewModel.Items == null || viewModel.Items.Count == 0)
                viewModel.LoadItemsCommand.Execute(null);
        }
    }
}