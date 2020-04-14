using System;
using System.Collections.Generic;
using System.Text;
using Org.BouncyCastle.Security;
using Xamarin.Essentials;
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
        public PermissionStatus PermissionStatus { get; private set; }

        IBusterBluetooth Bluetooth = DependencyService.Get<IBusterBluetooth>();
        LocalData _localData = DependencyService.Get<LocalData>();
        ForeignData _foreignData = DependencyService.Get<ForeignData>();

        //(byte[] publicKey, byte[] privateKey) _pair;
        private uint[] _idPool;

        public Buster() {
            _idPool = new uint[20];
            for (int i = 0; i < _idPool.Length; i++) {
                _idPool[i] = (uint)Random.Next(100_000);
            }

            Bluetooth.KeyReceived += KeyReceived;
            Bluetooth.Advertising += StartedAdvertising;

            ShortTick();

            Device.StartTimer(Constants.KEY_REGENERATION_INTERVAL, ShortTick);
            Device.StartTimer(Constants.LONG_INTERVAL, LongTick);
        }

        private bool ShortTick() {
            try {
                _foreignData.PersistData();

                RequestPermissions(); // make sure that we have the permissions to use bluetooth

                Scan();
                Advertise();
            } catch (Exception err) {
                //TODO: show/log exception
            }
            return true;
        }

        private bool LongTick() {
            try {
                _foreignData.PrunePersistedData();
                // TODO: Automatically download
            } catch (Exception err) {
                //TODO: show/log exception
            }
            return true;
        }

        private async void RequestPermissions() {
            PermissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
            if (PermissionStatus != PermissionStatus.Granted) {
                PermissionStatus = await Permissions.RequestAsync<Permissions.LocationAlways>();
                if (PermissionStatus == PermissionStatus.Granted) ShortTick(); // retry immediately
            }
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
                var pair = Crypto.GenerateKeyPair();
                var record = _foreignData.StoreForeignData(id, foreignKey, pair.privateKey, pair.publicKey, rssi, txPower);
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
            var id = _idPool[Random.Next() % _idPool.Length]; // random number, but not too large because we prefer collisions

            _localData.StoreLocalKey(id, pair.privateKey);
            Bluetooth.Advertise(pair.publicKey, id);
        }
    }
}
