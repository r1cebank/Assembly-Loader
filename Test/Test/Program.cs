using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using ApertureLabs.GEX;

namespace Test
{
    public class Program : Runnable
    {
        public static void Main()
        {
            // write your code here
            Debug.Print("I am launched");
            //OutputPort LED = new OutputPort(Pins.ONBOARD_LED,false);
            //while (true)
            //{
            //    LED.Write(!LED.Read());
            //    Thread.Sleep(1000);
            //}
            SetStatus(AppStatus.GEX_EXIT);
        }

    }
}
