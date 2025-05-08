using Rhino;
using Rhino.Commands;
using Rhino.Input;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Linq;
using WoodJointsPlugin.Models;
using WoodJointsPlugin.Models.Joints;
using WoodJointsPlugin.UI;

namespace WoodJointsPlugin.Commands
{
    public class DetectJointCommand : Command
    {
        public DetectJointCommand()
        {
            Instance = this;
        }

        public static DetectJointCommand Instance { get; private set; }

        public override string EnglishName => "DetectJoint";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            try
            {
                // Enable verbose logging
                RhinoApp.WriteLine("Starting DetectJoint command...");

                // 1. Select two solids
                RhinoApp.WriteLine("Please select two solids to join");
                var rc = RhinoGet.GetMultipleObjects("Wybierz dwa solidy do po³¹czenia", false, ObjectType.Brep, out ObjRef[] objRefs);
                if (rc != Result.Success || objRefs == null || objRefs.Length != 2)
                {
                    RhinoApp.WriteLine("Musisz wybraæ dok³adnie dwa solidy.");
                    return Result.Failure;
                }

                var breps = objRefs.Select(r => r.Brep()).ToArray();
                if (breps[0] == null || breps[1] == null)
                {
                    RhinoApp.WriteLine("Wybrane obiekty nie s¹ solidami.");
                    return Result.Failure;
                }

                // Log object properties
                RhinoApp.WriteLine($"First solid: {breps[0].IsSolid} (valid={breps[0].IsValid})");
                RhinoApp.WriteLine($"Second solid: {breps[1].IsSolid} (valid={breps[1].IsValid})");

                // 2. Detect intersection
                RhinoApp.WriteLine("Obliczanie przeciêcia miêdzy bry³ami...");
                var tolerance = doc.ModelAbsoluteTolerance;
                RhinoApp.WriteLine($"Using tolerance: {tolerance}");

                var intersection = Rhino.Geometry.Brep.CreateBooleanIntersection(breps[0], breps[1], tolerance);
                if (intersection == null || intersection.Length == 0)
                {
                    RhinoApp.WriteLine("Brak przeciêcia miêdzy solidami.");

                    // Try a more direct approach to help diagnose the issue
                    var intersect = Rhino.Geometry.Intersect.Intersection.BrepBrep(breps[0], breps[1], tolerance,
                        out Rhino.Geometry.Curve[] curves, out Rhino.Geometry.Point3d[] points);
                    RhinoApp.WriteLine($"Direct intersection check: {intersect} curves: {(curves?.Length ?? 0)}, points: {(points?.Length ?? 0)}");

                    if ((curves?.Length ?? 0) > 0 || (points?.Length ?? 0) > 0)
                    {
                        RhinoApp.WriteLine("Intersection exists but boolean operation failed. Try adjusting the models.");
                    }

                    return Result.Nothing;
                }

                RhinoApp.WriteLine($"Znaleziono {intersection.Length} przeciêæ.");
                // Add intersection to document for debugging
                doc.Objects.AddBrep(intersection[0]);
                doc.Views.Redraw();
                RhinoApp.WriteLine("Added intersection to document for debugging");

                // 3. Use the enhanced dialog to select joint type and adjust parameters
                var enhancedDialog = new EnhancedJointSelectionDialog();
                var dialogResult = enhancedDialog.ShowModal(Rhino.UI.RhinoEtoApp.MainWindow);
                if (dialogResult == null)
                {
                    RhinoApp.WriteLine("Anulowano wybór typu po³¹czenia.");
                    return Result.Cancel;
                }

                // Extract all parameters from the result
                JointType jointType = dialogResult.JointType;
                double width = dialogResult.Width;
                double depth = dialogResult.Depth;
                double clearance = dialogResult.Clearance;
                double tailAngle = dialogResult.TailAngle;

                RhinoApp.WriteLine($"Wybrano typ po³¹czenia: {jointType.ToDisplayString()}");
                RhinoApp.WriteLine($"Parametry: szerokoœæ={width:0.0} mm, g³êbokoœæ={depth:0.0} mm, luz={clearance:0.00} mm" +
                                   (jointType == JointType.Dovetail ? $", k¹t={tailAngle:0.0}°" : ""));

                // 5. Create appropriate joint object
                BaseJoint joint = null;

                try
                {
                    RhinoApp.WriteLine("Tworzenie obiektu po³¹czenia...");

                    // Create joint directly instead of using factory to avoid any potential issues
                    switch (jointType)
                    {
                        case JointType.MortiseAndTenon:
                            joint = new MortiseAndTenon(breps[0], breps[1], intersection);
                            break;
                        case JointType.Dovetail:
                            joint = new DovetailJoint(breps[0], breps[1], intersection);
                            break;
                        default:
                            RhinoApp.WriteLine($"Typ po³¹czenia {jointType} nie jest jeszcze zaimplementowany.");
                            return Result.Failure;
                    }

                    if (joint == null)
                    {
                        RhinoApp.WriteLine($"Nie uda³o siê utworzyæ obiektu po³¹czenia typu {jointType}");
                        return Result.Failure;
                    }

                    // 6. Set joint parameters
                    joint.Parameters.Width = width;
                    joint.Parameters.Depth = depth;
                    joint.Parameters.Clearance = clearance;

                    // 7. Generate joint
                    RhinoApp.WriteLine("Generowanie geometrii po³¹czenia...");
                    var jointResult = joint.GenerateJoint(); // Renamed variable to avoid conflict
                    var modifiedFirst = jointResult.Item1;
                    var modifiedSecond = jointResult.Item2;

                    // 8. Update document
                    if (modifiedFirst != null && modifiedSecond != null)
                    {
                        doc.Objects.Replace(objRefs[0].ObjectId, modifiedFirst);
                        doc.Objects.Replace(objRefs[1].ObjectId, modifiedSecond);
                        doc.Views.Redraw();
                        RhinoApp.WriteLine($"Wygenerowano po³¹czenie typu {jointType.ToDisplayString()}!");
                        return Result.Success;
                    }
                    else
                    {
                        RhinoApp.WriteLine("Nie uda³o siê wygenerowaæ po³¹czenia - otrzymano null.");
                        return Result.Failure;
                    }
                }
                catch (Exception ex)
                {
                    RhinoApp.WriteLine($"B³¹d podczas generowania po³¹czenia: {ex.Message}");
                    RhinoApp.WriteLine($"Stack trace: {ex.StackTrace}");
                    return Result.Failure;
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Unexpected error: {ex.Message}");
                RhinoApp.WriteLine($"Stack trace: {ex.StackTrace}");
                return Result.Failure;
            }
        }
    }
}
