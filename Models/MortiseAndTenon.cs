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
            // Initialize any additional properties specific to MortiseAndTenon
            RhinoApp.WriteLine("MortiseAndTenon joint created");
        }

        public override (Brep, Brep) GenerateJoint()
        {
            try
            {
                // 1. Determine joint orientation
                var jointPlane = GetJointPlane();
                RhinoApp.WriteLine("Mortise and tenon joint plane origin: " + jointPlane.Origin.ToString());

                // 2. Calculate joint dimensions based on intersection percent
                var bbox = Intersection[0].GetBoundingBox(true);
                double percent = Parameters.IntersectionPercent / 100.0;

                // Tenon width is percent of intersection width (X axis)
                double intersectionWidth = bbox.Max.X - bbox.Min.X;
                double tenonWidth = intersectionWidth * percent;
                double tenonDepth = bbox.Max.Y - bbox.Min.Y;
                double tenonHeight = bbox.Max.Z - bbox.Min.Z;

                double minX, maxX;
                if (Parameters.PositionMode == TenonPositionMode.Centered)
                {
                    double centerX = (bbox.Min.X + bbox.Max.X) / 2.0;
                    minX = centerX - tenonWidth / 2.0;
                    maxX = centerX + tenonWidth / 2.0;
                }
                else // Edge
                {
                    minX = bbox.Min.X;
                    maxX = bbox.Min.X + tenonWidth;
                }

                // Clearance for mortise
                double clearance = Parameters.Clearance;

                // Tenon box
                var tenonBox = new BoundingBox(
                    new Point3d(minX, bbox.Min.Y, bbox.Min.Z),
                    new Point3d(maxX, bbox.Max.Y, bbox.Max.Z)
                );
                Brep tenonBrep = Brep.CreateFromBox(tenonBox);
                if (tenonBrep == null)
                {
                    RhinoApp.WriteLine("Failed to create tenon geometry");
                    return (FirstSolid, SecondSolid);
                }

                // Mortise box (slightly larger for clearance)
                var mortiseBox = new BoundingBox(
                    new Point3d(minX - clearance/2, bbox.Min.Y - clearance/2, bbox.Min.Z - clearance/2),
                    new Point3d(maxX + clearance/2, bbox.Max.Y + clearance/2, bbox.Max.Z + clearance/2)
                );
                Brep mortiseBrep = Brep.CreateFromBox(mortiseBox);
                if (mortiseBrep == null)
                {
                    RhinoApp.WriteLine("Failed to create mortise geometry");
                    return (FirstSolid, SecondSolid);
                }

                // 3. Boolean operations
                RhinoApp.WriteLine("Performing boolean operations for mortise and tenon joint...");
                double tolerance = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

                Brep[] mortiseResult = Brep.CreateBooleanDifference(FirstSolid, mortiseBrep, tolerance);
                if (mortiseResult == null || mortiseResult.Length == 0)
                {
                    RhinoApp.WriteLine("Failed to create mortise");
                    return (FirstSolid, SecondSolid);
                }

                Brep[] tenonResult = Brep.CreateBooleanIntersection(SecondSolid, tenonBrep, tolerance);
                if (tenonResult == null || tenonResult.Length == 0)
                {
                    RhinoApp.WriteLine("Failed to create tenon");
                    return (FirstSolid, SecondSolid);
                }

                RhinoApp.WriteLine("Mortise and tenon joint creation successful");
                return (mortiseResult[0], tenonResult[0]);
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error in MortiseAndTenon.GenerateJoint: {ex.Message}");
                RhinoApp.WriteLine($"Stack trace: {ex.StackTrace}");
                return (FirstSolid, SecondSolid);
            }
        }

        private Brep CreateTenonGeometry(Plane plane, double width, double depth, double height)
        {
            try
            {
                // Create points for the tenon shape (rectangular)
                var points = new System.Collections.Generic.List<Point3d>
                {
                    plane.PointAt(-width/2, -depth/2, 0),
                    plane.PointAt(width/2, -depth/2, 0),
                    plane.PointAt(width/2, depth/2, 0),
                    plane.PointAt(-width/2, depth/2, 0),
                    plane.PointAt(-width/2, -depth/2, height),
                    plane.PointAt(width/2, -depth/2, height),
                    plane.PointAt(width/2, depth/2, height),
                    plane.PointAt(-width/2, depth/2, height)
                };

                // Create faces for the tenon
                var faces = new System.Collections.Generic.List<Brep>();

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
                    RhinoApp.WriteLine($"Failed to create some tenon faces: only {faces.Count}/6 created");
                    return null;
                }

                // Join all faces into a single solid
                var joinedBreps = Brep.JoinBreps(faces, 0.01);
                if (joinedBreps == null || joinedBreps.Length == 0)
                {
                    RhinoApp.WriteLine("Failed to join tenon faces");
                    return null;
                }

                return joinedBreps[0];
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error in CreateTenonGeometry: {ex.Message}");
                RhinoApp.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        // Generate the tenon geometry on the local plane of the intersection (prostopadły do powierzchni poziomego elementu)
        public Brep GenerateTenonOnLocalPlane()
        {
            try
            {
                var bbox = Intersection[0].GetBoundingBox(true);
                double percent = Parameters.IntersectionPercent / 100.0;
                double intersectionWidth = bbox.Max.X - bbox.Min.X;
                double tenonWidth = intersectionWidth * percent;
                double tenonDepth = bbox.Max.Y - bbox.Min.Y;
                double tenonHeight = bbox.Max.Z - bbox.Min.Z;

                // Center of intersection
                var center = bbox.Center;

                // Find closest face of SecondSolid to center
                BrepFace closestFace = null;
                double minDist = double.MaxValue;
                double closestU = 0, closestV = 0;
                foreach (var face in SecondSolid.Faces)
                {
                    double u, v;
                    if (!face.ClosestPoint(center, out u, out v))
                        continue;
                    var pt = face.PointAt(u, v);
                    double dist = pt.DistanceTo(center);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestFace = face;
                        closestU = u;
                        closestV = v;
                    }
                }
                if (closestFace == null)
                {
                    RhinoApp.WriteLine("Nie znaleziono najbliższej powierzchni do intersection center");
                    return null;
                }
                // Normal at closest point
                var normal = closestFace.NormalAt(closestU, closestV);
                normal.Unitize();
                // Build local plane (origin=center, normal=face normal)
                Plane tenonPlane = new Plane(center, normal);

                // Tenon width/height/depth logic as in GenerateJoint
                double minX, maxX;
                if (Parameters.PositionMode == TenonPositionMode.Centered)
                {
                    double centerX = (bbox.Min.X + bbox.Max.X) / 2.0;
                    minX = centerX - tenonWidth / 2.0;
                    maxX = centerX + tenonWidth / 2.0;
                }
                else // Edge
                {
                    minX = bbox.Min.X;
                    maxX = bbox.Min.X + tenonWidth;
                }
                // For simplicity, create the box at the origin of tenonPlane
                Brep tenonBrep = CreateTenonGeometry(tenonPlane, tenonWidth, tenonDepth, tenonHeight);
                if (tenonBrep == null)
                {
                    RhinoApp.WriteLine("Failed to create tenon geometry on local plane");
                    return null;
                }
                return tenonBrep;
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Error in GenerateTenonOnLocalPlane: {ex.Message}");
                RhinoApp.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }
    }
}
