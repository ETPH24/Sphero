using System;
using UIKit;
using CoreGraphics;
using CoreBluetooth;
using Foundation;

namespace Bluetooth
{
    public class SpheroViewController : UIViewController
    {
        // declare global variables
        public int h = 40;
        public int max = 255;
        public int min = 0;
        private Circle circle;
        private Circle touchCircle;

        // Get and set variables needed for this view to function properly
        public SpheroViewController(BluetoothService bluetoothService, CBPeripheral peripheral)
        {
            BluetoothDevice = bluetoothService;
            Peripheral = peripheral;
        }

        public BluetoothService BluetoothDevice;
        public CBPeripheral Peripheral;

        // Gets called when view is pushed to the screen
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = UIColor.White;
        }

        // Gets called when view is pushed to the screen
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // declare back button
            var backBtn = UIButton.FromType(UIButtonType.System);
            backBtn.Frame = new CGRect(10, 10, 50, 10 + h);
            backBtn.SetTitle("Back", UIControlState.Normal);

            backBtn.TouchUpInside += async (sender, e) =>
            {
                // Disconnect from device and switch back to previous view
                await BluetoothDevice.Disconnect(Peripheral, 1000);
                ViewController viewController = new ViewController();
                PresentViewController(viewController, true, null);
            };

            // declare red color slider
            UISlider redColorSlider = new UISlider
            {
                Frame = new CGRect(10, 50, View.Bounds.Width - 20, 40),
                MaxValue = max,
                MinValue = min,
                Continuous = true,
                Value = max / 2
            };

            // declare the green color slider
            UISlider greenColorSlider = new UISlider
            {
                Frame = new CGRect(10, 100, View.Bounds.Width - 20, 40),
                MaxValue = max,
                MinValue = min,
                Continuous = true,
                Value = max / 2
            };

            // declare the blue color slider
            UISlider blueColorSlider = new UISlider
            {
                Frame = new CGRect(10, 150, View.Bounds.Width - 20, 40),
                MaxValue = max,
                MinValue = min,
                Continuous = true,
                Value = max / 2
            };

            redColorSlider.ValueChanged += (sendaa, e) =>
            {
                // When the red color slider's value changed: send the sphero the color command with the three color slider's given colors
                var colorCommand = new FLEDCommand((int)redColorSlider.Value, (int)greenColorSlider.Value, (int)blueColorSlider.Value, 0);
                this.BluetoothDevice.SendCommand(Peripheral, colorCommand);
            };

            greenColorSlider.ValueChanged += (sendee, e) =>
            {
                // When the green color slider's value changed: send the sphero the color command with the three color slider's given colors
                var colorCommand = new FLEDCommand((int)redColorSlider.Value, (int)greenColorSlider.Value, (int)blueColorSlider.Value, 0);
                this.BluetoothDevice.SendCommand(Peripheral, colorCommand);
            };

            blueColorSlider.ValueChanged += (sendii, e) =>
            {
                // When the blue color slider's value changed: send the sphero the color command with the three color slider's given colors
                var colorCommand = new FLEDCommand((int)redColorSlider.Value, (int)greenColorSlider.Value, (int)blueColorSlider.Value, 0);
                this.BluetoothDevice.SendCommand(Peripheral, colorCommand);
            };

            // Create circle object
            circle = new Circle(new CGRect(10, 200, View.Bounds.Width - 20, View.Bounds.Height - 200), 5);

            // This is the interactive circle
            touchCircle = new Circle(new CGRect((circle.Frame.Width / 2) - 25, (circle.Frame.Height / 2) - 25 + 200, 50, 50), 2);
            touchCircle.UserInteractionEnabled = true;

