using System;
using System.Threading.Tasks;
using Foundation;
using CoreBluetooth;

namespace Bluetooth
{
    public class BluetoothService
    {
        // Initiates the CBCentralManager object
        private readonly CBCentralManager manager = new CBCentralManager();

        // Creates events that happen
        public EventHandler<CBDiscoveredPeripheralEventArgs> DiscoveredDevice;
        public EventHandler<StateChangedEventArgs> StateChanged;
        public EventHandler<CBPeripheralEventArgs> DeviceConnected;

        public BluetoothService()
        {
            // Whenever the built-in function triggers, it triggers the other functions
            this.manager.DiscoveredPeripheral += this.DiscoveredPeripheral;
            this.manager.UpdatedState += this.UpdatedState;
            this.manager.ConnectedPeripheral += this.ConnectedDevice;
        }

        public async Task Disconnect(CBPeripheral peripheral, int interval)
        {
            this.manager.CancelPeripheralConnection(peripheral);
            await Task.Delay(interval);
            Console.WriteLine($"Disconnected from {peripheral}");
        }

        // This method scans for Bluetooth devices
        public async Task Scan(int scanInterval)
        {
            // empty array to scan for everything
            CBUUID[] cbuuids = null;
            PeripheralScanningOptions scanningOptions = new PeripheralScanningOptions();
            scanningOptions.AllowDuplicatesKey = false;
            this.manager.ScanForPeripherals(cbuuids, scanningOptions);
            Console.WriteLine("Scanning started");
            // Stop scanning after specified amount of time
            await Task.Delay(scanInterval);
            this.StopScan();
        }

        // This method stops scanning
        // Defined as separate function to be called whenever
        public void StopScan()
        {
            this.manager.StopScan();
            Console.WriteLine("Scanning stopped");
        }

        // This method connects to the specified peripheral
        public void ConnectTo(CBPeripheral peripheral)
        {
            Console.WriteLine($"Trying to connect to {peripheral}");
            this.manager.ConnectPeripheral(peripheral);
        }

        // This method gets the devices connected to the central manager
        public CBPeripheral[] GetConnectedDevices()
        {
            CBUUID cbuuids = null;
            return this.manager.RetrieveConnectedPeripherals(new[] { cbuuids });
        }

        // This method gets the services of connected device
        public async Task GetServices(CBPeripheral peripheral)
        {
            peripheral.Delegate = new PeripheralDelegate();

            peripheral.DiscoverServices();
            await Task.Delay(2000);
        }

        public void WakeUp(CBPeripheral peripheral)
        {
            // Method that sends commands to sphero in specific order to wake it up

            // Create required data
            NSData antiDOS = NSData.FromString("011i3");
            var txdata = new byte[1];
            txdata[0] = 0x07;
            NSData TXdata = NSData.FromArray(txdata);
            var wakeupdata = new byte[1];
            wakeupdata[0] = 0x01;
            NSData WakeupData = NSData.FromArray(wakeupdata);

            // Notes
            // peripheral.Services[0].Characteristics is Control/Response Characteristics
            // Control first, then Response
            // peripheral.Services[1].Characteristics is Radio Service Characteristics
            // peripheral.Services[2].Characteristics is Device Info Characteristics
            // peripheral.Services[3].Characteristics is Device Information

            // Sending the wake up commands to the sphero
            // Can be simplified (either if discovery of services and characteristics are static, or by sorting the arrays
            // that information is contained in. For now this is just a working version.


            foreach (var characteristic in peripheral.Services[1].Characteristics)
            {
                if (characteristic.UUID == CBUUID.FromString("22bb746f-2bbd-7554-2D6F-726568705327"))
                {
                    Console.WriteLine("Writing to Anti DOS");
                    peripheral.WriteValue(antiDOS, characteristic, CBCharacteristicWriteType.WithResponse);
                }
            }
            foreach (var characteristic in peripheral.Services[1].Characteristics)
            {
                if (characteristic.UUID == CBUUID.FromString("22bb746f-2bb2-7554-2D6F-726568705327"))
                {
                    Console.WriteLine("Writing to TX Power");
                    peripheral.WriteValue(TXdata, characteristic, CBCharacteristicWriteType.WithResponse);
                }
            }
            foreach (var characteristic in peripheral.Services[1].Characteristics)
            {
                if (characteristic.UUID == CBUUID.FromString("22bb746f-2bbf-7554-2D6F-726568705327"))
                {
                    Console.WriteLine("Writing to Wakeup");
                    peripheral.WriteValue(WakeupData, characteristic, CBCharacteristicWriteType.WithResponse);
                }
            }


            /*
            peripheral.WriteValue(antiDOS, peripheral.Services[1].Characteristics[3], CBCharacteristicWriteType.WithResponse);
            peripheral.WriteValue(TXdata, peripheral.Services[1].Characteristics[0], CBCharacteristicWriteType.WithResponse);
            peripheral.WriteValue(WakeupData, peripheral.Services[1].Characteristics[5], CBCharacteristicWriteType.WithResponse);
            */           
        }

