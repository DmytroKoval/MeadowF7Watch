using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Foundation.Leds;
using Meadow.Peripherals.Leds;
using System;
using System.Threading.Tasks;
using Meadow.Units;
using System.Threading;
using System.Net;
using System.Net.Http;
using Meadow.Foundation.Serialization;
using System.Collections;
using System.IO;
using System.ComponentModel;

namespace F7Watch;

public class MeadowApp : App<F7FeatherV2>
{
    private ISpiBus spi;
    private SDA5708 display;
    private Timer dispTimer;
    private int tzOffset;
    private static readonly HttpClient client = new HttpClient();

    public override Task Initialize()
    {
        Resolver.Log.ShowTicks = true;

        Task.Run(async () => await QueryTimeZoneInfo());
        Thread.Yield();

        Resolver.Log.Info("Initialize...");

        var busSpeed = new Frequency(12, Frequency.UnitType.Megahertz);

        var spiConfig = new SpiClockConfiguration(busSpeed, SpiClockConfiguration.Mode.Mode0);

        spi = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.COPI, Device.Pins.CIPO, spiConfig);

        display = new SDA5708(spi, Device.Pins.D00, Device.Pins.D01);
        
        display.SetBrightness(2);

        dispTimer = new Timer(DisplayTime, null, Timeout.Infinite, Timeout.Infinite);

        return base.Initialize();
    }

    public DateTime LocalTime => DateTime.UtcNow.AddSeconds(tzOffset);

    public void DisplayTime(object _)
    {
        display.SetText(LocalTime.ToString("HH:mm:ss"));
    }

    public override async Task Run()
    {
        Resolver.Log.Info("Run...");

        display.SetText("MeadowOS");
        
        dispTimer.Change(2000, 100);

        await Task.CompletedTask;
    }

    private async Task QueryTimeZoneInfo()
    {
        try
        {
            Resolver.Log.Info("Querying time zone info...");

            var ipResponse = await client.GetAsync("https://api.ipify.org");
            ipResponse.EnsureSuccessStatusCode();
            string ip = await ipResponse.Content.ReadAsStringAsync();
            Resolver.Log.Info($"IP: {ip}");
                
            var tzInfoResponse = await client.GetAsync($"https://timeapi.io/api/timezone/ip?ipAddress={ip}");
            tzInfoResponse.EnsureSuccessStatusCode();
            string tzInfoJSON = await tzInfoResponse.Content.ReadAsStringAsync();

            var data = MicroJson.DeserializeString(tzInfoJSON) as Hashtable;
            var offsetData = data["currentUtcOffset"] as Hashtable;
            tzOffset = Convert.ToInt32(offsetData["seconds"]);
            Resolver.Log.Info($"UTC offset: {tzOffset}");

        }
        catch (Exception ex)
        {
            Resolver.Log.Error(ex, "Error obtaining time zone info");                
        }
    }
}