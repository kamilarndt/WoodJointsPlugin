using System;
using Eto.Forms;
using Eto.Drawing;
using WoodJointsPlugin.Models;

namespace WoodJointsPlugin.UI
{
    public class JointSelectionPanel : Dialog<string>
    {
        private RadioButtonList jointTypeList;
        
        public JointSelectionPanel()
        {
            Title = "Wybór typu po³¹czenia";
            MinimumSize = new Size(300, 400);
            Padding = new Padding(10);
            
            // Create radio button list for joint types
            jointTypeList = new RadioButtonList
            {
                Orientation = Orientation.Vertical,
                Spacing = new Size(5, 10)
            };
            
            // Add all joint types to the list
            foreach (JointType type in Enum.GetValues(typeof(JointType)))
            {
                jointTypeList.Items.Add(new ListItem
                {
                    Text = type.ToDisplayString(),
                    Tag = type
                });
            }
            
            // Select first item by default
            if (jointTypeList.Items.Count > 0)
                jointTypeList.SelectedIndex = 0;
            
            // Create buttons
            var okButton = new Button { Text = "OK" };
            okButton.Click += (sender, e) => 
            {
                if (jointTypeList.SelectedValue != null)
                {
                    var selectedItem = jointTypeList.SelectedValue as ListItem;
                    Result = selectedItem.Text;
                    Close();
                }
                else
                {
                    MessageBox.Show("Proszê wybraæ typ po³¹czenia.", "Wybór typu po³¹czenia", MessageBoxType.Warning);
                }
            };
            
            var cancelButton = new Button { Text = "Anuluj" };
            cancelButton.Click += (sender, e) => 
            {
                Result = null;
                Close();
            };
            
            // Layout
            var layout = new DynamicLayout();
            layout.DefaultPadding = new Padding(10);
            layout.DefaultSpacing = new Size(5, 5);
            
            layout.Add(new Label { Text = "Wybierz typ po³¹czenia:", Font = new Font(FontFamilies.Sans, 12, FontStyle.Bold) });
            layout.Add(jointTypeList);
            
            layout.AddRow(null, cancelButton, okButton);
            
            Content = layout;
        }
    }
}
