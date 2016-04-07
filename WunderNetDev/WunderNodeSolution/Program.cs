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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WunderNetNode;
namespace WunderNetTest
{
    class Program
    {
        static void Main(string[] args)
        {
            WunderNode wl;
            StandardFeature[] features = new StandardFeature[4];
            features[0] = new StandardFeature("MotorLeft", FeatureBaseTypes.INT, FeatureIOTypes.OUTPUT);
            features[1] = new StandardFeature("MotorRight", FeatureBaseTypes.INT, FeatureIOTypes.OUTPUT);
            features[2] = new StandardFeature("FrontUltrasonic", FeatureBaseTypes.INT, FeatureIOTypes.INPUT);
            features[3] = new StandardFeature("RearUltrasonic", FeatureBaseTypes.INT, FeatureIOTypes.INPUT);

            if(args.Length > 0)
            {
                wl = new WunderNode(args[0]);
            }
            else
            {
                wl = new WunderNode("Tester");
            }


            wl.AddFeature("MotorLeft", FeatureBaseTypes.INT, FeatureIOTypes.OUTPUT);
            wl.AddFeature("MotorRight", FeatureBaseTypes.INT, FeatureIOTypes.OUTPUT);
            wl.AddFeature("FrontUltrasonic", FeatureBaseTypes.INT, FeatureIOTypes.INPUT);
            wl.AddFeature("RearUltrasonic", FeatureBaseTypes.INT, FeatureIOTypes.INPUT);

            wl.BasePacketReceived += BasePacketReceived;
            wl.StringDataReceived += StringDataReceived;
            wl.DescriptionReceived += DescriptionReceived;
            wl.FeatureUpdateReceived += FeatureUpdateReceived;


            string ConsoleIn = "";
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
                                    wl.UpdateFeature(testing[1], Convert.ToUInt32(testing[3])); break;
                                case FeatureBaseTypes.STRING:
                                    wl.UpdateFeature(testing[1], testing[3]); break;
                            }
                        }break;
                    case "subscribe":
                        {
                            testing = testing[1].Split(new char[] { ' ' }, 2);
                            wl.SubscribeToFeature(testing[0], testing[1]);
                        } break;
                }

            }
            wl.Disconnect();
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
                        uint val = BitConverter.ToUInt32(e.packet.Data, 0);
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

    }
}
