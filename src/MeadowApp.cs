using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Hardware;

namespace DIMeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        public static ServiceCollection Services { get; } = new ServiceCollection();

        public MeadowApp()
        {
            Services.Add(Device);

            // you can create the real service like this:
            Services.Create<LedService, ILedService>();

            // or the mock like this:
            // Services.Create<MockLedService, ILedService>();

            BlinkLed();
        }

        protected void BlinkLed()
        {
            // retrieve a service by registration type from the services collection like this:
            var svc = Services.Get<ILedService>();

            // create a task to walk through some colors on the LED
            Task.Run(async () =>
            {
                var state = false;
                while (true)
                {
                    state = !state;

                    foreach (var e in Enum.GetValues(typeof(Color)))
                    {
                        svc.Illuminate((Color)e, state);
                        await Task.Delay(200);
                    }
                }
            });
        }
    }
}
