/**
 * Author: Corey Wunderlich
 * Date: 3/25/2016
 * Description: WunderLayer Packets. These are the packet structures 
 * for use on a WunderLayer network. Handles the serialization of the data
 * for network transport and also deserialization for use throughout the code.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace WunderNetNode
{
    public enum PacketTypes { OFFLINE, ONLINE, DISCOVER, IDENTIFY, DESCRIBE, DESCRIPTION, SUBSCRIBE, UNSUBSCRIBE, DATABLOCK, STRING, UPDATE, COMMAND }
    public enum FeatureIOTypes { INPUT, OUTPUT, INOUT }
    public enum FeatureBaseTypes { BOOL, INT, STRING, DATABLOCK }
    public interface WunderPacket
    {
        byte[] GetBytes();
    }

    public class BasePacket : WunderPacket
    {
        public const int BASESIZE = 84;
        public const string WUNDERHEADER = "!!WunderPacket!!"; //16

        private string _senderid; //32
        private string _recieverid; //32

        public string SenderID
        {
            get { return _senderid; }
            set
            {
                _senderid = value.TrimEnd();
                if (_senderid.Length > 32) _senderid = value.Substring(0, 32);
            }
        }
        public string ReceiverID
        {
            get { return _recieverid; }
            set
            {
                _recieverid = value.TrimEnd();
                if (_recieverid.Length > 32) _recieverid = value.Substring(0, 32);
            }
        }
        public Int32 PacketType; //4

        public BasePacket() { }
        public BasePacket(byte[] b)
        {
            int offset = WUNDERHEADER.Length;
            this.SenderID = Encoding.ASCII.GetString(b, offset, 32); offset += 32;
            this.ReceiverID = Encoding.ASCII.GetString(b, offset, 32); offset += 32;
            this.PacketType = BitConverter.ToInt32(b, offset); offset += 4;
        }

        public static bool CheckHeader(byte[] b)
        {
            if (b.Length >= WUNDERHEADER.Length)
            {
                string header = Encoding.ASCII.GetString(b, 0, WUNDERHEADER.Length);
                return header == WUNDERHEADER;
            }
            return false;
        }

        public virtual byte[] GetBytes()
        {
            byte[] a = Encoding.ASCII.GetBytes(WUNDERHEADER);

            string s = SenderID;
            if (s.Length < 32)  s = s.PadRight(32, ' ');
            byte[] b = Encoding.ASCII.GetBytes(s);

            s = ReceiverID;
            if (s.Length < 32) s = s.PadRight(32, ' ');
            byte[] c = Encoding.ASCII.GetBytes(s);

            byte[] d = BitConverter.GetBytes(this.PacketType);

            byte[] block = new byte[a.Length + b.Length + c.Length + d.Length];
            int offset = 0;
            System.Buffer.BlockCopy(a, 0, block, offset, a.Length);
            offset += a.Length;
            System.Buffer.BlockCopy(b, 0, block, offset, b.Length);
            offset += b.Length;
            System.Buffer.BlockCopy(c, 0, block, offset, c.Length);
            offset += c.Length;
            System.Buffer.BlockCopy(d, 0, block, offset, d.Length);

            return block;
        }
    }
    public class StringDataPacket : BasePacket
    {
        private Int32 _dataSize;
        private string _data;
        public string Data
        {
            get { return _data; }
            set
            {
                _data = value;
                _dataSize = _data.Length;
            }
        }

        public StringDataPacket() { }
        public StringDataPacket(byte[] b) : base(b)
        {
            int offset = BASESIZE;
            this._dataSize = BitConverter.ToInt32(b, offset); offset += 4;
            this.Data = Encoding.ASCII.GetString(b, offset, this._dataSize);
            
        }

        public override byte[] GetBytes()
        {
            byte[] a = base.GetBytes();
            byte[] b = BitConverter.GetBytes(this._dataSize);
            byte[] c = Encoding.ASCII.GetBytes(this._data);

            byte[] block = new byte[a.Length + b.Length + c.Length];
            int offset = 0;
            System.Buffer.BlockCopy(a, 0, block, offset, a.Length);
            offset += a.Length;
            System.Buffer.BlockCopy(b, 0, block, offset, b.Length);
            offset += b.Length;
            System.Buffer.BlockCopy(c, 0, block, offset, c.Length);
            return block;
        }
    }
    public class RawDataPacket : BasePacket
    {
        private Int32 _dataSize;
        private byte[] _data;
        public byte[] Data
        {
            get { return _data;  }
            set { _data = value; _dataSize = _data.Length; }
        }

        public RawDataPacket() { }
        public RawDataPacket(byte[] b): base(b)
        {
            int offset = BASESIZE;
            this._dataSize = BitConverter.ToInt32(b, offset); offset += 4;
            this.Data = new byte[this._dataSize];
            System.Buffer.BlockCopy(b, offset, this.Data, 0, this._dataSize);

        }

        public override byte[] GetBytes()
        {
            byte[] a = base.GetBytes();
            byte[] b = BitConverter.GetBytes(this._dataSize);
            byte[] c = this.Data;

            byte[] block = new byte[a.Length + b.Length + c.Length];
            int offset = 0;
            System.Buffer.BlockCopy(a, 0, block, offset, a.Length);
            offset += a.Length;
            System.Buffer.BlockCopy(b, 0, block, offset, b.Length);
            offset += b.Length;
            System.Buffer.BlockCopy(c, 0, block, offset, c.Length);
            return block;
        }
    }

    public class FeaturePacket : BasePacket
    {
        private string _featureName;
        public string FeatureName
        {
            get { return _featureName; }
            set
            {
                _featureName = value.TrimEnd();
                if (_featureName.Length > 32) _featureName = value.Substring(0, 32);
            }
        }
        public Int32 FeatureBaseType;
        private Int32 _dataSize;
        private byte[] _data;
        public byte[] Data
        {
            get { return _data; }
            set { _data = value; _dataSize = _data.Length; }
        }

        public FeaturePacket() { }
        public FeaturePacket(string sender, string receiver, string featureName, FeatureBaseTypes featuretype)
        {
            this.SenderID = sender;
            this.ReceiverID = receiver;
            this.PacketType = (Int32)PacketTypes.UPDATE;
            this.FeatureName = featureName;
            this.FeatureBaseType = (Int32)featuretype;
        }
        public FeaturePacket(string sender, string receiver, string featureName, FeatureBaseTypes featuretype, PacketTypes packettype )
        {
            this.SenderID = sender;
            this.ReceiverID = receiver;
            this.PacketType = (Int32)packettype;
            this.FeatureName = featureName;
            this.FeatureBaseType = (Int32)featuretype;
        }
        public FeaturePacket(string sender, string receiver, string featureName, PacketTypes packettype)
        {
            this.SenderID = sender;
            this.ReceiverID = receiver;
            this.PacketType = (Int32)packettype;
            this.FeatureName = featureName;
            this.FeatureBaseType = 0;
        }
        public FeaturePacket(byte[] b) : base(b)
        {
            int offset = BASESIZE;
            this.FeatureName = Encoding.ASCII.GetString(b, offset, 32); offset += 32;
            this.FeatureBaseType = BitConverter.ToInt32(b, offset); offset += 4;
            this._dataSize = BitConverter.ToInt32(b, offset); offset += 4;
            this.Data = new byte[this._dataSize];
            System.Buffer.BlockCopy(b, offset, this.Data, 0, this._dataSize);
        }

        public override byte[] GetBytes()
        {
            byte[] a = base.GetBytes();
            string s = FeatureName;
            if (s.Length < 32) s = s.PadRight(32, ' ');
            byte[] b = Encoding.ASCII.GetBytes(s);
            byte[] c = BitConverter.GetBytes(this.FeatureBaseType);
            byte[] d = BitConverter.GetBytes(this._dataSize);

            byte[] block = new byte[a.Length + b.Length + c.Length + d.Length + this._dataSize];

            int offset = 0;
            System.Buffer.BlockCopy(a, 0, block, offset, a.Length);
            offset += a.Length;
            System.Buffer.BlockCopy(b, 0, block, offset, b.Length);
            offset += b.Length;
            System.Buffer.BlockCopy(c, 0, block, offset, c.Length);
            offset += c.Length;
            System.Buffer.BlockCopy(d, 0, block, offset, d.Length);
            if (this._dataSize > 0)
            {
                offset += d.Length;
                System.Buffer.BlockCopy(this.Data, 0, block, offset, this._dataSize);
            }


            return block;

        }
    }

    public class StandardFeature : WunderPacket
    {
        private string _featureName;
        public string FeatureName
        {
            get { return _featureName; }
            set
            {
                _featureName = value.TrimEnd();
                if (_featureName.Length > 32) _featureName = value.Substring(0, 32);
            }
        }
        public Int32 FeatureIOType;
        public Int32 FeatureBaseType;

        public StandardFeature() { }
        public StandardFeature(string name, FeatureBaseTypes type, FeatureIOTypes io) 
        {
            this.FeatureBaseType = (Int32)type;
            this.FeatureName = name;
            this.FeatureIOType = (Int32)io;
        }
        public StandardFeature(byte[] b, int offset = 0)
        {
            this.FeatureName = Encoding.ASCII.GetString(b, offset, 32); offset += 32;
            this.FeatureIOType = BitConverter.ToInt32(b, offset); offset += 4;
            this.FeatureBaseType = BitConverter.ToInt32(b, offset);
            
        }
        public int SetBytes(byte[] b, int offset = 0)
        {
            this.FeatureName = Encoding.ASCII.GetString(b, offset, 32); offset += 32;
            this.FeatureIOType = BitConverter.ToInt32(b, offset); offset += 4;
            this.FeatureBaseType = BitConverter.ToInt32(b, offset); offset += 4;
            return offset;
        }
        public byte[] GetBytes()
        {
            string s = FeatureName;
            if (s.Length < 32) s = s.PadRight(32, ' ');
            byte[] a = Encoding.ASCII.GetBytes(s);
            byte[] b = BitConverter.GetBytes(this.FeatureIOType);
            byte[] c = BitConverter.GetBytes(this.FeatureBaseType);
            byte[] block = new byte[a.Length + b.Length + c.Length];
            int offset = 0;
            System.Buffer.BlockCopy(a, 0, block, offset, a.Length);
            offset += a.Length;
            System.Buffer.BlockCopy(b, 0, block, offset, b.Length);
            offset += b.Length;
            System.Buffer.BlockCopy(c, 0, block, offset, c.Length);
            return block;

        }
    }
    public class DescriptionPacket : BasePacket
    {
        private Int32 _featureCount = 0;
        private ArrayList _featureList = new ArrayList();
        public DescriptionPacket() { }
        public DescriptionPacket(byte[] b) : base(b)
        {
            int offset = BASESIZE;
            this._featureCount = BitConverter.ToInt32(b, offset); offset += 4;
            StandardFeature sf;
            //TODO include ExtendedFeature Options.
            for(int i=0; i<_featureCount; i++)
            {
                sf = new StandardFeature();
                offset = sf.SetBytes(b, offset);
                _featureList.Add(sf);
            }
            

        }
        public override byte[] GetBytes()
        {
            byte[] a = base.GetBytes();
            byte[] d = BitConverter.GetBytes(this._featureCount);
            byte[] b;
            byte[] c;
            c = new byte[a.Length + d.Length];
            System.Buffer.BlockCopy(a, 0, c, 0, a.Length);
            System.Buffer.BlockCopy(d, 0, c, a.Length, d.Length);
            a = c;
            for(int i=0; i<_featureList.Count; i++)
            {
                if(_featureList[i].GetType() == typeof(StandardFeature))
                {
                    b = ((StandardFeature)_featureList[i]).GetBytes();
                    c = new byte[a.Length + b.Length];
                    System.Buffer.BlockCopy(a, 0, c, 0, a.Length);
                    System.Buffer.BlockCopy(b, 0, c, a.Length, b.Length);
                    a = c;
                }
                
            }
            return a;
        }
        public void AddFeature(StandardFeature f)
        {
            _featureList.Add(f);
            _featureCount = _featureList.Count;
        }
        public ArrayList GetFeatures()
        {
            return _featureList;
        }
    }
}
