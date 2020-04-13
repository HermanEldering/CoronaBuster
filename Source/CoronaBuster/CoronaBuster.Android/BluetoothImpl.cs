using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace CoronaBuster.Droid {
    internal class BluetoothImpl {
#warning FIXME: this ID is only for testing purposes
        public const int MANUFACTURER_ID = 0xFFFF;
        public const int KEY_LENGTH = 33;
        public const int KEY_LENGTH_1 = 15;
        public const int KEY_LENGTH_2 = KEY_LENGTH - KEY_LENGTH_1;

        public static void Advertise(byte version, byte[] key, uint id, Callbacks callbacks) {
            var advertiser = BluetoothAdapter.DefaultAdapter.BluetoothLeAdvertiser;
            var settings = new AdvertiseSettings.Builder()
                .SetAdvertiseMode(AdvertiseMode.Balanced)
                .SetConnectable(true)
                .SetTxPowerLevel(AdvertiseTx.PowerMedium)
                .Build();

            var payload1 = new byte[1 + KEY_LENGTH_1];
            payload1[0] = version;
            Array.Copy(key, 0, payload1, 1, KEY_LENGTH_1);

            var payload2 = new byte[KEY_LENGTH_2 + 4];
            Array.Copy(key, KEY_LENGTH_1, payload2, 0, KEY_LENGTH_2);
            var idBytes = BitConverter.GetBytes(id);
            Array.Copy(idBytes, 0, payload2, KEY_LENGTH_2, 4);

            var data1 = new AdvertiseData.Builder()
                .SetIncludeTxPowerLevel(true)
                .AddManufacturerData(MANUFACTURER_ID, payload1)
                .Build();
            var data2 = new AdvertiseData.Builder()
                .AddManufacturerData(MANUFACTURER_ID, payload2)
                .Build();


            advertiser.StartAdvertising(settings, data1, data2, callbacks);
            //advertiser.StartAdvertising(settings.Build(), data1, callbacks);

        }

        public static void StopAdvertising(Callbacks callbacks) {
            var advertiser = BluetoothAdapter.DefaultAdapter.BluetoothLeAdvertiser;
            
            advertiser.StopAdvertising(callbacks);
        }

        public static MyScanCallback Scan(Action<ScanResult, ScanFailure> handler, Action<BluetoothDevice, int, byte[]> better) {
            var scanner = BluetoothAdapter.DefaultAdapter.BluetoothLeScanner;
            //var filter = new ScanFilter.Builder();
            //filter.SetManufacturerData(
            //var settings = new ScanSettings.Builder()
            //    .SetCallbackType(ScanCallbackType.AllMatches)
            //    .SetMatchMode(BluetoothScanMatchMode.Aggressive)
            //    .SetPhy((Android.Bluetooth.BluetoothPhy)255)
            //    .SetNumOfMatches(3)
            //    .SetScanMode(Android.Bluetooth.LE.ScanMode.LowPower)
            //    .SetReportDelay(0)
            //    .SetLegacy(true)
            //    .Build();

            //BluetoothAdapter.DefaultAdapter.StartDiscovery();

            var cb = new MyScanCallback(handler, better);
            //scanner.StartScan(new List<ScanFilter>() { new ScanFilter.Builder().SetManufacturerData(MANUFACTURER_ID, new byte[0]).Build() }, settings, cb);

            //scanner.StartScan(cb);

            if (BluetoothAdapter.DefaultAdapter.StartLeScan(cb)) return cb;

            return null;
        }

        public static void StopScanning(MyScanCallback cb) {
            BluetoothAdapter.DefaultAdapter.StopLeScan(cb);
        }


        public class Callbacks: AdvertiseCallback {
            public Action<AdvertiseFailure> Failure { get; private set; }
            public Action<Callbacks, AdvertiseSettings> Success { get; private set; }

            public Callbacks(Action<Callbacks, AdvertiseSettings> onSuccess, Action<AdvertiseFailure> onFailure) {
                Success = onSuccess;
                Failure = onFailure;
            }

            public override void OnStartFailure([GeneratedEnum] AdvertiseFailure errorCode) => Failure(errorCode);
            public override void OnStartSuccess(AdvertiseSettings settingsInEffect) => Success(this, settingsInEffect);
        }

        public class MyScanCallback: ScanCallback, BluetoothAdapter.ILeScanCallback {
            public Action<ScanResult, ScanFailure> Handler { get; private set; }
            public Action<BluetoothDevice, int, byte[]> BetterHandler { get; private set; }

            public MyScanCallback(Action<ScanResult, ScanFailure> handler, Action<BluetoothDevice, int, byte[]> better) {
                Handler = handler;
                BetterHandler = better;
            }

            public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result) => Handler(result, 0);
            public override void OnBatchScanResults(IList<ScanResult> results) {
                foreach (var item in results) {
                    Handler(item, 0);
                }
            }

            public override void OnScanFailed([GeneratedEnum] ScanFailure errorCode) => Handler(null, errorCode);
            public void OnLeScan(BluetoothDevice device, int rssi, byte[] scanRecord) => BetterHandler(device, rssi, scanRecord);
        }
    }
}