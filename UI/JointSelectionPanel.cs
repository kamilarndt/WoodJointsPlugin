using Eto.Forms;
using Eto.Drawing;

namespace WoodJointsPlugin.UI
{
    public class JointSelectionPanel : Dialog<string>
    {
        public JointSelectionPanel()
        {
            Title = "Wybierz typ połączenia";
            Width = 300;
            Height = 150;

            var label = new Label { Text = "Typ połączenia:", VerticalAlignment = VerticalAlignment.Center };
            var combo = new DropDown { Items = { "Czop", "Wpust", "Jaskółczy ogon" }, SelectedIndex = 0 };
            var okButton = new Button { Text = "Zastosuj" };
            okButton.Click += (s, e) =>
            {
                Result = combo.SelectedValue?.ToString() ?? "Czop";
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
                    new StackLayoutItem(label, HorizontalAlignment.Left),
                    new StackLayoutItem(combo, HorizontalAlignment.Stretch),
                    new StackLayoutItem(new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 10,
                        Items = { okButton, cancelButton }
                    }, HorizontalAlignment.Right)
                }
            };
        }
    }
} 