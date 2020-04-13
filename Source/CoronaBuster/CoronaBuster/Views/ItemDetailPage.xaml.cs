using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using CoronaBuster.Models;
using CoronaBuster.ViewModels;

namespace CoronaBuster.Views {
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class ItemDetailPage: ContentPage {
        ItemDetailViewModel viewModel;

        public ItemDetailPage(ItemDetailViewModel viewModel) {
            InitializeComponent();

            BindingContext = this.viewModel = viewModel;
        }

        public ItemDetailPage() {
            InitializeComponent();


            viewModel = new ItemDetailViewModel() {
                Duration = TimeSpan.FromMinutes(1.2),
                RiskFactor = 1.9,
                //Dates = { TimeSpan.FromHours(12.5), TimeSpan.FromHours(13.1), TimeSpan.FromHours(16.82) }
            };
            BindingContext = viewModel;
        }
    }
}