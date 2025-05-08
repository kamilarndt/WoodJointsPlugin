using System;

namespace WoodJointsPlugin.Models
{
    public class JointParameters
    {
        // Uproszczony parametr procentowy
        public double IntersectionPercent { get; set; } = 50.0;
        public double Clearance { get; set; } = 0.5;
        public TenonPositionMode PositionMode { get; set; } = TenonPositionMode.Centered;

        public JointParameters Clone()
        {
            return new JointParameters
            {
                IntersectionPercent = this.IntersectionPercent,
                Clearance = this.Clearance,
                PositionMode = this.PositionMode
            };
        }
    }
}
