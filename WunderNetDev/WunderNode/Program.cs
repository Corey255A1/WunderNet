using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WunderNode;
namespace WunderNetTest
{
    class Program
    {
        static void Main(string[] args)
        {

            WunderLayer wl;
            StandardFeature[] features = new StandardFeature[4];
            features[0] = new StandardFeature("MotorLeft", FeatureBaseTypes.INTVAL, FeatureIOTypes.OUTPUT);
            features[1] = new StandardFeature("MotorRight", FeatureBaseTypes.INTVAL, FeatureIOTypes.OUTPUT);
            features[2] = new StandardFeature("FrontUltrasonic", FeatureBaseTypes.INTVAL, FeatureIOTypes.INPUT);
            features[3] = new StandardFeature("RearUltrasonic", FeatureBaseTypes.INTVAL, FeatureIOTypes.INPUT);

            if(args.Length > 0)
            {
                wl = new WunderLayer(args[0], features);
            }
            else
            {
                wl = new WunderLayer("Tester", features);
            }

            wl.BasePacketReceived += BasePacketReceived;
            wl.StringDataReceived += StringDataReceived;
            wl.DescriptionReceived += DescriptionReceived;

            string ConsoleIn = "";
            while ((ConsoleIn = Console.ReadLine()) != "stop")
            {
                string[] testing = ConsoleIn.Split(new char[] { ' ' }, 2);
                if (testing[0] == "discover")
                {
                    wl.SendDiscover();
                }
                else if(testing[0] == "send")
                {
                    testing = testing[1].Split(new char[] { ' ' }, 2);
                    wl.SendStringData(testing[0], testing[1]);
                }
                else if(testing[0] == "describe")
                {
                    wl.SendDescribe(testing[1]);
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

    }
}
