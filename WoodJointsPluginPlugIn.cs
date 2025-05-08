using Rhino.PlugIns;

namespace WoodJointsPlugin
{
    public class WoodJointsPluginPlugIn : PlugIn
    {
        public WoodJointsPluginPlugIn()
        {
            Instance = this;
        }

        public static WoodJointsPluginPlugIn Instance { get; private set; }
    }
} 