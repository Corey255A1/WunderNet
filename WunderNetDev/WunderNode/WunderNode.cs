using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WunderNetNode
{
    public class WunderNode : WunderLayer
    {
        public WunderNode(string id): base(id)
        {
            this.SendOnline();
        }
        public WunderNode(string id, string ip, int port): base(id, ip, port)
        {
            this.SendOnline();
        }


    }
}
