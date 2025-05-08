using Rhino;
using Rhino.Geometry;
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

                // 2. Calculate joint dimensions based on intersection percent
                var bbox = Intersection[0].GetBoundingBox(true);
                double percent = Parameters.IntersectionPercent / 100.0;
                double clearance = Parameters.Clearance;

                // Dovetail width is percent of intersection width (X axis), centered
                double intersectionWidth = bbox.Max.X - bbox.Min.X;
                double dovetailWidth = intersectionWidth * percent;
                double dovetailDepth = bbox.Max.Y - bbox.Min.Y;
                double dovetailHeight = bbox.Max.Z - bbox.Min.Z;

                double centerX = (bbox.Min.X + bbox.Max.X) / 2.0;
                double minX = centerX - dovetailWidth / 2.0;
                double maxX = centerX + dovetailWidth / 2.0;

                double tailAngle = 15.0 * Math.PI / 180.0; // stały kąt 15°

                RhinoApp.WriteLine($"Dovetail parameters: width={dovetailWidth}, depth={dovetailDepth}, height={dovetailHeight}, tailAngle=15°, clearance={clearance}");

                // 3. Create the dovetail geometry (centered in intersection)
                Plane dovetailPlane = new Plane(new Point3d(centerX, (bbox.Min.Y + bbox.Max.Y) / 2.0, (bbox.Min.Z + bbox.Max.Z) / 2.0), jointPlane.XAxis, jointPlane.ZAxis);
                Brep dovetailBrep = CreateDovetailGeometry(dovetailPlane, dovetailWidth, dovetailDepth, tailAngle, dovetailHeight);
                if (dovetailBrep == null)
                {
                    RhinoApp.WriteLine("Failed to create dovetail geometry");
                    return (FirstSolid, SecondSolid);
                }

                // 4. Create the socket geometry (slightly larger for clearance)
                Brep socketBrep = CreateDovetailGeometry(dovetailPlane, dovetailWidth + clearance, dovetailDepth + clearance, tailAngle, dovetailHeight + clearance);
                if (socketBrep == null)
                {
                    RhinoApp.WriteLine("Failed to create socket geometry");
                    return (FirstSolid, SecondSolid);
                }

                // Debug visualization: add dovetail and socket to doc
                var doc = RhinoDoc.ActiveDoc;
                if (doc != null)
                {
                    var dovetailAttr = new Rhino.DocObjects.ObjectAttributes { ObjectColor = System.Drawing.Color.Red, ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject, Name = "DEBUG_Dovetail" };
                    var socketAttr = new Rhino.DocObjects.ObjectAttributes { ObjectColor = System.Drawing.Color.Yellow, ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject, Name = "DEBUG_Socket" };
                    doc.Objects.AddBrep(dovetailBrep, dovetailAttr);
                    doc.Objects.AddBrep(socketBrep, socketAttr);
                    doc.Views.Redraw();
                }

                // Log bounding boxes
                var dovetailBox = dovetailBrep.GetBoundingBox(true);
                var socketBox = socketBrep.GetBoundingBox(true);
                RhinoApp.WriteLine($"Dovetail bbox: min={dovetailBox.Min}, max={dovetailBox.Max}");
                RhinoApp.WriteLine($"Socket bbox: min={socketBox.Min}, max={socketBox.Max}");

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

        private Brep CreateDovetailGeometry(Plane plane, double width, double depth, double tailAngle, double height)
        {
            try
            {
                double widerWidth = width + 2 * height * Math.Tan(tailAngle);
                var points = new System.Collections.Generic.List<Point3d>
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
                var faces = new System.Collections.Generic.List<Brep>();
                var bottomFace = Brep.CreateFromCornerPoints(points[0], points[1], points[2], points[3], 0.01);
                if (bottomFace != null) faces.Add(bottomFace);
                var topFace = Brep.CreateFromCornerPoints(points[4], points[5], points[6], points[7], 0.01);
                if (topFace != null) faces.Add(topFace);
                var side1 = Brep.CreateFromCornerPoints(points[0], points[3], points[7], points[4], 0.01);
                if (side1 != null) faces.Add(side1);
                var side2 = Brep.CreateFromCornerPoints(points[1], points[5], points[6], points[2], 0.01);
                if (side2 != null) faces.Add(side2);
                var front = Brep.CreateFromCornerPoints(points[0], points[4], points[5], points[1], 0.01);
                if (front != null) faces.Add(front);
                var back = Brep.CreateFromCornerPoints(points[3], points[2], points[6], points[7], 0.01);
                if (back != null) faces.Add(back);
                if (faces.Count < 6)
                {
                    RhinoApp.WriteLine($"Failed to create some dovetail faces: only {faces.Count}/6 created");
                    return null;
                }
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
