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
    public partial class TestPage: ContentPage {
        TestViewModel viewModel;

        public TestPage() {
            InitializeComponent();

            BindingContext = viewModel = new TestViewModel();
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args) {
            var item = args.SelectedItem as Contact;
            if (item == null)
                return;

            await Navigation.PushAsync(new TestDetailPage(new TestDetailViewModel(item)));

            item.Read = true;

            // Manually deselect item.
            ItemsListView.SelectedItem = null;
        }

        //async void AddItem_Clicked(object sender, EventArgs e) {
        //    await Navigation.PushModalAsync(new NavigationPage(new NewItemPage()));
        //}

        protected override void OnAppearing() {
            base.OnAppearing();

            if (viewModel.Items == null || viewModel.Items.Count == 0)
                viewModel.LoadItemsCommand.Execute(null);
        }
    }
}