            // Add the button to the screen.
            View.AddSubview(backBtn);
            // Add the sliders to the screen
            View.AddSubview(redColorSlider);
            View.AddSubview(greenColorSlider);
            View.AddSubview(blueColorSlider);
            // Add back circle to screen
            View.AddSubview(circle);
            // Add starting position of frontCircle to screen
            View.AddSubview(touchCircle);
        }

        // Gets called when View leaves the screen
        public override async void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            await this.BluetoothDevice.Disconnect(Peripheral, 1000);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            UITouch touch = touches.AnyObject as UITouch;
            if (touch != null)
            {
                if (this.circle.Frame.Contains(touch.LocationInView(this.View)))
                {
                    var x = touch.GetPreciseLocation(this.View).X;
                    var y = touch.GetPreciseLocation(this.View).Y;
                    if (this.circle.DistanceFromOrigin(x, y) > this.circle.GetRadius())
                    {
                        // doesn't work
                        var newPoint = this.circle.PointOnCircle(x, y);
                        this.touchCircle.Frame = new CGRect(newPoint.X, newPoint.Y, 50, 50);
                    }
                    else
                    {
                        // works right
                        this.touchCircle.Frame = new CGRect(x, y, 50, 50);
                    }
                    // doesn't work
                    var distance = this.circle.DistanceFromOrigin(this.touchCircle.Frame.X, this.touchCircle.Frame.Y);
                    var weight = this.circle.GetWeight(distance);
                    var rollCommand = new RollCommand(255 * (int)weight, 0, false);
                    this.BluetoothDevice.SendCommand(Peripheral, rollCommand);
                }
            }
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);
            UITouch touch = touches.AnyObject as UITouch;
            if (touch != null)
            {
                if (this.circle.Frame.Contains(touch.LocationInView(this.View)))
                {
                    var x = touch.GetPreciseLocation(this.View).X;
                    var y = touch.GetPreciseLocation(this.View).Y;
                    if (this.circle.DistanceFromOrigin(x, y) > this.circle.GetRadius())
                    {
                        // doesn't work
                        var newPoint = this.circle.PointOnCircle(x, y);
                        this.touchCircle.Frame = new CGRect(newPoint.X, newPoint.Y, 50, 50);
                    }
                    else
                    {
                        // works right
                        this.touchCircle.Frame = new CGRect(x, y, 50, 50);
                    }
                    // doesn't work
                    var distance = this.circle.DistanceFromOrigin(this.touchCircle.Frame.X, this.touchCircle.Frame.Y);
                    var weight = this.circle.GetWeight(distance);
                    var rollCommand = new RollCommand(255 * (int)weight, 0, false);
                    this.BluetoothDevice.SendCommand(Peripheral, rollCommand);
                }
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            // doesn't work
            this.touchCircle.Frame = new CGRect(this.circle.Frame.Width / 2 - 25, this.circle.Frame.Height / 2 - 25, 50, 50);
            var rollCommand = new RollCommand(0, 0, true);
            this.BluetoothDevice.SendCommand(Peripheral, rollCommand);
        }
    }

    class Circle : UIView
    {
        const float FULL_CIRCLE = 2 * (float)Math.PI;
        int _radius = 10;
        int _lineWidth = 5;
        UIColor _backColor = UIColor.Gray;
        public Circle(CGRect frame, int lineWidth)
        {
            this.BackgroundColor = UIColor.White;
            _lineWidth = lineWidth;
            this.Frame = new CGRect(frame.X, frame.Y, frame.Width, frame.Height);
            Origin = new CGPoint((frame.Width / 2) + frame.X, (frame.Height / 2) + frame.Y);
        }

        public CGPoint Origin { get; set; }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);
            // draw stuff in here
            using (CGContext g = UIGraphics.GetCurrentContext())
            {
                var diameter = Math.Min(this.Bounds.Width, this.Bounds.Height);
                _radius = (int)(diameter / 2) - _lineWidth;

                DrawGraph(g, this.Bounds.GetMidX(), this.Bounds.GetMidY());
            }
        }

        public void DrawGraph(CGContext g, nfloat x, nfloat y)
        {
            g.SetLineWidth(_lineWidth);

            // Draw circle
            CGPath path = new CGPath();
            _backColor.SetStroke();
            path.AddArc(x, y, _radius, 0, FULL_CIRCLE, true);
            g.AddPath(path);
            g.DrawPath(CGPathDrawingMode.Stroke);
        }

        public CGPoint PointOnCircle(nfloat x2, nfloat y2)
        {
            // Calculating parts of equation of line between circleOrigin and provided point
            var circleOrigin = new CGPoint((this.Frame.Width / 2) + this.Frame.X, (this.Frame.Height / 2) + this.Frame.Y);
            var x1 = circleOrigin.X;
            var y1 = circleOrigin.Y;
            var theta = Math.Atan2((y2 - y1), (x2 - x1));
            var newX = _radius * Math.Cos(theta);
            var newY = _radius * Math.Sin(theta);

            return new CGPoint(newX, newY);
        }

        public double DistanceFromOrigin(nfloat x2, nfloat y2)
        {
            // Calculating parts of equation of line between circleOrigin and provided point
            var circleOrigin = new CGPoint((this.Frame.Width / 2) + this.Frame.X, (this.Frame.Height / 2) + this.Frame.Y);
            var x1 = circleOrigin.X;
            var y1 = circleOrigin.Y;
            var m = (y1 - y2) / (x1 - x2);
            var b = y1 - (m * x1);

            // calculate distance
            return Math.Sqrt(Math.Pow((x1 - x2), 2) + Math.Pow((y1 - y2), 2));
        }

        public double GetWeight(double distance)
        {
            return distance / _radius;
        }

        public double GetRadius()
        {
            return _radius;
        }
    }
}