        public void Sleep(CBPeripheral peripheral)
        {
            Console.WriteLine("Sleep method entered");
            var data = NSData.FromString("011i3");

            Console.WriteLine("Created data variable");
            foreach (var characteristic in peripheral.Services[1].Characteristics)
            {
                Console.WriteLine("Checking characteristic");
                if (characteristic.UUID == CBUUID.FromString("22BB746F-2bb7-7554-2D6F-726568705327"))
                {
                    Console.WriteLine("Writing to Deep Sleep");
                    peripheral.WriteValue(data, characteristic, CBCharacteristicWriteType.WithResponse);
                }
            }
        }

        public void SendCommand(CBPeripheral peripheral, BaseSpheroCommand command)
        {
            // Send the given command to the sphero
            NSData dataCommand = NSData.FromArray(command.GetBytes());
            peripheral.WriteValue(dataCommand, peripheral.Services[0].Characteristics[0], CBCharacteristicWriteType.WithoutResponse);
        }

        // This method is run everytime a peripheral is discovered
        public void DiscoveredPeripheral(object sender, CBDiscoveredPeripheralEventArgs args)
        {
            // Invokes the DiscoveredDevice method in the ViewController.cs page
            if (args?.Peripheral?.Name != null)
            {
                Console.WriteLine(args.Peripheral.Name);
            }
            this.DiscoveredDevice?.Invoke(sender, args);
        }

        // This method is run whenever the state is updated
        public void UpdatedState(object sender, EventArgs args)
        {
            // Creates new instance of event to send to other method
            StateChangedEventArgs state = new StateChangedEventArgs();
            state.State = this.manager.State;
            // Invokes the StateChanged method in the ViewController.cs page
            this.StateChanged?.Invoke(sender, state);
        }

        // This method gets called whenver a device is connected
        public void ConnectedDevice(object sender, CBPeripheralEventArgs args)
        {
            // Invokes the DeviceConnected method in the ViewController.cs page
            if (args.Peripheral.State == CBPeripheralState.Connected)
            {
                Console.WriteLine($"Connected to {args.Peripheral}");
            }
            this.DeviceConnected?.Invoke(sender, args);
        }
    }

    // This class defines a custom event that defines the CBCentralManager's state
    public class StateChangedEventArgs : EventArgs
    {
        public CBCentralManagerState State { get; set; }
    }

    // Control the events of the sphero that get called
    public class PeripheralDelegate : CBPeripheralDelegate
    {

        public override void DiscoveredService(CBPeripheral peripheral, NSError error)
        {
            // Discover all the characteristics of the services contained in peripheral.Services
            foreach (var service in peripheral.Services)
            {
                peripheral.DiscoverCharacteristics(service);
            }
        }

        public override void DiscoveredCharacteristic(CBPeripheral peripheral, CBService service, NSError error)
        {
            // Discover the Descriptors of the characteristics contained in service.Characteristics
            foreach (var characteristic in service.Characteristics)
            {
                peripheral.DiscoverDescriptors(characteristic);
            }
        }

        public override void WroteCharacteristicValue(CBPeripheral peripheral, CBCharacteristic characteristic, NSError error)
        {
            // Method used to test successful data sends.
            if (error != null)
            {
                Console.WriteLine($"Error: {error}");
            }
        }
    }
}
