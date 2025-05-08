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
                RhinoApp.WriteLine("Starting DetectJoint command...");

                // 1. User selects all solids to join
                doc.Objects.UnselectAll();
                doc.Views.Redraw();
                RhinoApp.WriteLine("Zaznacz wszystkie elementy do połączenia (np. nogi, belki, półki). Po zakończeniu wyboru naciśnij Enter.");
                var rc = RhinoGet.GetMultipleObjects("Zaznacz wszystkie elementy do połączenia. Po zakończeniu wyboru naciśnij Enter.", true, ObjectType.Brep, out ObjRef[] objRefs);
                if (rc != Result.Success || objRefs == null || objRefs.Length < 2)
                {
                    RhinoApp.WriteLine("Musisz wybrać co najmniej dwa elementy.");
                    return Result.Failure;
                }

                // 2. Dialog z parametrami połączenia
                var enhancedDialog = new EnhancedJointSelectionDialog();
                var dialogResult = enhancedDialog.ShowModal(Rhino.UI.RhinoEtoApp.MainWindow);
                if (dialogResult == null)
                {
                    RhinoApp.WriteLine("Anulowano wybór parametrów połączenia.");
                    return Result.Cancel;
                }
                double intersectionPercent = dialogResult.IntersectionPercent;
                double clearance = dialogResult.Clearance;
                TenonPositionMode positionMode = dialogResult.PositionMode;

                // 3. Przygotuj listę brepów i ich id
                var solids = objRefs.Select(r => r.Brep()).ToList();
                var ids = objRefs.Select(r => r.ObjectId).ToList();
                var tolerance = doc.ModelAbsoluteTolerance;

                // 4. Dla każdej unikalnej pary (A, B) generuj połączenie
                for (int i = 0; i < solids.Count; i++)
                {
                    for (int j = 0; j < solids.Count; j++)
                    {
                        if (i == j) continue;
                        var solidA = solids[i];
                        var solidB = solids[j];
                        var intersection = Brep.CreateBooleanIntersection(solidA, solidB, tolerance);
                        if (intersection == null || intersection.Length == 0) continue;

                        // --- Wyznacz plane intersection względem solidA ---
                        var joint = new MortiseAndTenon(solidA, solidB, intersection);
                        joint.Parameters.IntersectionPercent = intersectionPercent;
                        joint.Parameters.Clearance = clearance;
                        joint.Parameters.PositionMode = positionMode;
                        var tenonBrep = joint.GenerateTenonOnLocalPlane();
                        if (tenonBrep == null)
                        {
                            RhinoApp.WriteLine($"Nie udało się wygenerować czopa dla pary {i}-{j}");
                            continue;
                        }
                        // --- BooleanUnion czopa z solidA ---
                        var union = Brep.CreateBooleanUnion(new[] { solidA, tenonBrep }, tolerance);
                        if (union != null && union.Length > 0)
                        {
                            solids[i] = union[0];
                        }
                        else
                        {
                            RhinoApp.WriteLine($"Nie udało się połączyć czopa z elementem {i}");
                        }
                        // --- BooleanDifference czopa od solidB (wpust) ---
                        var diff = Brep.CreateBooleanDifference(solidB, tenonBrep, tolerance);
                        if (diff != null && diff.Length > 0)
                        {
                            solids[j] = diff[0];
                        }
                        else
                        {
                            RhinoApp.WriteLine($"Nie udało się wyciąć wpustu w elemencie {j}");
                        }
                    }
                }

                // 5. Podmień wszystkie oryginalne obiekty na zmodyfikowane
                for (int i = 0; i < ids.Count; i++)
                {
                    if (solids[i] != null)
                        doc.Objects.Replace(ids[i], solids[i]);
                }
                doc.Views.Redraw();
                RhinoApp.WriteLine("Wszystkie połączenia wygenerowane. Pozostały tylko docelowe elementy.");
                return Result.Success;
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
