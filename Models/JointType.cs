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
                case JointType.Dovetail: return "Jaskó³czy ogon";
                case JointType.HalfLap: return "Po³¹czenie na pó³ drewna";
                case JointType.FingerJoint: return "Po³¹czenie palcowe";
                case JointType.BoxJoint: return "Po³¹czenie na jaskó³czy ogon prosty";
                case JointType.Bridle: return "Po³¹czenie czopowe otwarte";
                default: return type.ToString();
            }
        }
    }
}
