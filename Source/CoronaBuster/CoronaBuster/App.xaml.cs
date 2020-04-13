using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CoronaBuster.Services;
using CoronaBuster.Views;
using System.Globalization;

namespace CoronaBuster {
    public partial class App: Application {

        public App() {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("nl-nl");// CultureInfo.CurrentUICulture.ToString());

            InitializeComponent();

            DependencyService.Register<Buster>();
            DependencyService.Register<LocalData>();
            DependencyService.Register<ForeignData>();
            DependencyService.Register<PublicData>();
            DependencyService.Register<HitsData>();
            DependencyService.Register<InfectionReporter>();

            // make sure these are initialized when the app is started:
            DependencyService.Get<HitsData>(); 
            DependencyService.Get<Buster>();

            MainPage = new AppShell();
        }

        protected override void OnStart() {
        }

        protected override void OnSleep() {
        }

        protected override void OnResume() {
        }
    }
}
