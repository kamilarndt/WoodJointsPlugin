using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using WoodJointsPlugin.Models;

namespace WoodJointsPlugin.UI
{
    public class EnhancedJointSelectionDialog : Dialog<JointSelectionResult>
    {
        private Slider percentSlider;
        private Label percentValueLabel;
        private NumericStepper clearanceBox;
        private RadioButtonList positionModeList;
        private TenonPositionMode selectedPositionMode;

        public EnhancedJointSelectionDialog()
        {
            Title = "Parametry połączenia czopowego";
            MinimumSize = new Size(350, 180);
            Padding = new Padding(10);

            // Suwak procentowy
            percentSlider = new Slider { MinValue = 0, MaxValue = 100, Value = 50, Width = 200 };
            percentValueLabel = new Label { Text = "50%" };
            percentSlider.ValueChanged += (s, e) =>
            {
                percentValueLabel.Text = percentSlider.Value + "%";
            };

            // NumericStepper for clearance
            clearanceBox = new NumericStepper { MinValue = 0, MaxValue = 5, Increment = 0.01, DecimalPlaces = 2, Value = 0.5, Width = 80 };

            // RadioButtonList for position mode
            positionModeList = new RadioButtonList
            {
                Orientation = Orientation.Horizontal,
                Spacing = new Size(10, 10),
                Width = 200
            };
            positionModeList.Items.Add(new ListItem { Text = "Centralny", Tag = TenonPositionMode.Centered });
            positionModeList.Items.Add(new ListItem { Text = "Przy krawędzi", Tag = TenonPositionMode.Edge });
            positionModeList.SelectedIndex = 0;
            positionModeList.SelectedIndexChanged += (sender, e) =>
            {
                if (positionModeList.SelectedValue is ListItem selectedItem && selectedItem.Tag is TenonPositionMode mode)
                    selectedPositionMode = mode;
            };
            selectedPositionMode = TenonPositionMode.Centered;

            // Przycisk OK
            var okButton = new Button { Text = "OK" };
            okButton.Click += (sender, e) =>
            {
                Result = new JointSelectionResult
                {
                    IntersectionPercent = percentSlider.Value,
                    Clearance = (double)clearanceBox.Value,
                    PositionMode = selectedPositionMode
                };
                Close();
            };
            var cancelButton = new Button { Text = "Anuluj" };
            cancelButton.Click += (sender, e) => { Result = null; Close(); };
            DefaultButton = okButton;
            AbortButton = cancelButton;

            // Layout
            Content = new StackLayout
            {
                Padding = 10,
                Spacing = 10,
                Items =
                {
                    new Label { Text = "Intersection [%]", Font = new Font(FontFamilies.Sans, 12, FontStyle.Bold) },
                    new StackLayout { Orientation = Orientation.Horizontal, Items = { percentSlider, percentValueLabel } },
                    new StackLayout { Orientation = Orientation.Horizontal, Items = { new Label { Text = "Luz [mm]:" }, clearanceBox } },
                    new Label { Text = "Pozycja czopa:", Font = new Font(FontFamilies.Sans, 12, FontStyle.Bold) },
                    positionModeList,
                    new StackLayout { Orientation = Orientation.Horizontal, Spacing = 10, Items = { okButton, cancelButton } }
                }
            };
        }
    }

    public class JointSelectionResult
    {
        public double IntersectionPercent { get; set; }
        public double Clearance { get; set; }
        public TenonPositionMode PositionMode { get; set; }
    }
}
