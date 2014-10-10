using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using ApertureLabs.AssemblyLoader;

namespace ApertureLabs
{
    public class Program
    {
        public static void Main()
        {
            // write your code here
            ApertureLabs.System.MemoryWatchCat.RunWatchCat(1000);
            Debug.Print("Init Free mem: " + Debug.GC(true));
            ApertureLabs.AssemblyLoader.AssemblyLoader.LoadApp(@"SD\PROGRAM\Test.GPK\");
            Thread.Sleep(4000);
            Debug.Print("Killing Process");
            System.ProcessManager.KillProcess("TestingClass");
            Debug.Print("Process killed");
            System.ProcessManager.ListProcesses();
            System.PinBusyList.PrintPinList();
            ApertureLabs.AssemblyLoader.AssemblyLoader.LoadApp(@"SD\PROGRAM\Test.GPK\");
            Thread.Sleep(10000);
            ApertureLabs.System.ProcessManager.ListProcesses();
            Thread.Sleep(Timeout.Infinite);
        }

    }
}
