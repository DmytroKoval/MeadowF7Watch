using Meadow;
using Meadow.Foundation;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace F7Watch
{
    public class SDA5708 : ISpiPeripheral, IDisposable
    {
        // CS signal, active low
        private readonly IDigitalOutputPort loadPort;
        private readonly IDigitalOutputPort resetPort;

        protected ISpiCommunications spiComms;

        public SDA5708(ISpiBus spiBus, IDigitalOutputPort loadPort, IDigitalOutputPort resetPort)
        {
            this.loadPort = loadPort;
            this.resetPort = resetPort;

            spiComms = new SpiCommunications(spiBus, loadPort,
                DefaultSpiBusSpeed, DefaultSpiBusMode, writeBufferSize: 16,
                csMode: ChipSelectMode.ActiveLow);

            Initialize();
        }

        public SDA5708(ISpiBus spiBus, IPin loadPin, IPin resetPin)
            : this(spiBus, loadPin.CreateDigitalOutputPort(), resetPin.CreateDigitalOutputPort())
        {
            createdPort = true;
        }

        /// <summary>
        /// The default SPI bus speed for the device
        /// </summary>
        public Frequency DefaultSpiBusSpeed => new(12, Frequency.UnitType.Megahertz);

        /// <summary>
        /// The default SPI bus mode for the device
        /// </summary>
        public SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode0;

        /// <summary>
        /// The SPI bus mode for the device
        /// </summary>
        public SpiClockConfiguration.Mode SpiBusMode
        {
            get => spiComms.BusMode;
            set => spiComms.BusMode = value;
        }

        /// <summary>
        /// The SPI bus speed for the device
        /// </summary>
        public Frequency SpiBusSpeed
        {
            get => spiComms.BusSpeed;
            set => spiComms.BusSpeed = value;
        }

        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Did we create the port(s) used by the peripheral
        /// </summary>
        readonly bool createdPort = false;

        private void Initialize()
        {
            Resolver.Log.Info("Initialize display", "SDA5708");
            Reset();
            loadPort.State = true;
            // control word
            // 0b11a0bccc
            // a - 0: display off, 1: display on
            // b - 0:max display current, 1: 12.5% of max display current
            // ccc - 000: 100% brightness, 001: 53%, 010: 40%, 011: 27%, 100: 20%, 101: 13%, 110: 7%, 111: 0%
            //
            // data transfer is least significant bit first
            spiComms.Write(ReverseByte(0b11100010));
        }

        public void Reset()
        {
            resetPort.State = false;
            Task.Delay(2).Wait();
            resetPort.State = true;
        }

        private void WriteChar(char ch, int place)
        {
            // 0b10100xxx - address
            // 0b000xxxxx - row data
            var char_data = new byte[8];
            var base_idx = (((uint)ch) & 0xFF) * 5;
            for (int row = 1; row < 8; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    if ((FontData.font_data_00[base_idx + col] & (1 << (row - 1))) != 0)
                    {
                        char_data[row] = (byte)(char_data[row] | (0x80 >> (4 - col)));
                    }
                }
            }
            char_data[0] = ReverseByte((byte)(0b10100_000 | (place & 0b00000_111)));

            for (int i = 0; i < 8; i++)
            {
                // CS should be low only during 1 byte transfer
                // so transfer 1 byte per transaction
                spiComms.Write(char_data[i]);
            }
        }

        private byte ReverseByte(byte b)
        {
            byte r = 0;
            for (int i = 0; i < 8; i++)
            {
                r = (byte)((r << 1) | (b & 1));
                b >>= 1;
            }
            return r;
        }

        public void SetText(string text)
        {
            var data = text.PadRight(8);
            for (int i = 0; i < 8; i++)
            {
                WriteChar(data[i], i);
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && createdPort)
                {
                    loadPort.Dispose();
                    resetPort.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}
