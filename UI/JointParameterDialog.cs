using Eto.Forms;
using Eto.Drawing;

namespace WoodJointsPlugin.UI
{
    public class JointParameterDialog : Dialog<(double width, double depth, double clearance)?>
    {
        public JointParameterDialog()
        {
            Title = "Parametry połączenia";
            Width = 300;
            Height = 220;

            var widthBox = new NumericStepper { MinValue = 1, MaxValue = 1000, Value = 50, DecimalPlaces = 2, Increment = 1 };
            var depthBox = new NumericStepper { MinValue = 1, MaxValue = 1000, Value = 30, DecimalPlaces = 2, Increment = 1 };
            var clearanceBox = new NumericStepper { MinValue = 0, MaxValue = 10, Value = 0.5, DecimalPlaces = 2, Increment = 0.1 };

            var okButton = new Button { Text = "Zastosuj" };
            okButton.Click += (s, e) =>
            {
                Result = ((double)widthBox.Value, (double)depthBox.Value, (double)clearanceBox.Value);
                Close();
            };
            var cancelButton = new Button { Text = "Anuluj" };
            cancelButton.Click += (s, e) =>
            {
                Result = null;
                Close();
            };

            Content = new StackLayout
            {
                Padding = 10,
                Spacing = 10,
                Items =
                    {
                        new Label { Text = "Szerokość [mm]:" }, widthBox,
                        new Label { Text = "Głębokość [mm]:" }, depthBox,
                        new Label { Text = "Luz montażowy [mm]:" }, clearanceBox,
                        new StackLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 10,
                            Items = { okButton, cancelButton }
                        }
                    }
            };
        }
    }
} 