/**
 * Author: Corey Wunderlich
 * Date: 3/25/16
 * Description: Simple command testing utility for the WunderLayer.
 * 
 * Since this is a very quick and dirty tester, make sure all of the params are filled.
 * There aren't checks.
 * 
 * Open multiple instances of the program with unique node names
 * and they can communicate with each other.
 * 
 * WunderNetTest.exe [node name] - this will start the command line tool 
 *   and that instances name will be node name.
 * 
 * Command Line commands:
 * discover - Every instance or Node will respond back and be printed to the command line
 * send [node name] [string] - Send a string to a node. 
 *      Node name case sensitive. String can be arbitrary. Receiver will print.
 * describe [node name] - Send the describe request to a node. 
 *      The response will be printed to the command line.
 */

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using WunderNetNode;
namespace WunderNetTest
{
    class Program
    {
        static WunderNode wl;
        static bool updateThread = false;
        static Thread updateTest;
        static Hashtable FeatureValues = new Hashtable();
        static void Main(string[] args)
        {
            
            if(args.Length > 0)
            {
                wl = new WunderNode(args[0]);
            }
            else
            {
                wl = new WunderNode("Tester");
            }

            FeatureValues.Add("MotorLeft", 0);
            FeatureValues.Add("MotorRight", 0);
            FeatureValues.Add("FrontUltrasonic", 0);
            FeatureValues.Add("RearUltrasonic", 0);
            wl.AddFeature("MotorLeft", FeatureBaseTypes.INT, FeatureIOTypes.OUTPUT);
            wl.AddFeature("MotorRight", FeatureBaseTypes.INT, FeatureIOTypes.OUTPUT);
            wl.AddFeature("FrontUltrasonic", FeatureBaseTypes.INT, FeatureIOTypes.INPUT);
            wl.AddFeature("RearUltrasonic", FeatureBaseTypes.INT, FeatureIOTypes.INPUT);

            wl.BasePacketReceived += BasePacketReceived;
            wl.StringDataReceived += StringDataReceived;
            wl.DescriptionReceived += DescriptionReceived;
            wl.FeatureUpdateReceived += FeatureUpdateReceived;
            wl.FeatureCommandReceived += FeatureCommandReceived;

            string ConsoleIn = "";
            updateThread = true;
            updateTest = new Thread(TestUpdateThread);
            updateTest.Start();
            while ((ConsoleIn = Console.ReadLine()) != "stop")
            {
                string[] testing = ConsoleIn.Split(new char[] { ' ' }, 2);
                switch(testing[0])
                {
                    case "discover":
                        wl.SendDiscover(); 
                        break;
                    case "send":
                        testing = testing[1].Split(new char[] { ' ' }, 2);
                        wl.SendStringData(testing[0], testing[1]);
                        break;
                    case "describe": 
                        wl.SendDescribe(testing[1]); 
                        break;
                    case "update":
                        {
                            testing = testing[1].Split(new char[] { ' ' }, 4);
                            switch (((FeatureBaseTypes)Convert.ToInt32(testing[2])))
                            {
                                case FeatureBaseTypes.INT:
                                    wl.UpdateFeature(testing[1], Convert.ToInt32(testing[3])); break;
                                case FeatureBaseTypes.STRING:
                                    wl.UpdateFeature(testing[1], testing[3]); break;
                            }
                        }break;
                    case "subscribe":
                        {
                            testing = testing[1].Split(new char[] { ' ' }, 3);
                            wl.SubscribeToFeature(testing[0], testing[1], (FeatureBaseTypes)Convert.ToInt32(testing[2]));
                        } break;
                    case "command":
                        {
                            testing = testing[1].Split(new char[] { ' ' }, 3);
                            wl.CommandFeature(testing[0],testing[1], Convert.ToInt32(testing[2]));
                        } break;
                }

            }
            updateThread = false;
            wl.Disconnect();
        }


        private static void TestUpdateThread()
        {
            Random r = new Random();
            while(updateThread)
            {
                if(wl!=null)
                {
                    wl.UpdateFeature("FrontUltrasonic", r.Next(15));
                    wl.UpdateFeature("MotorLeft", (int)FeatureValues["MotorLeft"]);
                }
                Thread.Sleep(50);
            }

        }
        private static void StringDataReceived(object sender, StringDataPacketEventArgs e)
        {
            Console.WriteLine(e.packet.SenderID + ": " + e.packet.Data);
        }
        private static void DescriptionReceived(object sender, DescriptionPacketEventArgs e)
        {
            foreach (StandardFeature sf in e.packet.GetFeatures())
            {
                string s = sf.FeatureName + " " +
                    ((FeatureBaseTypes)sf.FeatureBaseType) + " " +
                    ((FeatureIOTypes)sf.FeatureIOType);
                Console.WriteLine(s);

            }
        }
        private static void BasePacketReceived(object sender, BasePacketEventArgs e)
        {
            switch ((PacketTypes)e.packet.PacketType)
            {
                case PacketTypes.IDENTIFY: Console.WriteLine(e.packet.SenderID); break;
                case PacketTypes.ONLINE: Console.WriteLine(e.packet.SenderID + " is now Online"); break;
                case PacketTypes.OFFLINE: Console.WriteLine(e.packet.SenderID + " is now Offline"); break;
            }
        }
        private static void FeatureUpdateReceived(object sender, FeatureUpdatePacketEventArgs e)
        {
            switch((FeatureBaseTypes)e.packet.FeatureBaseType)
            {
                case FeatureBaseTypes.INT:
                    {
                        int val = BitConverter.ToInt32(e.packet.Data, 0);
                        Console.WriteLine(e.packet.SenderID + " " + e.packet.FeatureName + " value: " + val.ToString());
                        break;
                    }
                case FeatureBaseTypes.BOOL:
                    {
                        bool val = BitConverter.ToBoolean(e.packet.Data, 0);
                        Console.WriteLine(e.packet.SenderID + " " + e.packet.FeatureName + " value: " + val.ToString());
                        break;
                    }
                case FeatureBaseTypes.STRING:
                    {
                        string val = Encoding.ASCII.GetString(e.packet.Data);
                        Console.WriteLine(e.packet.SenderID + " " + e.packet.FeatureName + " value: " + val);
                        break;
                    }

            }
        }
        private static void FeatureCommandReceived(object sender, FeatureCommandPacketEventArgs e)
        {
            switch ((FeatureBaseTypes)e.packet.FeatureBaseType)
            {
                case FeatureBaseTypes.INT: FeatureValues[e.packet.FeatureName] = BitConverter.ToInt32(e.packet.Data, 0); break;
                case FeatureBaseTypes.BOOL: FeatureValues[e.packet.FeatureName] = BitConverter.ToBoolean(e.packet.Data, 0); break;
                case FeatureBaseTypes.STRING: FeatureValues[e.packet.FeatureName] = Encoding.ASCII.GetString(e.packet.Data); break;
            }
        }

    }
}
