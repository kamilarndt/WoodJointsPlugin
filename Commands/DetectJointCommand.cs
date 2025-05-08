using Rhino;
using Rhino.Commands;
using Rhino.Input;
using Rhino.DocObjects;
using System.Linq;

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
            // 1. Wybór dwóch solidów
            var rc = RhinoGet.GetMultipleObjects("Wybierz dwa solidy do połączenia", false, ObjectType.Brep, out ObjRef[] objRefs);
            if (rc != Result.Success || objRefs == null || objRefs.Length != 2)
            {
                RhinoApp.WriteLine("Musisz wybrać dokładnie dwa solidy.");
                return Result.Failure;
            }

            var breps = objRefs.Select(r => r.Brep()).ToArray();
            if (breps[0] == null || breps[1] == null)
            {
                RhinoApp.WriteLine("Wybrane obiekty nie są solidami.");
                return Result.Failure;
            }

            // 2. Wykrycie przecięcia
            var intersection = Rhino.Geometry.Brep.CreateBooleanIntersection(breps[0], breps[1], 0.01);
            if (intersection == null || intersection.Length == 0)
            {
                RhinoApp.WriteLine("Brak przecięcia między solidami.");
                return Result.Nothing;
            }

            // 3. Panel wyboru typu połączenia
            var panel = new UI.JointSelectionPanel();
            var result = panel.ShowModal(Rhino.UI.RhinoEtoApp.MainWindow);
            if (string.IsNullOrEmpty(result))
            {
                RhinoApp.WriteLine("Anulowano wybór typu połączenia.");
                return Result.Cancel;
            }
            RhinoApp.WriteLine($"Wybrano typ połączenia: {result}");

            // 4. Panel parametrów połączenia
            var paramDialog = new UI.JointParameterDialog();
            var paramResult = paramDialog.ShowModal(Rhino.UI.RhinoEtoApp.MainWindow);
            if (paramResult == null)
            {
                RhinoApp.WriteLine("Anulowano ustawianie parametrów połączenia.");
                return Result.Cancel;
            }
            var (width, depth, clearance) = paramResult.Value;
            RhinoApp.WriteLine($"Parametry: szerokość={width} mm, głębokość={depth} mm, luz={clearance} mm");

            // 5. Podsumowanie (na razie tylko komunikat)
            RhinoApp.WriteLine($"[DEMO] Generowanie połączenia typu {result} z parametrami: szerokość={width}, głębokość={depth}, luz={clearance}");

            return Result.Success;
        }
    }
} 