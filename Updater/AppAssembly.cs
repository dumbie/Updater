using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Updater
{
    public partial class App
    {
        public static Assembly AssemblyResolveEmbedded(object sender, ResolveEventArgs args)
        {
            try
            {
                string fileName = args.Name.Split(',')[0] + ".dll";
                string assemblyPath = Assembly.GetEntryAssembly().GetName().Name + ".Assembly." + fileName;
                byte[] fileBytes = EmbeddedResourceToBytes(assemblyPath);
                Debug.WriteLine("Resolving embedded assembly dll: " + assemblyPath);
                return Assembly.Load(fileBytes);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed resolving assembly dll: " + ex.Message);
                return null;
            }
        }

        public static byte[] EmbeddedResourceToBytes(string fileName)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (Stream fileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName))
                    {
                        fileStream.CopyTo(memoryStream);
                        return memoryStream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to convert embedded resource: " + ex.Message);
                return null;
            }
        }
    }
}