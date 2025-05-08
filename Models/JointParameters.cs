using System;

namespace WoodJointsPlugin.Models
{
    public class JointParameters
    {
        // Common parameters for all joints
        public double Width { get; set; } = 20.0;
        public double Depth { get; set; } = 30.0;
        public double Clearance { get; set; } = 0.1;
        
        // Specialized parameters
        public double TailAngle { get; set; } = 15.0; // For dovetail joints
        public int NumberOfFingers { get; set; } = 3;  // For finger and box joints
        
        public JointParameters Clone()
        {
            return new JointParameters
            {
                Width = this.Width,
                Depth = this.Depth,
                Clearance = this.Clearance,
                TailAngle = this.TailAngle,
                NumberOfFingers = this.NumberOfFingers
            };
        }
    }
}
