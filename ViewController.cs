using System;
using CoreBluetooth;
using CoreGraphics;

using UIKit;

namespace Bluetooth
{
    public partial class ViewController : UIViewController
    {
        private BluetoothService bluetoothService;
        public int h = 40;

        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            // Declare new instance of BluetoothService
            this.bluetoothService = new BluetoothService();
            // Define when each method should trigger
            this.bluetoothService.StateChanged += StateChanged;
            this.bluetoothService.DiscoveredDevice += DiscoveredDevice;
            this.bluetoothService.DeviceConnected += DeviceConnected;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        // This method calls the Scan method of bluetoothService
        private async void StateChanged(object sender, StateChangedEventArgs args)
        {
            if (args.State == CBCentralManagerState.PoweredOn)
            {
                await this.bluetoothService.Scan(10000);
            }
        }

        // This method writes every unique DiscoveredDevice's name to the console
        private void DiscoveredDevice(object sender, CBDiscoveredPeripheralEventArgs args)
        {
            if (args?.Peripheral?.Name != null)
            {
                var btn = UIButton.FromType(UIButtonType.System);
                btn.Frame = new CGRect(10, h, View.Bounds.Width - 20, 40);
                btn.SetTitle($"Connect to {args.Peripheral.Name}", UIControlState.Normal);

                btn.TouchUpInside += (sendee, e) =>
                {
                    this.bluetoothService.ConnectTo(args.Peripheral);
                };
                View.AddSubview(btn);
                h = h + 40;
            }
        }

        private void DeviceConnected(object sender, CBPeripheralEventArgs args)
        {
            // This isn't working, but also not a focus so haven't fixed yet
            for (int i = 0; i < View.Subviews.Length; i++)
            {
                View.Subviews[i].Dispose();
                View.Subviews[i] = null;
            }

            Console.WriteLine("Peripheral State: " + args.Peripheral.State);

            var disconnectBtn = UIButton.FromType(UIButtonType.System);
            disconnectBtn.Frame = new CGRect(10, (View.Bounds.Height / 2) - 50, View.Bounds.Width - 10, 40);
            disconnectBtn.SetTitle("Disconnect", UIControlState.Normal);

            disconnectBtn.TouchUpInside += async (sendoo, e) =>
            {
                await this.bluetoothService.Disconnect(args.Peripheral, 2000);
            };

            View.AddSubview(disconnectBtn);
        }
    }
}

