using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CoronaBuster.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace CoronaBuster.ViewModels {
    public class AboutViewModel: BaseViewModel {
        public Buster Buster => DependencyService.Get<Buster>();
        private InfectionReporter Uploader => DependencyService.Get<InfectionReporter>();
        private PublicData PublicData => DependencyService.Get<PublicData>();

        string _uploadStatus;
        public string UploadStatus {
            get => _uploadStatus;
            set => SetProperty(ref _uploadStatus, value);
        }

        string _downloadStatus;
        public string DownloadStatus {
            get => _downloadStatus;
            set => SetProperty(ref _downloadStatus, value);
        }

        bool _isDownloading;
        public bool IsDownloading {
            get => _isDownloading;
            set => SetProperty(ref _isDownloading, value);
        }

        public AboutViewModel() {
            Title = "Status";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://xamarin.com"));

            AdvertiseCommand = new Command(Buster.Advertise);
            ScanCommand = new Command(Buster.Scan);
            UploadCommand = new Command(async () => UploadStatus = await Uploader.Report() ? $"Upload success @{DateTime.Now.TimeOfDay}" : $"Upload failed @{DateTime.Now.TimeOfDay}");
            DownloadCommand = new Command(Download, () => !IsDownloading); 
        }

        private async void Download() {
            try {
                IsDownloading = true;
                DownloadStatus = "Downloading...";
                var result = await PublicData.DownloadAndCheck();
                DownloadStatus = result != 0 ? $"Found {result} new hits @{DateTime.Now.TimeOfDay}" : $"No new hits found @{DateTime.Now.TimeOfDay}";
            } catch (Exception err) {
                DownloadStatus = $"Error while downloading: {err}";
            } finally {
                IsDownloading = false;
            }
        }

        public ICommand OpenWebCommand { get; }

        public ICommand AdvertiseCommand { get; }
        public ICommand ScanCommand { get; }
        public ICommand UploadCommand { get; }
        public ICommand DownloadCommand { get; }
    }
}