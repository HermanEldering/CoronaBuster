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
    public partial class TestDetailPage: ContentPage {
        TestDetailViewModel viewModel;

        public TestDetailPage(TestDetailViewModel viewModel) {
            InitializeComponent();

            BindingContext = this.viewModel = viewModel;
        }

        public TestDetailPage() {
            InitializeComponent();

            var item = new Hit(
                    new PublicRecord(
                        new ForeignRecord(1, "1234", 1981, -50, 5, TimeSpan.FromDays(90)), TimeSpan.FromDays(120)
                        ),
                    new Services.LocalData.LocalKey(1, new byte[] { 5, 6, 7, 8 }, TimeSpan.FromDays(90))
                );

            viewModel = new TestDetailViewModel(item);
            BindingContext = viewModel;
        }
    }
}