using Eto.Forms;
using Eto.Drawing;

public class JointSettingsDialog : Dialog
{
    public double IntersectionPercent { get; private set; } = 50;

    public JointSettingsDialog()
    {
        Title = "Joint Settings";
        Width = 300;
        Height = 150;

        var slider = new Slider { MinValue = 0, MaxValue = 100, Value = 50 };
        slider.ValueChanged += (s, e) => IntersectionPercent = slider.Value;

        var okButton = new Button { Text = "OK" };
        okButton.Click += (s, e) => Close();

        var layout = new DynamicLayout();
        layout.AddRow("Intersection %:", slider);
        layout.AddRow(null, okButton);

        Content = layout;
    }
}
