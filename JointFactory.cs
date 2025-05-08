using WoodJointsPlugin.Models;
using WoodJointsPlugin.Models.Joints;
using Rhino.Geometry;

namespace WoodJointsPlugin
{
    public static class JointFactory
    {
        public static BaseJoint CreateJoint(JointType jointType, Brep firstSolid, Brep secondSolid, Brep[] intersection)
        {
            switch (jointType)
            {
                case JointType.MortiseAndTenon:
                    return new MortiseAndTenon(firstSolid, secondSolid, intersection);
                    
                case JointType.Dovetail:
                    return new DovetailJoint(firstSolid, secondSolid, intersection);
                    
                // Add other joint types as implemented
                    
                default:
                    return null;
            }
        }
    }
}
