using Rhino;
using Rhino.Geometry;
using System.Collections.Generic;
using System;

namespace WoodJointsPlugin.Models.Joints
{
    public class DovetailJoint : BaseJoint
    {
        public DovetailJoint(Brep firstSolid, Brep secondSolid, Brep[] intersection)
            : base(firstSolid, secondSolid, intersection)
        {
            RhinoApp.WriteLine("DovetailJoint created");
        }

        public override (Brep, Brep) GenerateJoint()
        {
            try
            {
                // 1. Determine joint orientation
                var jointPlane = GetJointPlane();
                RhinoApp.WriteLine("Dovetail joint plane origin: " + jointPlane.Origin.ToString());

                // 2. Calculate joint dimensions
                double width = Parameters.Width;
                double depth = Parameters.Depth;
                double clearance = Parameters.Clearance;
                double tailAngle = Parameters.TailAngle * Math.PI / 180.0; // Convert to radians
                double extension = GetJointExtension() * 0.8;

                RhinoApp.WriteLine($"Dovetail parameters: width={width}, depth={depth}, tailAngle={Parameters.TailAngle}°, extension={extension}");

                // 3. Create the dovetail geometry
                Brep dovetailBrep = CreateDovetailGeometry(jointPlane, width, depth, tailAngle, extension);
                if (dovetailBrep == null)
                {
                    RhinoApp.WriteLine("Failed to create dovetail geometry");
                    return (FirstSolid, SecondSolid);
                }

                // 4. Create the socket geometry (slightly larger for clearance)
                Brep socketBrep = CreateDovetailGeometry(jointPlane, width + clearance * 2, depth + clearance * 2,
                                                       tailAngle, extension + clearance);
                if (socketBrep == null)
                {
                    RhinoApp.WriteLine("Failed to create socket geometry");
                    return (FirstSolid, SecondSolid);
                }

                // 5. Apply boolean operations
                RhinoApp.WriteLine("Performing boolean operations for dovetail joint...");
                double tolerance = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

                Brep[] socketResult = Brep.CreateBooleanDifference(FirstSolid, socketBrep, tolerance);
                if (socketResult == null || socketResult.Length == 0)
                {
                    RhinoApp.WriteLine("Failed to create socket");
                    return (FirstSolid, SecondSolid);
                }

                Brep[] tailResult = Brep.CreateBooleanIntersection(SecondSolid, dovetailBrep, tolerance);
                if (tailResult == null || tailResult.Length == 0)
                {
                    RhinoApp.WriteLine("Failed to create tail");
                    return (FirstSolid, SecondSolid);
                }

                RhinoApp.WriteLine("Dovetail joint creation successful");
                return (socketResult[0], tailResult[0]);
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error in DovetailJoint.GenerateJoint: {ex.Message}");
                RhinoApp.WriteLine($"Stack trace: {ex.StackTrace}");
                return (FirstSolid, SecondSolid);
            }
        }


        // Removed the incomplete and invalid line
        // No changes to the rest of the code
        private Brep CreateDovetailGeometry(Plane plane, double width, double depth, double tailAngle, double height)
        {
            try
            {
                // Calculate the wider part of the dovetail based on the tail angle
                double widerWidth = width + 2 * height * Math.Tan(tailAngle);

                // Create points for the dovetail shape
                var points = new List<Point3d>
                {
                    plane.PointAt(-width/2, -depth/2, 0),
                    plane.PointAt(width/2, -depth/2, 0),
                    plane.PointAt(width/2, depth/2, 0),
                    plane.PointAt(-width/2, depth/2, 0),
                    plane.PointAt(-widerWidth/2, -depth/2, height),
                    plane.PointAt(widerWidth/2, -depth/2, height),
                    plane.PointAt(widerWidth/2, depth/2, height),
                    plane.PointAt(-widerWidth/2, depth/2, height)
                };

                // Create faces for the dovetail
                var faces = new List<Brep>();

                // Bottom face
                var bottomFace = Brep.CreateFromCornerPoints(
                    points[0], points[1], points[2], points[3], 0.01);
                if (bottomFace != null) faces.Add(bottomFace);

                // Top face
                var topFace = Brep.CreateFromCornerPoints(
                    points[4], points[5], points[6], points[7], 0.01);
                if (topFace != null) faces.Add(topFace);

                // Side faces
                var side1 = Brep.CreateFromCornerPoints(
                    points[0], points[3], points[7], points[4], 0.01);
                if (side1 != null) faces.Add(side1);

                var side2 = Brep.CreateFromCornerPoints(
                    points[1], points[5], points[6], points[2], 0.01);
                if (side2 != null) faces.Add(side2);

                // Front and back faces
                var front = Brep.CreateFromCornerPoints(
                    points[0], points[4], points[5], points[1], 0.01);
                if (front != null) faces.Add(front);

                var back = Brep.CreateFromCornerPoints(
                    points[3], points[2], points[6], points[7], 0.01);
                if (back != null) faces.Add(back);

                if (faces.Count < 6)
                {
                    RhinoApp.WriteLine($"Failed to create some dovetail faces: only {faces.Count}/6 created");
                    return null;
                }

                // Join all faces into a single solid
                var joinedBreps = Brep.JoinBreps(faces, 0.01);
                if (joinedBreps == null || joinedBreps.Length == 0)
                {
                    RhinoApp.WriteLine("Failed to join dovetail faces");
                    return null;
                }

                return joinedBreps[0];
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error in CreateDovetailGeometry: {ex.Message}");
                RhinoApp.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }
    }
}
