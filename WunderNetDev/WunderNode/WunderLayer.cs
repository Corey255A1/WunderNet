/**
 * Author: Corey Wunderlich
 * Date: 3/25/2016
 * Description: The WunderLayer. This handles all of the basic
 * protocol handshaking and packet deserialization. The basic idea is that
 * any node on the network can discover and control/communicate with other nodes.
 * The basic protocol intends for there to be entities that have features. You
 * can discover entities on the network, get their list of features, and control those
 * features in a standard way.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using WunderNetLayer;

namespace WunderNode
{
    public class BasePacketEventArgs : EventArgs
    {
        public BasePacket packet;
    }
    public class StringDataPacketEventArgs : EventArgs
    {
        public StringDataPacket packet;
    }
    public class DescriptionPacketEventArgs : EventArgs 
    {
        public DescriptionPacket packet;
    }
    
    class WunderLayer
    {
        WunderNet _TheNet;
        public string Identifer;
        public StandardFeature[] Features;
        public event EventHandler<BasePacketEventArgs> BasePacketReceived;
        public event EventHandler<StringDataPacketEventArgs> StringDataReceived;
        public event EventHandler<DescriptionPacketEventArgs> DescriptionReceived;

        public WunderLayer(string id, StandardFeature[] features)
        {
            this.Identifer = id;
            this.Features = features;

            _TheNet = new WunderNet();
            _TheNet.SetTXPort(1000);
            _TheNet.StartListening(1000);
            _TheNet.WunderNetEvent += ProcessWunderNet;

            SendOnline();
        }

        public void SendDiscover()
        {
            SendBasePacket(PacketTypes.DISCOVER);
        }
        public void SendOnline()
        {
            SendBasePacket(PacketTypes.ONLINE);
        }
        public void SendDescribe(string receiver)
        {
            SendBasePacket(receiver, PacketTypes.DESCRIBE);
        }
        public void SendIdentify(string receiver)
        {
            SendBasePacket(receiver, PacketTypes.IDENTIFY);
        }
        public void SendDescription(string receiver, StandardFeature[] sf)
        {
            DescriptionPacket dp = new DescriptionPacket();
            dp.SenderID = Identifer;
            dp.ReceiverID = receiver;
            dp.PacketType = (Int32)PacketTypes.DESCRIPTION;
            foreach(StandardFeature f in sf)
            {
                dp.AddFeature(f);
            }
            _TheNet.SendPacket(dp.GetBytes());
        }
        public void SendStringData(string receiver, string data)
        {
            StringDataPacket wp = new StringDataPacket();
            wp.SenderID = Identifer;
            wp.ReceiverID = receiver;
            wp.PacketType = (Int32)PacketTypes.DATABLOCK;
            wp.Data = data;
            _TheNet.SendPacket(wp.GetBytes());
        }        
        public void Disconnect()
        {
            SendBasePacket(PacketTypes.OFFLINE);
            _TheNet.StopListening();
        }

        private void SendBasePacket(PacketTypes type)
        {
            SendBasePacket("", type);
        }
        private void SendBasePacket(string receiver, PacketTypes type)
        {
            BasePacket wp = new BasePacket();
            wp.SenderID = Identifer;
            wp.ReceiverID = receiver;
            wp.PacketType = (Int32)type;
            _TheNet.SendPacket(wp.GetBytes());
        }

        private void ProcessWunderNet(object sender, WunderNetPacketEventArgs e)
        {
            if(BasePacket.CheckHeader(e.RawData))
            {
                BasePacket bp = new BasePacket(e.RawData);

                if (bp.SenderID == this.Identifer) return;

                switch((PacketTypes)bp.PacketType)
                {
                    case PacketTypes.DATABLOCK: ProcessDataBlock(bp, e.RawData); break;
                    case PacketTypes.DISCOVER: ProcessDiscover(bp); break;
                    case PacketTypes.IDENTIFY: ProcessIdentify(bp); break;
                    case PacketTypes.ONLINE: ProcessBasePacketCallbacks(bp); break;
                    case PacketTypes.OFFLINE: ProcessBasePacketCallbacks(bp); break;
                    case PacketTypes.DESCRIBE: ProcessDescribe(bp); break;
                    case PacketTypes.DESCRIPTION: ProcessDecription(bp, e.RawData); break;
                }

            }
        }

        private void ProcessBasePacketCallbacks(BasePacket p)
        {
            Delegate[] registeredEvents = BasePacketReceived.GetInvocationList();
            BasePacketEventArgs e = new BasePacketEventArgs();
            e.packet = p;
            foreach (Delegate el in registeredEvents)
            {
                ((EventHandler<BasePacketEventArgs>)el).Invoke(this, e);
            }
        }
        private void ProcessDescriptionPacketCallbacks(DescriptionPacket p)
        {
            Delegate[] registeredEvents = DescriptionReceived.GetInvocationList();
            DescriptionPacketEventArgs e = new DescriptionPacketEventArgs();
            e.packet = p;
            foreach (Delegate el in registeredEvents)
            {
                ((EventHandler<DescriptionPacketEventArgs>)el).Invoke(this, e);
            }
        }
        private void ProcessStringDataPacketCallbacks(StringDataPacket p)
        {
            Delegate[] registeredEvents = StringDataReceived.GetInvocationList();
            StringDataPacketEventArgs e = new StringDataPacketEventArgs();
            e.packet = p;
            foreach (Delegate el in registeredEvents)
            {
                ((EventHandler<StringDataPacketEventArgs>)el).Invoke(this, e);
            }
        }

        private void ProcessDataBlock(BasePacket bp, byte[] rawBytes)
        {
            if (bp.ReceiverID == this.Identifer)
            {
                StringDataPacket sdp = new StringDataPacket(rawBytes);
                ProcessStringDataPacketCallbacks(sdp);
            }
        }
        private void ProcessDiscover(BasePacket bp)
        {
            SendIdentify(bp.SenderID);
        }
        private void ProcessDescribe(BasePacket bp)
        {
            if (bp.ReceiverID == this.Identifer)
            {
                SendDescription(bp.SenderID, this.Features);  
            }
        }
        private void ProcessIdentify(BasePacket bp)
        {
            if (bp.ReceiverID == this.Identifer) ProcessBasePacketCallbacks(bp);
        }
        private void ProcessDecription(BasePacket bp, byte[] rawBytes)
        {
            if (bp.ReceiverID == this.Identifer)
            {
                DescriptionPacket dp = new DescriptionPacket(rawBytes);
                ProcessDescriptionPacketCallbacks(dp);
            }
        }

    }
}
