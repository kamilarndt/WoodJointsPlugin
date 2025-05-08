using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using Rhino;
using Rhino.Geometry;
using WoodJointsPlugin.Models;

namespace WoodJointsPlugin.UI
{
    /// <summary>
    /// Enhanced dialog for joint selection with visual preview and dimension sliders
    /// </summary>
    public class EnhancedJointSelectionDialog : Dialog<JointSelectionResult>
    {
        
        private RadioButtonList jointTypeList;
        private Slider widthSlider;
        private Slider depthSlider;
        private Slider clearanceSlider;
        private Slider angleSlider;
        private Label widthValueLabel;
        private Label depthValueLabel;
        private Label clearanceValueLabel;
        private Label angleValueLabel;
        private ImageView previewImage;
        private Dictionary<JointType, Image> jointImages;
        private JointType selectedJointType;
        
        // Default values and ranges
        private const double WidthMin = 5;
        private const double WidthMax = 100;
        private const double WidthDefault = 20;
        
        private const double DepthMin = 5;
        private const double DepthMax = 100;
        private const double DepthDefault = 30;
        
        private const double ClearanceMin = 0.05;
        private const double ClearanceMax = 1.0;
        private const double ClearanceDefault = 0.1;
        
        private const double AngleMin = 5;
        private const double AngleMax = 45;
        private const double AngleDefault = 15;

        public EnhancedJointSelectionDialog()
        {
            Title = "Wybór typu po³¹czenia";
            MinimumSize = new Size(600, 500);
            Padding = new Padding(10);
            
            // Initialize joint preview images
            InitializeJointImages();
            
            // Initialize UI components
            CreateJointTypeList();
            CreateDimensionSliders();
            CreatePreviewPanel();
            
            // Create buttons
            var okButton = new Button { Text = "OK" };
            okButton.Click += (sender, e) => 
            {
                Result = new JointSelectionResult
                {
                    JointType = selectedJointType,
                    Width = ConvertSliderValueToWidth(widthSlider.Value),
                    Depth = ConvertSliderValueToDepth(depthSlider.Value),
                    Clearance = ConvertSliderValueToClearance(clearanceSlider.Value),
                    TailAngle = angleSlider.Visible ? ConvertSliderValueToAngle(angleSlider.Value) : AngleDefault
                };
                Close();
            };
            
            var cancelButton = new Button { Text = "Anuluj" };
            cancelButton.Click += (sender, e) => 
            {
                Result = null;
                Close();
            };
            
            // Create main layout
            var mainLayout = new TableLayout
            {
                Padding = new Padding(10),
                Spacing = new Size(15, 15)
            };
            
            // Left panel - joint type selection
            var leftPanel = new TableLayout
            {
                Rows = 
                {
                    new TableRow(new Label { Text = "Typ po³¹czenia:", Font = new Font(FontFamilies.Sans, 12, FontStyle.Bold) }),
                    new TableRow(jointTypeList),
                    new TableRow(null) { ScaleHeight = true }
                }
            };
            
            // Right panel - preview and dimensions
            var rightPanel = new TableLayout
            {
                Rows =
                {
                    new TableRow(new Label { Text = "Podgl¹d:", Font = new Font(FontFamilies.Sans, 12, FontStyle.Bold) }),
                    new TableRow(previewImage),
                    new TableRow(new Label { Text = "Wymiary po³¹czenia:", Font = new Font(FontFamilies.Sans, 12, FontStyle.Bold) }),
                    CreateSliderRow("Szerokoœæ:", widthSlider, widthValueLabel),
                    CreateSliderRow("G³êbokoœæ:", depthSlider, depthValueLabel),
                    CreateSliderRow("Luz:", clearanceSlider, clearanceValueLabel),
                    CreateSliderRow("K¹t jaskó³czego ogona:", angleSlider, angleValueLabel),
                    new TableRow(null) { ScaleHeight = true }
                }
            };
            
            // Add panels to main layout
            mainLayout.Rows.Add(new TableRow(
                new TableCell(leftPanel, true),
                new TableCell(rightPanel, true)
            ) { ScaleHeight = true });
            
            // Add button panel
            var buttonPanel = new TableLayout
            {
                Padding = new Padding(0, 10, 0, 0),
                Spacing = new Size(10, 0),
                Rows = { new TableRow(null, cancelButton, okButton) }
            };
            
            mainLayout.Rows.Add(new TableRow(buttonPanel));
            
            Content = mainLayout;
            
            // Set initial joint type
            UpdateSelectedJointType(JointType.MortiseAndTenon);
        }

        private void InitializeJointImages()
        {
            jointImages = new Dictionary<JointType, Image>();

            JointType[] supportedJoints = { JointType.MortiseAndTenon, JointType.Dovetail };

            foreach (var type in supportedJoints)
            {
                var bitmap = new Bitmap(300, 200, PixelFormat.Format32bppRgba);
                using (var g = new Graphics(bitmap))
                {
                    g.Clear(Colors.LightGrey);
                    string jointTypeStr = GetJointTypeDisplayString(type); // Use the helper method
                    g.DrawText(new Font(FontFamilies.Sans, 14), Colors.Black,
                        new PointF(10, 10), $"Preview for {jointTypeStr}");
                }
                jointImages[type] = bitmap;
            }
        }

