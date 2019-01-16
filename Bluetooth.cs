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
        public void GetService(CBPeripheral peripheral)
        {
            peripheral.Delegate = new PeripheralDelegate();

            peripheral.DiscoverServices();
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

    public class PeripheralDelegate : CBPeripheralDelegate
    {

        public override void DiscoveredService(CBPeripheral peripheral, NSError error)
        {
            foreach (var service in peripheral.Services)
            {
                //if (service.UUID == CBUUID.FromString("22BB746F-2BA0-7554-2D6F-726568705327"))
                //{
                    peripheral.DiscoverCharacteristics(service);
                //}
            }
        }

        public override void DiscoveredCharacteristic(CBPeripheral peripheral, CBService service, NSError error)
        {

            //0x2804ca780
            //6755BD3B-BC83-AB98-3BB9-2C989CA9E895

            foreach (var characteristic in service.Characteristics)
            {
                //if (characteristic.UUID == CBUUID.FromString("22BB746F-2BA1-7554-2D6F-726568705327"))
                //{
                    Console.WriteLine($"Characteristics of {service}:");
                    Console.WriteLine(characteristic);
                    Console.WriteLine("Attempting to write data.");

                    if (characteristic.UUID == CBUUID.FromString("22BB746F-2BA6-7554-2D6F-726568705327"))
                    {
                        peripheral.SetNotifyValue(true, characteristic);
                    }

                /*
                 * var LEDCommand = new FLEDCommand(255, 0, 0, false, 0x37);
                 * var command = NSData.FromArray(LEDCommand.CreateCommandPacket());
                 */

                var pingCommand = new PingCommand();
                    var command = NSData.FromArray(pingCommand.CreateCommand());                    

                    Console.WriteLine("-------------------------");
                    Console.WriteLine(command);
                    Console.WriteLine("Bytes:  " + command.Bytes);
                    Console.WriteLine("Length: " + command.Length);
                    Console.WriteLine("Characteristic Properties: " + characteristic.Properties);
                    Console.WriteLine("-------------------------");

                    Console.WriteLine("Sending the Command");
                    peripheral.WriteValue(command, characteristic, CBCharacteristicWriteType.WithResponse);

                //}
            }
        }

        public override void WroteCharacteristicValue(CBPeripheral peripheral, CBCharacteristic characteristic, NSError error)
        {
            Console.WriteLine("Characteristic Value written.");
            Console.WriteLine("Error: " + error);
        }
    }
}
