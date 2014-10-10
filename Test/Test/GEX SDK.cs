using System;
using System.IO;
using System.Reflection;
using Microsoft.SPOT;
using System.Threading;

namespace ApertureLabs
{
    namespace GEX
    {
        public class Runnable
        {
            public enum AppStatus
            {
                GEX_EXIT,
                GEX_EXIT_WITH_CODE,
                GEX_EXIT_WITH_EXCEPTION,
                GEX_SLEEP,
                GEX_LOOP,
                GEX_IO,
                GEX_UNKNOWN
            }
            public Type calledType = null;
            public Thread programThread = null;
            public MethodInfo method = null;
            private static StreamWriter log;
            public void Run(string typeName, string methodName)
            {
                calledType = Type.GetType(typeName);
                programThread = new Thread(() => Invoke(methodName));
                programThread.Start();
            }
            public void Invoke (string methodName)
            {
                method = calledType.GetMethod(methodName);
                method.Invoke(AppDomain.CurrentDomain, null);
            }
            public void Quit(string DisplayName)
            {
                try
                {
                    programThread.Abort();
                    Debug.Print("Trying to stop process " + DisplayName);
                }
                catch (Exception)
                {
                    Debug.Print(DisplayName + " has stopped successfully.");
                }
                programThread = null;
            }
            public static void SetStatus(AppStatus status)
            {
                if (!Directory.Exists(@"\SD\DATA"))
                {
                    Directory.CreateDirectory(@"\Sd\DATA");
                }
                if (File.Exists(@"\SD\DATA\Status"))
                {
                    File.Delete(@"\SD\DATA\Status");
                }
                log = new StreamWriter(@"\SD\DATA\Status", false);
                log.WriteLine(status);
                log.Close();
            }
        }
    }
}