        private void CreateJointTypeList()
        {
            // Create radio button list for joint types
            jointTypeList = new RadioButtonList
            {
                Orientation = Orientation.Vertical,
                Spacing = new Size(5, 10),
                Width = 200
            };
            
            // Only add main joint types as shown in the image
            JointType[] mainTypes = { JointType.MortiseAndTenon, JointType.Dovetail };
            
            foreach (var type in mainTypes)
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
                
            // Add event handler for joint type selection change
            jointTypeList.SelectedIndexChanged += (sender, e) => 
            {
                if (jointTypeList.SelectedValue != null)
                {
                    var selectedItem = jointTypeList.SelectedValue as ListItem;
                    if (selectedItem != null && selectedItem.Tag is JointType type)
                    {
                        UpdateSelectedJointType(type);
                    }
                }
            };
        }
        
        private void CreateDimensionSliders()
        {
            // Width slider
            widthSlider = new Slider
            {
                MinValue = 0,
                MaxValue = 100,
                Value = 50, // Default - will be mapped to the actual width
                Width = 300
            };
            widthValueLabel = new Label { Text = $"{WidthDefault:0.0} mm" };
            widthSlider.ValueChanged += (sender, e) => 
            {
                var actualWidth = ConvertSliderValueToWidth(widthSlider.Value);
                widthValueLabel.Text = $"{actualWidth:0.0} mm";
            };
            
            // Depth slider
            depthSlider = new Slider
            {
                MinValue = 0,
                MaxValue = 100,
                Value = 50, // Default - will be mapped to the actual depth
                Width = 300
            };
            depthValueLabel = new Label { Text = $"{DepthDefault:0.0} mm" };
            depthSlider.ValueChanged += (sender, e) => 
            {
                var actualDepth = ConvertSliderValueToDepth(depthSlider.Value);
                depthValueLabel.Text = $"{actualDepth:0.0} mm";
            };
            
            // Clearance slider
            clearanceSlider = new Slider
            {
                MinValue = 0,
                MaxValue = 100,
                Value = 10, // Default - will be mapped to the actual clearance
                Width = 300
            };
            clearanceValueLabel = new Label { Text = $"{ClearanceDefault:0.00} mm" };
            clearanceSlider.ValueChanged += (sender, e) => 
            {
                var actualClearance = ConvertSliderValueToClearance(clearanceSlider.Value);
                clearanceValueLabel.Text = $"{actualClearance:0.00} mm";
            };
            
            // Angle slider for dovetail
            angleSlider = new Slider
            {
                MinValue = 0,
                MaxValue = 100,
                Value = 30, // Default - will be mapped to the actual angle
                Width = 300,
                Visible = false // Initially hidden, will be shown for dovetail joint
            };
            angleValueLabel = new Label { Text = $"{AngleDefault:0.0}°", Visible = false };
            angleSlider.ValueChanged += (sender, e) => 
            {
                var actualAngle = ConvertSliderValueToAngle(angleSlider.Value);
                angleValueLabel.Text = $"{actualAngle:0.0}°";
            };
        }
        
        private void CreatePreviewPanel()
        {
            // Create image preview control
            previewImage = new ImageView
            {
                Size = new Size(300, 200),
                Image = jointImages.ContainsKey(JointType.MortiseAndTenon) 
                    ? jointImages[JointType.MortiseAndTenon] 
                    : null
            };
        }
        
        private TableRow CreateSliderRow(string labelText, Slider slider, Label valueLabel)
        {
            return new TableRow(
                new TableCell(new Label { Text = labelText }, true),
                new TableCell(slider, true),
                new TableCell(valueLabel, true)
            );
        }
        
        private void UpdateSelectedJointType(JointType jointType)
        {
            selectedJointType = jointType;
            
            // Update preview image
            if (jointImages.ContainsKey(jointType))
            {
                previewImage.Image = jointImages[jointType];
            }
            
            // Show/hide specific controls based on joint type
            bool isDovetail = jointType == JointType.Dovetail;
            angleSlider.Visible = isDovetail;
            angleValueLabel.Visible = isDovetail;
            
            // Update labels in the UI to reflect the joint type
            UpdateParameterLabelsForJointType(jointType);
        }
        
        private void UpdateParameterLabelsForJointType(JointType jointType)
        {
            // Customize labels/descriptions based on joint type if needed
        }
        
        // Convert slider values to actual dimensions
        
        private double ConvertSliderValueToWidth(int sliderValue)
        {
            return WidthMin + (sliderValue / 100.0) * (WidthMax - WidthMin);
        }
        
        private double ConvertSliderValueToDepth(int sliderValue)
        {
            return DepthMin + (sliderValue / 100.0) * (DepthMax - DepthMin);
        }
        
        private double ConvertSliderValueToClearance(int sliderValue)
        {
            return ClearanceMin + (sliderValue / 100.0) * (ClearanceMax - ClearanceMin);
        }
        
        private double ConvertSliderValueToAngle(int sliderValue)
        {
            return AngleMin + (sliderValue / 100.0) * (AngleMax - AngleMin);
        }

        private string GetJointTypeDisplayString(JointType jointType)
        {
            // Provide a user-friendly display string for each joint type  
            switch (jointType)
            {
                case JointType.MortiseAndTenon:
                    return "Mortise and Tenon";
                case JointType.Dovetail:
                    return "Dovetail";
                case JointType.HalfLap:
                    return "Half Lap";
                case JointType.FingerJoint:
                    return "Finger Joint";
                case JointType.BoxJoint:
                    return "Box Joint";
                case JointType.Bridle:
                    return "Bridle";
                default:
                    return "Unknown Joint";
            }
        }
    }
    
    /// <summary>
    /// Result class for joint selection
    /// </summary>
    public class JointSelectionResult
    {
        public JointType JointType { get; set; }
        public double Width { get; set; }
        public double Depth { get; set; }
        public double Clearance { get; set; }
        public double TailAngle { get; set; }
    }
}
