using Updater.Classes;

namespace Updater
{
    public partial class AppVariables
    {
        //Windows
        public static WindowMain WindowMain = null;

        //Settings
        public static UpdaterJson UpdaterSettings = new UpdaterJson();

        //Startup
        public static string[] StartupArguments = { };
    }
}