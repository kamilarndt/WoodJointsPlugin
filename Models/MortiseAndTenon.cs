using Rhino;
using Rhino.Geometry;
using System;

namespace WoodJointsPlugin.Models.Joints
{
    public class MortiseAndTenon : BaseJoint
    {
        public MortiseAndTenon(Brep firstSolid, Brep secondSolid, Brep[] intersection)
            : base(firstSolid, secondSolid, intersection)
        {
            RhinoApp.WriteLine("MortiseAndTenon joint created");
        }

        public override (Brep, Brep) GenerateJoint()
        {
            try
            {
                RhinoApp.WriteLine("Generating MortiseAndTenon joint...");
                
                // Validate inputs
                if (!FirstSolid.IsValid || !SecondSolid.IsValid)
                {
                    RhinoApp.WriteLine("Invalid input breps");
                    return (FirstSolid, SecondSolid);
                }
                
                if (!FirstSolid.IsSolid || !SecondSolid.IsSolid)
                {
                    RhinoApp.WriteLine("Input breps are not solids");
                    // Continue anyway as this might still work
                }
                
                // For T-shaped connections (as in the provided image), 
                // special handling is needed to determine orientation
                bool isVertical = DetectIfVerticalJoint();
                RhinoApp.WriteLine($"Detected vertical joint orientation: {isVertical}");
                
                // 1. Determine joint orientation based on intersection
                var jointPlane = GetJointPlane();
                if (isVertical)
                {
                    // Adjust for vertical joints (e.g., T-shaped connections)
                    RhinoApp.WriteLine("Adjusting plane for vertical T-shaped connection");
                    jointPlane = AdjustPlaneForVerticalJoint(jointPlane);
                }
                
                RhinoApp.WriteLine($"Joint plane origin: {jointPlane.Origin}, X: {jointPlane.XAxis}, Y: {jointPlane.YAxis}, Z: {jointPlane.ZAxis}");
                
                // 2. Calculate joint dimensions
                double width = Parameters.Width;
                double depth = Parameters.Depth;
                double clearance = Parameters.Clearance;
                double extension = GetJointExtension() * 0.8; // 80% of the joint dimension
                RhinoApp.WriteLine($"Joint dimensions: width={width}, depth={depth}, clearance={clearance}, extension={extension}");
                
                // 3. Create tenon geometry
                RhinoApp.WriteLine("Creating tenon geometry...");
                var tenonInterval1 = new Interval(-width/2, width/2);
                var tenonInterval2 = new Interval(-depth/2, depth/2);
                var tenonInterval3 = new Interval(-clearance/2, extension);
                
                RhinoApp.WriteLine($"Tenon intervals: [{tenonInterval1.Min}, {tenonInterval1.Max}], [{tenonInterval2.Min}, {tenonInterval2.Max}], [{tenonInterval3.Min}, {tenonInterval3.Max}]");
                
                var tenonBox = new Box(jointPlane, tenonInterval1, tenonInterval2, tenonInterval3);
                if (!tenonBox.IsValid)
                {
                    RhinoApp.WriteLine("Failed to create valid tenon box");
                    return (FirstSolid, SecondSolid);
                }
                
                var tenonBrep = tenonBox.ToBrep();
                if (tenonBrep == null || !tenonBrep.IsValid)
                {
                    RhinoApp.WriteLine("Failed to create valid tenon brep");
                    return (FirstSolid, SecondSolid);
                }
                
                // 4. Create mortise geometry (slightly larger than tenon for clearance)
                RhinoApp.WriteLine("Creating mortise geometry...");
                var mortiseInterval1 = new Interval(-width/2 - clearance, width/2 + clearance);
                var mortiseInterval2 = new Interval(-depth/2 - clearance, depth/2 + clearance);
                var mortiseInterval3 = new Interval(-extension, clearance/2);
                
                RhinoApp.WriteLine($"Mortise intervals: [{mortiseInterval1.Min}, {mortiseInterval1.Max}], [{mortiseInterval2.Min}, {mortiseInterval2.Max}], [{mortiseInterval3.Min}, {mortiseInterval3.Max}]");
                
                var mortiseBox = new Box(jointPlane, mortiseInterval1, mortiseInterval2, mortiseInterval3);
                if (!mortiseBox.IsValid)
                {
                    RhinoApp.WriteLine("Failed to create valid mortise box");
                    return (FirstSolid, SecondSolid);
                }
                
                var mortiseBrep = mortiseBox.ToBrep();
                if (mortiseBrep == null || !mortiseBrep.IsValid)
                {
                    RhinoApp.WriteLine("Failed to create valid mortise brep");
                    return (FirstSolid, SecondSolid);
                }
                
                // 5. Apply boolean operations
                RhinoApp.WriteLine("Performing boolean operations...");
                
                // Use a small tolerance value suitable for millimeters
                double tolerance = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
                RhinoApp.WriteLine($"Using tolerance: {tolerance}");
                
                RhinoApp.WriteLine("Creating mortise (boolean difference)...");
                Brep[] mortiseResult = Brep.CreateBooleanDifference(FirstSolid, mortiseBrep, tolerance);
                
                if (mortiseResult == null || mortiseResult.Length == 0)
                {
                    RhinoApp.WriteLine("Failed to create mortise - boolean difference operation failed");
                    return (FirstSolid, SecondSolid);
                }
                
                RhinoApp.WriteLine("Creating tenon (boolean intersection)...");
                Brep[] tenonResult = Brep.CreateBooleanIntersection(SecondSolid, tenonBrep, tolerance);
                
                if (tenonResult == null || tenonResult.Length == 0)
                {
                    RhinoApp.WriteLine("Failed to create tenon - boolean intersection operation failed");
                    return (FirstSolid, SecondSolid);
                }
                
                RhinoApp.WriteLine("Joint creation successful");
                return (mortiseResult[0], tenonResult[0]);
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error in MortiseAndTenon.GenerateJoint: {ex.Message}");
                RhinoApp.WriteLine($"Stack trace: {ex.StackTrace}");
                return (FirstSolid, SecondSolid);
            }
        }
        
