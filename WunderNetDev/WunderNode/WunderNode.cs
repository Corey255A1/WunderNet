/**
 * Author: Corey Wunderlich
 * Date: 4/7/2016
 * Description: The WunderNode. This handles even higher level
 * stuff such as, handling the features the object has, handling
 * the subscriptions and eventually other goodness.
 * It is an extension of the WunderLayer and implements
 * the process Describe and Subscribe functions.
 * 
 */

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WunderNetNode
{
    public class WunderNode : WunderLayer
    {
        private Hashtable FeatureList = new Hashtable();
        private Hashtable FeatureSubscribers = new Hashtable();
        private Hashtable SubscribedFeatures = new Hashtable();
        public WunderNode(string id): base(id)
        {
            this.SendOnline();
        }
        public WunderNode(string id, string ip, int port): base(id, ip, port)
        {
            this.SendOnline();
        }

        public void AddFeature(string name, FeatureBaseTypes type, FeatureIOTypes io)
        {
            FeatureList.Add(name, new StandardFeature(name, type, io));
            FeatureSubscribers.Add(name, new ArrayList());
        }
        public void SubscribeToFeature(string receiver, string name)
        {
            if (SubscribedFeatures.ContainsKey(receiver))
            {
                ((ArrayList)SubscribedFeatures[receiver]).Add(name);
            }
            else
            {
                SubscribedFeatures.Add(receiver, new ArrayList());
                ((ArrayList)SubscribedFeatures[receiver]).Add(name);
            }
            SendFeatureSubscribe(receiver, name);
        }
        public void UpdateFeature(string name, object value)
        {
            if (FeatureList.ContainsKey(name))
            {
                if(((ArrayList)FeatureSubscribers[name]).Count > 0)
                {
                    StandardFeature sf = ((StandardFeature)FeatureList[name]);
                    switch ((FeatureBaseTypes)sf.FeatureBaseType)
                    {
                        case FeatureBaseTypes.INT: SendFeatureUpdate("", name, Convert.ToUInt32(value)); break;
                        case FeatureBaseTypes.BOOL: SendFeatureUpdate("", name, Convert.ToBoolean(value)); break;
                        case FeatureBaseTypes.STRING: SendFeatureUpdate("", name, Convert.ToString(value)); break;
                    }
                }
            }
            
        }

        protected override void ProcessDescribe(BasePacket bp)
        {
            if (bp.ReceiverID == this.Identifer)
            {
                SendDescription(bp.SenderID, this.FeatureList.Values);
            }
        }
        protected override void ProcessSubscribe(BasePacket bp, byte[] rawBytes)
        {
            if (bp.ReceiverID == this.Identifer)
            {
                FeaturePacket fp = new FeaturePacket(rawBytes);
                if(FeatureSubscribers.ContainsKey(fp.FeatureName))
                {
                    ArrayList al = ((ArrayList)FeatureSubscribers[fp.FeatureName]);
                    if (!al.Contains(fp.SenderID)) al.Add(fp.SenderID);
                }
            }
        }
        protected bool IsSubscribedToFeature(string sender, string featurename)
        {
            if (SubscribedFeatures.ContainsKey(sender))
            {
                if (((ArrayList)SubscribedFeatures[sender]).Contains(featurename))
                {
                    return true;
                }
            }
            return false;
        }
        protected override void ProcessFeatureUpdate(BasePacket bp, byte[] rawBytes)
        {
            FeaturePacket dp = new FeaturePacket(rawBytes);
            if (IsSubscribedToFeature(dp.SenderID, dp.FeatureName))
            {
                ProcessFeatureUpdateCallbacks(dp);
            }
        }

    }
}
