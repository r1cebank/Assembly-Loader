/*
 * ApertureLabs Netduino & Netduino Plus + PC Library
 * Author : Siyuan Gao
 * Lisence : Private Use Only
 * Third-Party Library Used : MicroLiquidCrystal
 * Date : May 17, 2011
 * Warning : This is a private developed library that only limited
 * for private use, all code is under copyright of Siyuan Gao 2011
 */
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace ApertureLabs
{
    namespace AssemblyLoader
    {
        /// <summary>
        /// Load assembly from SD card or user defined places
        /// </summary>
        class AssemblyLoader
        {
            /// <summary>
            /// Stream Reader for assembly loading
            /// </summary>
            public static StreamReader reader = null;
            /// <summary>
            /// File stream for Mainfest file processing
            /// </summary>
            public static FileStream mfStream = null;
            /// <summary>
            /// AppInfo, used for launching apps
            /// </summary>
            public static string AppInfo = "";
            /// <summary>
            /// AppType, used for launching app in current domain
            /// </summary>
            public static string AppType = "";
            /// <summary>
            /// Main method, defined in the GMF file
            /// </summary>
            public static string Method = "";
            /// <summary>
            /// App's friendly display name
            /// </summary>
            public static string DisplayName = "";
            /// <summary>
            /// App's version
            /// </summary>
            public static string Version = "";
            /// <summary>
            /// Pin List for checking Avaliable Pins
            /// </summary>
            public static string PinList = "";
            /// <summary>
            /// Quit current app by Path
            /// </summary>
            /// <param name="appPath">application Path</param>
            public static void QuitApp(string appPath)
            {
                Type appType = null;
                MethodInfo method = null;
                Clear();
                mfStream = new FileStream(appPath + "AppInfo.GMF", FileMode.Open, FileAccess.Read, FileShare.None);
                Process();
                appType = Type.GetType(AppType);
                method = appType.GetMethod("Quit");
                method.Invoke(AppDomain.CurrentDomain, new Object[] { DisplayName });
                mfStream.Close();
                mfStream = null;
            }
            /// <summary>
            /// Load application from SD card
            /// </summary>
            /// <param name="appPath">application path (real path)</param>
            public static void LoadApp(string appPath)
            {
                //TODO: Add Check for Pin Usage here!
                Clear();
                mfStream = new FileStream(appPath + "AppInfo.GMF", FileMode.Open, FileAccess.Read, FileShare.None);
                Process();
                Debug.Print("Checking for busy pins...");
                if (!System.PinBusyList.AddProgram(DisplayName, PinList))
                {
                    Debug.Print("Process" + DisplayName + " cannot be launched.");
                    return;
                }
                Debug.Print("Pin Checking Complete...");
                Debug.Print("Checking for running process...");

                if (!System.ProcessManager.AddProcesses(DisplayName, appPath))
                {
                    Debug.Print(DisplayName + "v" + Version + " is already running.");
                    return;
                }
                Debug.Print("Process " + DisplayName + "is ready for launch.");
                try
                {
                    using (FileStream assmfile = new FileStream(appPath + "Main.GEX", FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        byte[] assmbytes = new byte[assmfile.Length];
                        Assembly assm = null;
                        object domain = new object();
                        Type type = null;
                        MethodInfo mi = null;
                        try
                        {
                            assmfile.Read(assmbytes, 0, (int)assmfile.Length);
                        }
                        catch (Exception)
                        {
                            Debug.Print("File Read Failed!");
                            return;
                        }
                        try
                        {
                            assm = Assembly.Load(assmbytes);
                        }
                        catch (Exception)
                        {
                            Debug.Print("Assembly " + AppType + " has failed to load.");
                            return;
                        }
                        try
                        {
                            domain = AppDomain.CurrentDomain;
                            //domain = AppDomain.CurrentDomain.CreateInstanceAndUnwrap(AppInfo, AppType);
                        }
                        catch (Exception)
                        {
                            Debug.Print("App: " + AppInfo + " has failed to Unwrap");
                            return;
                        }
                        try
                        {
                            type = assm.GetType(AppType);
                        }
                        catch (Exception)
                        {
                            Debug.Print("Type: " + AppType + " not found!");
                            return;
                        }
                        try
                        {
                            mi = type.GetMethod("Run");
                        }
                        catch (Exception)
                        {
                            Debug.Print("Method: " + Method + " is not found.");
                            return;
                        }
                        try
                        {
                            mi.Invoke(domain, new Object[] { AppType, Method });
                            Debug.Print(DisplayName + "v" + Version + " (loaded)");
                        }
                        catch (Exception)
                        {
                            Debug.Print("Program: " + AppInfo + " has failed to launch.");
                            return;
                        }
                    }
                }
                catch (Exception)
                {
                    Debug.Print("Main.gex is not found.");
                    return;
                }
                reader.Close();
                mfStream.Close();
                reader = null;
                mfStream = null;
            }
            /// <summary>
            /// Clear all static members, called before any launching
            /// </summary>
            private static void Clear()
            {
                AppType = "";
                AppInfo = "";
                Method = "";
                Version = "";
                DisplayName = "";
                PinList = "";
            }
            /// <summary>
            /// Process the GMF file for app information
            /// </summary>
            private static void Process()
            {
                //TODO: add the PIN Usage here!
                reader = new StreamReader(mfStream);
                string line = "";
                while ((line = reader.ReadLine()) != "")
                {
                    if (line.Split(' ')[0].ToUpper() == "APPNAME")
                    {
                        AppInfo = line.Split(' ')[2];
                    }
                    if (line.Split(' ')[0].ToUpper() == "VERSION")
                    {
                        AppInfo += (", Version=" + line.Split(' ')[2]);
                        Version = line.Split(' ')[2];
                    }
                    if (line.Split(' ')[0].ToUpper() == "NAMESPACE")
                    {
                        AppType = line.Split(' ')[2];
                    }
                    if (line.Split(' ')[0].ToUpper() == "CLASS")
                    {
                        AppType += ("." + line.Split(' ')[2]);
                    }
                    if (line.Split(' ')[0].ToUpper() == "METHOD")
                    {
                        Method = line.Split(' ')[2];
                    }
                    if (line.Split(' ')[0].ToUpper() == "DISPLAYNAME")
                    {
                        for (int i = 2; i < line.Split(' ').Length; i++)
                        {
                            DisplayName += (line.Split(' ')[i] + " ");
                            DisplayName = DisplayName.Trim();
                        }
                    }
                    if (line.Split(' ')[0].ToUpper() == "PIN")
                    {
                        PinList = line.Split(' ')[2];
                    }
                }
            }
        }
    }
    namespace System
    {
        //TODO: Need more functionality, good for now.
        class MemoryWatchCat
        {
            /// <summary>
            /// Thread that will run watchCat
            /// </summary>
            public static Thread watchCatThread = null;
            /// <summary>
            /// Run watch Cat in thread
            /// </summary>
            /// <param name="wait">wait time for watch cat in ms</param>
            public static void RunWatchCat(int wait)
            {
                watchCatThread = new Thread(() => WatchCat(wait));
                watchCatThread.Priority = ThreadPriority.Highest;
                watchCatThread.Start();
            }
            /// <summary>
            /// Watch Cat main method, loop forever
            /// </summary>
            /// <param name="wait">wait time in ms</param>
            private static void WatchCat(int wait)
            {
                while (true)
                {
                    if (Debug.GC(true) < 5000)
                    {
                        //Do some killing
                        ProcessManager.KillAll();
                    }
                    if (Debug.GC(true) < 1000)
                    {
                        PowerState.RebootDevice(false);
                    }
                    Thread.Sleep(wait);
                }
            }
        }
        class ProcessManager
        {
            /// <summary>
            /// HashTable for process manager
            /// </summary>
            //TODO:TODO
            public static Hashtable processes = null;
            /// <summary>
            /// Determine if this is the first time, used to initialize hashtable
            /// </summary>
            private static bool FirstCall = true;
            /// <summary>
            /// Clear the current process table
            /// </summary>
            private static void ClearTable()
            {
                //Clear HashTable
                processes.Clear();
            }
            /// <summary>
            /// Number of process is running
            /// </summary>
            public static int NumOfProcess
            {
                get
                {
                    return processes.Count;
                }
            }
            /// <summary>
            /// Kill all processes in the hashtable
            /// </summary>
            public static void KillAll()
            {
                foreach (DictionaryEntry e in processes)
                {
                    KillProcess(e.Key.ToString());
                }
                processes.Clear();
            }
            /// <summary>
            /// List all running processes
            /// </summary>
            public static void ListProcesses()
            {
                Debug.Print("Process Display Name | AppPath");
                foreach (DictionaryEntry e in processes)
                {
                    Debug.Print(e.Key + " | " + e.Value);
                }
            }
            /// <summary>
            /// Add a process to the hashtable
            /// </summary>
            /// <param name="DisplayName">Displayname for the process</param>
            /// <param name="AppPath">path of the app</param>
            /// <returns>False for this app is already in the hastable, True for not</returns>
            public static bool AddProcesses(string DisplayName, string AppPath)
            {
                if (FirstCall)
                {
                    processes = new Hashtable();
                    FirstCall = false;
                }
                if (processes.Contains(DisplayName))
                {
                    return false;
                }
                else
                {
                    processes.Add(DisplayName, AppPath);
                    return true;
                }
            }
            /// <summary>
            /// Kill a process by its AppPath
            /// </summary>
            /// <param name="DisplayName">Displayname of the app</param>
            /// <param name="AppPath">App Path</param>
            public static void KillProcess(string DisplayName)
            {
                string AppPath = processes[DisplayName].ToString();
                AssemblyLoader.AssemblyLoader.QuitApp(AppPath);
                PinBusyList.RemoveProgram(DisplayName);
                processes.Remove(DisplayName);
            }
            /// <summary>
            /// Check to see if the app is running
            /// </summary>
            /// <param name="DisplayName">Process's display name</param>
            /// <returns>True for running, false fot not</returns>
            public static bool IsRunning(string DisplayName)
            {
                if (processes.Contains(DisplayName))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        class PinBusyList
        {
            /// <summary>
            /// Hashtable for BusyPins Listing
            /// </summary>
            private static Hashtable BusyPins = null;
            /// <summary>
            /// Flag indicates first run
            /// </summary>
            private static bool FirstRun = true;
            /// <summary>
            /// Add a program to BusyPins List
            /// </summary>
            /// <param name="DisplayName">Displayname of the application</param>
            /// <param name="PinList">Pin list defined in GMF</param>
            /// <returns></returns>
            public static bool AddProgram(string DisplayName, string PinList)
            {
                //If true then none of the Pins are used
                bool RunStatus = true;
                string[] Pin = PinList.Split(',');
                if (FirstRun)
                {
                    BusyPins = new Hashtable();
                    FirstRun = false;
                }
                if (PinList == "GPIO_NONE")
                {
                    Debug.Print("Program " + DisplayName + "is not using any Pins.");
                    return true;
                }
                else
                {
                    foreach (string s in Pin)
                    {
                        foreach (DictionaryEntry e in BusyPins)
                        {
                            string[] EntryPin = e.Value.ToString().Split(',');
                            foreach (string EntryS in EntryPin)
                            {
                                if (s == EntryS)
                                {
                                    RunStatus = false;
                                    Debug.Print("Process " + e.Key.ToString() + " is using " + EntryS);
                                }
                            }
                        }
                    }
                }
                if (RunStatus)
                {
                    BusyPins.Add(DisplayName, PinList);
                }
                return RunStatus;
            }
            /// <summary>
            /// Print PinList in Hashtable
            /// </summary>
            public static void PrintPinList()
            {
                Debug.Print("Display Name | Pin");
                foreach (DictionaryEntry e in BusyPins)
                {
                    Debug.Print(e.Key.ToString() + " | " + e.Value.ToString());
                }
            }
            /// <summary>
            /// Remove program from the hashtable
            /// </summary>
            /// <param name="DisplayName">Displayname of the program as key</param>
            public static void RemoveProgram(string DisplayName)
            {
                BusyPins.Remove(DisplayName);
            }
        }
    }
}
