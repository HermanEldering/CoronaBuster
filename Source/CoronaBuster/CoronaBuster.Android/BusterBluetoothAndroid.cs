using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace CoronaBuster.Droid {
    class BusterBluetoothAndroid: CoronaBuster.Services.IBusterBluetooth {
        const byte VERSION_NUMBER = 1;

        public event Action<byte[], uint, int, int> KeyReceived;
        public event Action<byte[], uint> Advertising;

        private BluetoothImpl.Callbacks _advertiseCallbacks;
        private BluetoothImpl.MyScanCallback _scanCallbacks;

        public void Advertise(byte[] key, uint id) {
            //key = Enumerable.Range(10, BluetoothImpl.KEY_LENGTH).Select(i => (byte)i).ToArray();
            //id = 0x24681357;

            if (_advertiseCallbacks != null) {
                BluetoothImpl.StopAdvertising(_advertiseCallbacks);
            }

            if (key == null) {
                _advertiseCallbacks = null;
                return;
            }

            _advertiseCallbacks = new BluetoothImpl.Callbacks(
                    (cb, s) => Advertising?.Invoke(key, id),
                    _ => Advertising?.Invoke(null, id)
                );

            BluetoothImpl.Advertise(VERSION_NUMBER, key, id, _advertiseCallbacks);
        }

        public void Scan() {
            if (_scanCallbacks != null) BluetoothImpl.StopScanning(_scanCallbacks);
            _scanCallbacks = BluetoothImpl.Scan(ScanHandler, BetterHandler);
            if (_scanCallbacks == null) throw new Exception("Failed to start bluetooth scan.");
        }

        private void BetterHandler(BluetoothDevice device, int rssi, byte[] packet) {
            var offset = (BluetoothImpl.KEY_LENGTH_1 - 20); // correction for change in key length

            // TODO: make processing of packet more resilient
            if (CheckManufacturerId(packet, 4) 
                && CheckManufacturerId(packet, 32 + offset)
                && (packet[7] & 0x0F) == 1) {

                var key = new byte[BluetoothImpl.KEY_LENGTH];
                Array.Copy(packet, 8, key, 0, BluetoothImpl.KEY_LENGTH_1);
                Array.Copy(packet, 35 + offset, key, BluetoothImpl.KEY_LENGTH_1, BluetoothImpl.KEY_LENGTH_2);

                var id = BitConverter.ToUInt32(packet, 55 + BluetoothImpl.KEY_LENGTH - 40); // 40 is old key length

                var powerIndex = FindCodeInBuffer(packet, 0x0A);
                var txPower = (sbyte)packet[powerIndex.index];
                KeyReceived?.Invoke(key, id, rssi, txPower);
            }
        }

        private static (int index, int len) FindCodeInBuffer(byte[] buffer, byte code) {
            int length = buffer.Length;
            int i = 0;
            while (i < length - 2) {
                int len = buffer[i];
                if (len < 0) {
                    return (-1, -1);
                }

                if (i + len >= length) {
                    return (-1, -1);
                }

                byte tcode = buffer[i + 1];
                if (tcode == code) {
                    return (i + 2, len);
                }

                i += len + 1;
            }

            return (-1, -1);
        }

        private static bool CheckManufacturerId(byte[] packet, int index) => 
            packet[index] == 0xFF 
            && packet[index + 1] == (BluetoothImpl.MANUFACTURER_ID >> 8)
            && packet[index + 2] == (BluetoothImpl.MANUFACTURER_ID & 0xFF);

        private void ScanHandler(ScanResult result, ScanFailure failureCode) {
            // obsolete
        }
    }
}