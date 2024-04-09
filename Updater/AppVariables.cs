using Updater.Classes;

namespace Updater
{
    public partial class AppVariables
    {
        //Windows
        public static WindowMain WindowMain = new WindowMain();

        //Settings
        public static UpdaterJson UpdaterSettings = new UpdaterJson();

        //Startup
        public static string[] StartupArguments = { };
    }
}