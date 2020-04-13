using System;
using System.Collections.Generic;
using System.Text;
using Org.BouncyCastle.Security;
using Xamarin.Forms;

namespace CoronaBuster.Services {
    public class Buster: ViewModels.BaseViewModel {
        public static SecureRandom Random { get; } = new SecureRandom();

        private string _advertisedKey;
        public string AdvertisedKey {
            get => _advertisedKey;
            private set => SetProperty(ref _advertisedKey, value);
        }

        string _lastScannedKey;
        public string LastScannedKey {
            get => _lastScannedKey;
            set => SetProperty(ref _lastScannedKey, value);
        }

        private bool _isScanning = false;
        public bool IsScanning {
            get => _isScanning;
            set => SetProperty(ref _isScanning, value);
        }

        IBusterBluetooth Bluetooth = DependencyService.Get<IBusterBluetooth>();
        LocalData _localData = DependencyService.Get<LocalData>();
        ForeignData _foreignData = DependencyService.Get<ForeignData>();

        (byte[] publicKey, byte[] privateKey) _pair;

        public Buster() {
            Bluetooth.KeyReceived += KeyReceived;
            Bluetooth.Advertising += StartedAdvertising;

            TimerTick();

            Device.StartTimer(TimeSpan.FromMinutes(2), TimerTick);
        }

        private bool TimerTick() {
            try {
                _pair = Crypto.GenerateKeyPair();

                //if (!IsScanning) 
                Scan();
                Advertise();
            } catch (Exception err) {
                //TODO: show/log exception
            }
            return true;
        }

        private void StartedAdvertising(byte[] publicKey, uint id) => AdvertisedKey = $"@{DateTime.Now}: {Convert.ToBase64String(publicKey)}";

        public void Scan() {
            IsScanning = false;
            try {
                Bluetooth.Scan();
                IsScanning = true;
            } catch (Exception err) {
                // TODO: show/log error
            }
        }

        private void KeyReceived(byte[] foreignKey, uint id, int rssi, int txPower) {
            try {
                // TODO: rate limit new keys to prevent DoS attack
                var record = _foreignData.StoreForeignData(id, foreignKey, _pair.privateKey, _pair.publicKey, rssi, txPower);
                if (record != null) {
                    LastScannedKey = $"@{DateTime.Now}: {record.MinimumPathLoss} - {Convert.ToBase64String(foreignKey)}";
                }
            } catch (Exception err) {
                // TODO: show/log error but catch it because otherwise the app could crash on invalid public key
                LastScannedKey = $"!!!{DateTime.Now}: {Convert.ToBase64String(foreignKey)}";
            }
        }

        public void Advertise() {
            var pair = Crypto.GenerateKeyPair();
            //TODO: generate id's in a way that they can easily be clustered
            var id = (uint)Random.Next(100000); // random number, but not too large because we prefer collisions

            _localData.StoreLocalKey(id, pair.privateKey);
            Bluetooth.Advertise(pair.publicKey, id);
        }
    }
}
