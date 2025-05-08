using Rhino;
using Rhino.Commands;

namespace WoodJointsPlugin.Commands
{
    public class HelloCommand : Command
    {
        public HelloCommand()
        {
            Instance = this;
        }

        public static HelloCommand Instance { get; private set; }

        public override string EnglishName => "HelloCommand";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            RhinoApp.WriteLine("Hello from WoodJointsPlugin!");
            return Result.Success;
        }
    }
} 