using System;
using System.Collections.Generic;
using System.Text;

namespace CoronaBuster.Services {
    public interface IBusterBluetooth {
        event Action<byte[], uint, int, int> KeyReceived;
        event Action<byte[], uint> Advertising;

        void Advertise(byte[] key, uint id);
        void Scan();
    }
}
