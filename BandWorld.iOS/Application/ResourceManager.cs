using System;
using System.IO;
using BandWorld.MVC;
using BandWorld.MVC.Application;
using JTRazorPortable;

namespace BandWorld.iOS
{
    public class ResourceManager
    {
        public ResourceManager()
        {
        }

        public static void EnsureResources(System.Reflection.Assembly assembly, string dataPath, bool forceCopy, bool refresh)
        {
            string mediaVersionFile = dataPath + "/" + "mediaVersion.txt";
            string mediaVersion = String.Empty;
            if (!refresh)
            {
                try
                {
                    if (ApplicationData.Global.FileExists(mediaVersionFile))
                    {
                        mediaVersion = ApplicationData.Global.FileReadAllText(mediaVersionFile);
                        if ((mediaVersion == ApplicationData.MediaVersion) && !forceCopy)
                            return;
                    }
                }
                catch (Exception)
                {
                }
            }

            string assemblyName = assembly.GetName().Name + ".";
            string contentIdentifier = "Content.";
            string scriptIdentifier = "Scripts.";
            string dataIdentifier = "App_Data.";
            string[] resources = assembly.GetManifestResourceNames();
            string fileName;
            int count = resources.Length;
            int index = 0;

            ApplicationData.Global.ProgressOperation_Dispatch(ProgressMode.Start, count, "Copying resources...");

            foreach (var resource in resources)
            {
                string path = resource.Substring(assemblyName.Length);
                if (path.StartsWith(contentIdentifier))
                {
                    // Treat Content resources as though every "." except the last is a directory separator
                    var lastDot = path.LastIndexOf(".");
                    if (lastDot > -1)
                        path = path.Substring(0, lastDot).Replace('.', Path.DirectorySeparatorChar) + "." + path.Substring(lastDot + 1);
                    else
                        path = path.Replace('.', Path.DirectorySeparatorChar);
                    // Hack to handle Lex.Db
                    path = path.Replace("Lex/Db", "Lex.Db");
                    path = path.Replace("/min", ".min");
                    path = Path.Combine(dataPath, path);
                    fileName = ApplicationData.Global.GetFileName(path);
                    ApplicationData.Global.ProgressOperation_Dispatch(ProgressMode.Update, index++, fileName);
                    EnsureResource(assembly, path, resource, forceCopy);
                }
                else if (path.StartsWith(scriptIdentifier))
                {
                    path = "Scripts/" + path.Substring(scriptIdentifier.Length);
                    path = Path.Combine(dataPath, path);
                    fileName = ApplicationData.Global.GetFileName(path);
                    ApplicationData.Global.ProgressOperation_Dispatch(ProgressMode.Update, index++, fileName);
                    EnsureResource(assembly, path, resource, forceCopy);
                }
                else if (path.StartsWith(dataIdentifier))
                {
                    path = "App_Data/" + path.Substring(dataIdentifier.Length);
                    path = Path.Combine(dataPath, path);
                    fileName = ApplicationData.Global.GetFileName(path);
                    ApplicationData.Global.ProgressOperation_Dispatch(ProgressMode.Update, index++, fileName);
                    EnsureResource(assembly, path, resource, forceCopy);

                    /*
                    if (path.EndsWith(".zip"))
                    {
                        try
                        {
                            IArchiveFile zipFile = zipFile = FileSingleton.Archive();

                            if (zipFile.Create(path))
                            {
                                zipFile.Extract(dataPath, forceCopy, null);
                                zipFile.Close();
                            }

                            zipFile = null;
                        }
                        catch (Exception exc)
                        {
                            string msg = "Exception during zip: " + exc.Message;
                            if (exc.InnerException != null)
                                msg += exc.InnerException.Message;
                            throw new Exception(msg);
                        }
                        finally
                        {
                            try
                            {
                                FileSingleton.Delete(path);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    */
                }
            }

            try
            {
                if (mediaVersion != ApplicationDataPlatform.MediaVersion)
                {
                    fileName = ApplicationData.Global.GetFileName(mediaVersionFile);
                    ApplicationData.Global.ProgressOperation_Dispatch(ProgressMode.Update, index++, fileName);
                    ApplicationData.Global.FileWriteAllText(mediaVersionFile, ApplicationDataPlatform.MediaVersion);
                }
            }
            catch (Exception)
            {
            }

            ApplicationData.Global.ProgressOperation_Dispatch(ProgressMode.Stop, count, null);
        }

        public static void EnsureResource(System.Reflection.Assembly assembly, string fileName, string resource,
            bool forceCopy)
        {
            ApplicationData.Global.DirectoryExistsCheck(fileName);

            if (forceCopy)
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
            else
            {
                if (File.Exists(fileName))
                    return;
            }

            var directoryName = fileName.Substring(0, fileName.LastIndexOf("/"));
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            var input = ResourceLoader.GetEmbeddedResourceStream(assembly, resource);
            using (var output = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                byte[] buffer = new byte[1024];
                int length;
                while ((length = input.Read(buffer, 0, 1024)) > 0)
                    output.Write(buffer, 0, length);
                output.Flush();
                output.Close();
                input.Close();
            }
        }
    }
}