        // Helper method to detect if this is a vertical T-shaped joint
        private bool DetectIfVerticalJoint()
        {
            try
            {
                // Get bounding boxes in world coordinates
                var firstBBox = FirstSolid.GetBoundingBox(true);
                var secondBBox = SecondSolid.GetBoundingBox(true);
                
                // Calculate height differences
                double firstHeight = firstBBox.Max.Z - firstBBox.Min.Z;
                double secondHeight = secondBBox.Max.Z - secondBBox.Min.Z;
                
                // If one piece is much taller than the other, it might be a vertical joint
                double heightRatio = Math.Max(firstHeight, secondHeight) / Math.Min(firstHeight, secondHeight);
                bool heightDifferenceSignificant = heightRatio > 2.0;
                
                RhinoApp.WriteLine($"Height analysis - First: {firstHeight}, Second: {secondHeight}, Ratio: {heightRatio}");
                
                // Also check if one is significantly above the other
                bool verticalOverlap = 
                    (firstBBox.Max.Z > secondBBox.Max.Z && firstBBox.Min.Z < secondBBox.Min.Z) ||
                    (secondBBox.Max.Z > firstBBox.Max.Z && secondBBox.Min.Z < firstBBox.Min.Z);
                
                return heightDifferenceSignificant || verticalOverlap;
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error in DetectIfVerticalJoint: {ex.Message}");
                return false;
            }
        }
        
        // Adjust the joint plane for vertical T-connections
        private Plane AdjustPlaneForVerticalJoint(Plane originalPlane)
        {
            try
            {
                var plane = originalPlane.Clone();
                
                // For T-shaped connections, the plane needs to be oriented 
                // so that the Z-axis points along the vertical piece
                
                // First check which piece is likely the vertical one
                var firstBBox = FirstSolid.GetBoundingBox(true);
                var secondBBox = SecondSolid.GetBoundingBox(true);
                
                double firstHeight = firstBBox.Max.Z - firstBBox.Min.Z;
                double secondHeight = secondBBox.Max.Z - secondBBox.Min.Z;
                
                bool firstIsVertical = firstHeight > secondHeight;
                
                if (firstIsVertical)
                {
                    // The mortise will be in the horizontal piece (second)
                    // Rotate to match Z-axis with the tall piece
                    plane.Rotate(Math.PI/2, plane.XAxis);
                }
                else
                {
                    // The mortise will be in the horizontal piece (first)
                    // Rotate to match Z-axis with the tall piece
                    plane.Rotate(Math.PI/2, plane.XAxis);
                }
                
                return plane;
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error in AdjustPlaneForVerticalJoint: {ex.Message}");
                return originalPlane;
            }
        }
    }
}
