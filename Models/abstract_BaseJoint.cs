using Rhino;
using Rhino.Geometry;
using System;

namespace WoodJointsPlugin.Models.Joints
{
    public abstract class BaseJoint
    {
        // Common properties
        public JointParameters Parameters { get; set; } = new JointParameters();
        
        // Reference to the two solids
        protected Brep FirstSolid { get; set; }
        protected Brep SecondSolid { get; set; }
        protected Brep[] Intersection { get; set; }
        
        // Constructor
        public BaseJoint(Brep firstSolid, Brep secondSolid, Brep[] intersection)
        {
            if (firstSolid == null)
                throw new ArgumentNullException(nameof(firstSolid));
            if (secondSolid == null)
                throw new ArgumentNullException(nameof(secondSolid));
            if (intersection == null)
                throw new ArgumentNullException(nameof(intersection));
            if (intersection.Length == 0)
                throw new ArgumentException("Intersection array cannot be empty", nameof(intersection));
            
            FirstSolid = firstSolid;
            SecondSolid = secondSolid;
            Intersection = intersection;
        }

        // Abstract method to be implemented by each specific joint
        public abstract (Brep, Brep) GenerateJoint();
        
        // Helper methods for joint positioning, orientation detection, etc.
        protected Plane GetJointPlane()
        {
            RhinoApp.WriteLine("Getting joint plane...");
            
            try
            {    
                // Get the bounding box of the intersection
                var bbox = Intersection[0].GetBoundingBox(true);
                
                // Log dimensions
                RhinoApp.WriteLine($"Intersection bbox min: {bbox.Min}, max: {bbox.Max}");
                
                // Create a plane at the center of the intersection
                var plane = Plane.WorldXY;
                plane.Origin = bbox.Center;
                
                // Try to determine the main direction of the intersection
                var dimensions = new double[] 
                {
                    Math.Abs(bbox.Max.X - bbox.Min.X),
                    Math.Abs(bbox.Max.Y - bbox.Min.Y),
                    Math.Abs(bbox.Max.Z - bbox.Min.Z)
                };
                
                RhinoApp.WriteLine($"Intersection dimensions: X={dimensions[0]}, Y={dimensions[1]}, Z={dimensions[2]}");
                
                // Find the smallest dimension (which should be perpendicular to the joint face)
                int minDimIndex = 0;
                double minDim = dimensions[0];
                for (int i = 1; i < dimensions.Length; i++)
                {
                    if (dimensions[i] < minDim)
                    {
                        minDim = dimensions[i];
                        minDimIndex = i;
                    }
                }
                
                RhinoApp.WriteLine($"Minimum dimension: {minDimIndex} (value={minDim})");
                
                // Check if we have a very thin intersection that might cause issues
                double tolerance = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance * 10;
                if (minDim < tolerance)
                {
                    RhinoApp.WriteLine($"Warning: Very thin intersection ({minDim} < {tolerance})");
                }
                
                // Rotate the plane to align with the joint direction
                switch (minDimIndex)
                {
                    case 0: // X is smallest, so joint is in YZ plane
                        RhinoApp.WriteLine("Rotating plane for YZ alignment");
                        plane.Rotate(Math.PI/2, Vector3d.YAxis);
                        break;
                    case 1: // Y is smallest, so joint is in XZ plane
                        RhinoApp.WriteLine("Rotating plane for XZ alignment");
                        plane.Rotate(Math.PI/2, Vector3d.XAxis);
                        break;
                    default: // Z is smallest, already in XY plane, no rotation needed
                        RhinoApp.WriteLine("No rotation needed, already in XY plane");
                        break;
                }
                
                return plane;
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error in GetJointPlane: {ex.Message}");
                RhinoApp.WriteLine("Falling back to default plane");
                return Plane.WorldXY;
            }
        }
        
        // Calculate how far the joint should extend
        protected double GetJointExtension()
        {
            try
            {
                // Get bounds of both solids in the joint direction
                var plane = GetJointPlane();
                var firstBBox = FirstSolid.GetBoundingBox(plane);
                var secondBBox = SecondSolid.GetBoundingBox(plane);
                
                // Log dimensions
                RhinoApp.WriteLine($"First solid Z range: {firstBBox.Min.Z} to {firstBBox.Max.Z}");
                RhinoApp.WriteLine($"Second solid Z range: {secondBBox.Min.Z} to {secondBBox.Max.Z}");
                
                // Estimate a reasonable extension based on the Z dimension of the bounding box
                double firstExtent = Math.Abs(firstBBox.Max.Z - firstBBox.Min.Z);
                double secondExtent = Math.Abs(secondBBox.Max.Z - secondBBox.Min.Z);
                double extension = Math.Min(firstExtent, secondExtent) * 0.5; // Use 50% of the smaller dimension
                
                RhinoApp.WriteLine($"Calculated joint extension: {extension}");
                
                // Ensure a minimum reasonable size
                double minExtension = 10.0; // 10mm minimum extension
                if (extension < minExtension)
                {
                    RhinoApp.WriteLine($"Extension too small ({extension}), using minimum value ({minExtension})");
                    extension = minExtension;
                }
                
                return extension;
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error in GetJointExtension: {ex.Message}");
                RhinoApp.WriteLine($"Stack trace: {ex.StackTrace}");
                return 30.0; // Default fallback value
            }
        }
    }
}
