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
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using WunderNetLayer;

namespace WunderNetNode
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
    public class FeatureUpdatePacketEventArgs : EventArgs
    {
        public FeaturePacket packet;
    }
    
    public class WunderLayer
    {
        WunderNet _TheNet;
        public string Identifier;
        public event EventHandler<BasePacketEventArgs> BasePacketReceived;
        public event EventHandler<StringDataPacketEventArgs> StringDataReceived;
        public event EventHandler<DescriptionPacketEventArgs> DescriptionReceived;
        public event EventHandler<FeatureUpdatePacketEventArgs> FeatureUpdateReceived;
        public WunderLayer(string id)
        {
            this.WunderLayerSharedConstructor(id, "", 1000);
        }
        public WunderLayer(string id, string ip, int port)
        {
            this.WunderLayerSharedConstructor(id, ip, port);
        }
        private void WunderLayerSharedConstructor(string id, string ip, int port)
        {
            this.Identifier = id;
            _TheNet = new WunderNet();
            if (ip != "") _TheNet.SetTXPort(ip, port);
            else _TheNet.SetTXPort(port);
            _TheNet.WunderNetEvent += ProcessWunderNet;
            _TheNet.StartListening(port);
            
        }

        public void SendDiscover()
        {
            SendBasePacket(PacketTypes.DISCOVER);
        }
        public void SendDescribe(string receiver)
        {
            SendBasePacket(receiver, PacketTypes.DESCRIBE);
        }
        public void SendStringData(string receiver, string data)
        {
            StringDataPacket wp = new StringDataPacket();
            wp.SenderID = Identifier;
            wp.ReceiverID = receiver;
            wp.PacketType = (Int32)PacketTypes.STRING;
            wp.Data = data;
            _TheNet.SendPacket(wp.GetBytes());
        }
        public void Disconnect()
        {
            SendBasePacket(PacketTypes.OFFLINE);
            _TheNet.StopListening();
        }

        protected void SendOnline()
        {
            SendBasePacket(PacketTypes.ONLINE);
        }
        protected void SendFeatureUpdate(string receiver, string featureName, string s)
        {
            FeaturePacket update = new FeaturePacket(this.Identifier, receiver, featureName, FeatureBaseTypes.STRING);
            update.Data = Encoding.ASCII.GetBytes(s);
            _TheNet.SendPacket(update.GetBytes());
        }
        protected void SendFeatureUpdate(string receiver, string featureName, UInt32 s)
        {
            FeaturePacket update = new FeaturePacket(this.Identifier, receiver, featureName, FeatureBaseTypes.INT);
            update.Data = BitConverter.GetBytes(s);
            _TheNet.SendPacket(update.GetBytes());
        }
        protected void SendFeatureUpdate(string receiver, string featureName, bool s)
        {
            FeaturePacket update = new FeaturePacket(this.Identifier, receiver, featureName, FeatureBaseTypes.BOOL);
            update.Data = BitConverter.GetBytes(s);
            _TheNet.SendPacket(update.GetBytes());
        }
        protected void SendFeatureSubscribe(string receiver, string featureName)
        {
            FeaturePacket subs = new FeaturePacket(this.Identifier, receiver, featureName, PacketTypes.SUBSCRIBE);
            _TheNet.SendPacket(subs.GetBytes());
        }


        protected void SendDescription(string receiver, StandardFeature[] sf)
        {
            DescriptionPacket dp = new DescriptionPacket();
            dp.SenderID = Identifier;
            dp.ReceiverID = receiver;
            dp.PacketType = (Int32)PacketTypes.DESCRIPTION;
            foreach (StandardFeature f in sf)
            {
                dp.AddFeature(f);
            }
            _TheNet.SendPacket(dp.GetBytes());
        }
        protected void SendDescription(string receiver, ArrayList sf)
        {
            DescriptionPacket dp = new DescriptionPacket();
            dp.SenderID = Identifier;
            dp.ReceiverID = receiver;
            dp.PacketType = (Int32)PacketTypes.DESCRIPTION;
            foreach (StandardFeature f in sf)
            {
                dp.AddFeature(f);
            }
            _TheNet.SendPacket(dp.GetBytes());
        }
        protected void SendDescription(string receiver, ICollection sf)
        {
            DescriptionPacket dp = new DescriptionPacket();
            dp.SenderID = Identifier;
            dp.ReceiverID = receiver;
            dp.PacketType = (Int32)PacketTypes.DESCRIPTION;
            foreach (StandardFeature f in sf)
            {
                dp.AddFeature(f);
            }
            _TheNet.SendPacket(dp.GetBytes());
        }
        protected void SendIdentify(string receiver)
        {
            SendBasePacket(receiver, PacketTypes.IDENTIFY);
        }
        protected void SendBasePacket(PacketTypes type)
        {
            SendBasePacket("", type);
        }
        protected void SendBasePacket(string receiver, PacketTypes type)
        {
            BasePacket wp = new BasePacket();
            wp.SenderID = Identifier;
            wp.ReceiverID = receiver;
            wp.PacketType = (Int32)type;
            _TheNet.SendPacket(wp.GetBytes());
        }

        private void ProcessWunderNet(object sender, WunderNetPacketEventArgs e)
        {
            if(BasePacket.CheckHeader(e.RawData))
            {
                BasePacket bp = new BasePacket(e.RawData);

                if (bp.SenderID == this.Identifier) return;

                switch((PacketTypes)bp.PacketType)
                {
                    case PacketTypes.DATABLOCK: ProcessDataBlock(bp, e.RawData); break;
                    case PacketTypes.STRING: ProcessDataBlock(bp, e.RawData); break;
                    case PacketTypes.DISCOVER: ProcessDiscover(bp); break;
                    case PacketTypes.IDENTIFY: ProcessIdentify(bp); break;
                    case PacketTypes.ONLINE: ProcessBasePacketCallbacks(bp); break;
                    case PacketTypes.OFFLINE: ProcessBasePacketCallbacks(bp); break;
                    case PacketTypes.DESCRIBE: ProcessDescribe(bp); break;
                    case PacketTypes.DESCRIPTION: ProcessDecription(bp, e.RawData); break;
                    case PacketTypes.UPDATE: ProcessFeatureUpdate(bp, e.RawData); break;
                    case PacketTypes.COMMAND: ProcessBasePacketCallbacks(bp); break;
                    case PacketTypes.SUBSCRIBE: ProcessSubscribe(bp, e.RawData); break;
                }

            }
        }

        protected void ProcessBasePacketCallbacks(BasePacket p)
        {
            Delegate[] registeredEvents = BasePacketReceived.GetInvocationList();
            BasePacketEventArgs e = new BasePacketEventArgs();
            e.packet = p;
            foreach (Delegate el in registeredEvents)
            {
                ((EventHandler<BasePacketEventArgs>)el).Invoke(this, e);
            }
        }
        protected void ProcessDescriptionPacketCallbacks(DescriptionPacket p)
        {
            Delegate[] registeredEvents = DescriptionReceived.GetInvocationList();
            DescriptionPacketEventArgs e = new DescriptionPacketEventArgs();
            e.packet = p;
            foreach (Delegate el in registeredEvents)
            {
                ((EventHandler<DescriptionPacketEventArgs>)el).Invoke(this, e);
            }
        }
        protected void ProcessStringDataPacketCallbacks(StringDataPacket p)
        {
            Delegate[] registeredEvents = StringDataReceived.GetInvocationList();
            StringDataPacketEventArgs e = new StringDataPacketEventArgs();
            e.packet = p;
            foreach (Delegate el in registeredEvents)
            {
                ((EventHandler<StringDataPacketEventArgs>)el).Invoke(this, e);
            }
        }
        protected void ProcessFeatureUpdateCallbacks(FeaturePacket p)
        {
            Delegate[] registeredEvents = FeatureUpdateReceived.GetInvocationList();
            FeatureUpdatePacketEventArgs e = new FeatureUpdatePacketEventArgs();
            e.packet = p;
            foreach (Delegate el in registeredEvents)
            {
                ((EventHandler<FeatureUpdatePacketEventArgs>)el).Invoke(this, e);
            }
        }

        protected void ProcessDataBlock(BasePacket bp, byte[] rawBytes)
        {
            if (bp.ReceiverID == this.Identifier)
            {
                StringDataPacket sdp = new StringDataPacket(rawBytes);
                ProcessStringDataPacketCallbacks(sdp);
            }
        }
        protected void ProcessDiscover(BasePacket bp)
        {
            SendIdentify(bp.SenderID);
        }
        protected virtual void ProcessDescribe(BasePacket bp) { }
        protected virtual void ProcessSubscribe(BasePacket bp, byte[] rawBytes) { }
        protected void ProcessIdentify(BasePacket bp)
        {
            if (bp.ReceiverID == this.Identifier) ProcessBasePacketCallbacks(bp);
        }
        protected void ProcessDecription(BasePacket bp, byte[] rawBytes)
        {
            if (bp.ReceiverID == this.Identifier)
            {
                DescriptionPacket dp = new DescriptionPacket(rawBytes);
                ProcessDescriptionPacketCallbacks(dp);
            }
        }
        protected virtual void ProcessFeatureUpdate(BasePacket bp, byte[] rawBytes)
        {
            if (bp.ReceiverID == this.Identifier)
            {
                FeaturePacket dp = new FeaturePacket(rawBytes);
                ProcessFeatureUpdateCallbacks(dp);
            }
        }

    }
}
