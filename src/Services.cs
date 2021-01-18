using Meadow.Devices;
using Meadow.Hardware;
using System;

namespace DIMeadowApp
{
    public enum Color
    {
        Red,
        Green,
        Blue
    }

    public interface ILedService
    {
        void Illuminate(Color color, bool state);
    }

    public class MockLedService : ILedService
    {
        public void Illuminate(Color color, bool state)
        {
            Console.WriteLine($"{color} LED is now {(state ? "ON" : "OFF")}");
        }
    }

    public class LedService : ILedService
    {
        private IDigitalOutputPort[] m_leds = new IDigitalOutputPort[3];

        public LedService(F7Micro device)
        {
            // the Device gets injected here by the DI container

            Console.WriteLine($"Device object injection {(device == null ? "FAILED" : "successsful")}");

            // create ports for the onboard LED
            m_leds[0] = device.CreateDigitalOutputPort(device.Pins.OnboardLedRed);
            m_leds[1] = device.CreateDigitalOutputPort(device.Pins.OnboardLedGreen);
            m_leds[2] = device.CreateDigitalOutputPort(device.Pins.OnboardLedBlue);
        }

        public void Illuminate(Color color, bool state)
        {
            m_leds[(int)color].State = state;

            Console.WriteLine($"Set {color} to {(state ? "ON" : "OFF")}");
        }
    }
}
