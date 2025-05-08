using System;

namespace WoodJointsPlugin.Models
{
    public enum JointType
    {
        MortiseAndTenon,
        Dovetail,
        HalfLap,
        FingerJoint,
        BoxJoint,
        Bridle
    }
    
    public static class JointTypeExtensions
    {
        public static string ToDisplayString(this JointType type)
        {
            switch (type)
            {
                case JointType.MortiseAndTenon: return "Czop";
                case JointType.Dovetail: return "Jask�czy ogon";
                case JointType.HalfLap: return "Po��czenie na p� drewna";
                case JointType.FingerJoint: return "Po��czenie palcowe";
                case JointType.BoxJoint: return "Po��czenie na jask�czy ogon prosty";
                case JointType.Bridle: return "Po��czenie czopowe otwarte";
                default: return type.ToString();
            }
        }
    }
}
