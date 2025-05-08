using System;
using Eto.Forms;
using Eto.Drawing;
using WoodJointsPlugin.Models;

namespace WoodJointsPlugin.UI
{
    public class JointParameterDialog : Dialog<(double width, double depth, double clearance)?>
    {
        private NumericStepper widthStepper;
        private NumericStepper depthStepper;
        private NumericStepper clearanceStepper;
        private NumericStepper tailAngleStepper;
        private NumericStepper fingersCountStepper;

        public JointParameterDialog(JointType jointType = JointType.MortiseAndTenon)
        {
            Title = "Parametry po³¹czenia";
            MinimumSize = new Size(400, 300);
            Padding = new Padding(10);

            // Create controls
            widthStepper = new NumericStepper
            {
                Value = 20.0,
                MinValue = 5.0,
                MaxValue = 500.0,
                DecimalPlaces = 1,
                Increment = 1.0
            };

            depthStepper = new NumericStepper
            {
                Value = 30.0,
                MinValue = 5.0,
                MaxValue = 500.0,
                DecimalPlaces = 1,
                Increment = 1.0
            };

            clearanceStepper = new NumericStepper
            {
                Value = 0.1,
                MinValue = 0.0,
                MaxValue = 5.0,
                DecimalPlaces = 2,
                Increment = 0.1
            };

            tailAngleStepper = new NumericStepper
            {
                Value = 15.0,
                MinValue = 5.0,
                MaxValue = 45.0,
                DecimalPlaces = 1,
                Increment = 1.0
            };

            fingersCountStepper = new NumericStepper
            {
                Value = 3,
                MinValue = 1,
                MaxValue = 20,
                DecimalPlaces = 0,
                Increment = 1
            };

            // Create buttons
            var okButton = new Button { Text = "OK" };
            okButton.Click += (sender, e) =>
            {
                Result = (widthStepper.Value, depthStepper.Value, clearanceStepper.Value);
                Close();
            };

            var cancelButton = new Button { Text = "Anuluj" };
            cancelButton.Click += (sender, e) =>
            {
                Result = null;  // Now this is valid because we're using a nullable tuple
                Close();
            };

            // Layout
            var layout = new DynamicLayout();
            layout.DefaultPadding = new Padding(10);
            layout.DefaultSpacing = new Size(5, 5);

            layout.Add(new Label { Text = "Parametry podstawowe:", Font = new Font(FontFamilies.Sans, 12, FontStyle.Bold) });
            layout.AddRow(new Label { Text = "Szerokoœæ (mm):" }, widthStepper);
            layout.AddRow(new Label { Text = "G³êbokoœæ (mm):" }, depthStepper);
            layout.AddRow(new Label { Text = "Luz (mm):" }, clearanceStepper);

            // Specialized parameters for specific joint types
            if (jointType == JointType.Dovetail)
            {
                layout.Add(new Label { Text = "Parametry jaskó³czego ogona:", Font = new Font(FontFamilies.Sans, 12, FontStyle.Bold) });
                layout.AddRow(new Label { Text = "K¹t rozszerzenia (°):" }, tailAngleStepper);
            }
            else if (jointType == JointType.FingerJoint || jointType == JointType.BoxJoint)
            {
                layout.Add(new Label { Text = "Parametry po³¹czenia palcowego:", Font = new Font(FontFamilies.Sans, 12, FontStyle.Bold) });
                layout.AddRow(new Label { Text = "Liczba palców:" }, fingersCountStepper);
            }

            layout.Add(null); // Spacer
            layout.AddRow(null, cancelButton, okButton);

            Content = layout;
        }
    }
}
