/**
 * Author: Corey Wunderlich
 * Date: 3/25/2016
 * Description: Simple UDP Network Layer setup for node cluster communication
 * I think that's what it would be called. Sends Multicast packets,
 * receives packets and notifies listeners on seperate threads.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
namespace WunderNetLayer
{

    public class WunderNetPacketEventArgs : EventArgs
    {
        public byte[] RawData;
    }
   
    class WunderNet
    {
        private Socket _TX;
        private UdpClient _RX;
        private IPEndPoint _RXPort;
        private IPEndPoint _TXPort;
        private bool _bRunServer;
        private Thread _ListenThread;

        public event EventHandler<WunderNetPacketEventArgs> WunderNetEvent;

        public static byte[] ConvertObjToBytes(object o)
        {
            int s = Marshal.SizeOf(o);
            byte[] b = new byte[s];
            IntPtr p = Marshal.AllocHGlobal(s);
            Marshal.StructureToPtr(o, p, true);
            Marshal.Copy(p, b, 0, s);
            Marshal.FreeHGlobal(p);
            return b;
        }
        public WunderNet()
        {
            _TX = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _bRunServer = false;
        }
        public void SetTXPort(int port)
        {
            IPAddress[] thisIps = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress ip in thisIps)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    string strip = ip.ToString();
                    strip = (strip.Remove(strip.LastIndexOf('.')) + ".255");
                    _TXPort = new IPEndPoint(IPAddress.Parse(strip), port);
                    break;
                }
            }
        }
        public void SetTXPort(string ip, int port)
        {
            _TXPort = new IPEndPoint(IPAddress.Parse(ip), port);
        }
        public void StartListening(int port)
        {
            if (!_bRunServer)
            {
                _bRunServer = true;
                _RX = new UdpClient();
                _RX.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                _RXPort = new IPEndPoint(IPAddress.Any, port);
                _RX.Client.Bind(_RXPort);
                
                _ListenThread = new Thread(PacketReader);
                _ListenThread.Start();
            }

        }
        public void StopListening()
        {
            if(_bRunServer)
            {
                _bRunServer = false;
                Console.WriteLine("Joining...");
                _ListenThread.Join();
                Console.WriteLine("Joined...");
            }
        }
        public void SendPacket(string packet)
        {
            if(_TXPort != null)
            {
                _TX.SendTo(Encoding.ASCII.GetBytes(packet),_TXPort);
            }

        }
        public void SendPacket(byte[] packet)
        {
            if (_TXPort != null)
            {
                _TX.SendTo(packet, _TXPort);
            }

        }
        public bool IsAlive()
        {
            return _bRunServer;
        }

        private void PacketReader()
        {
            byte[] packet;
            while(_bRunServer)
            {
                if(_RX.Available > 0)
                {
                    try
                    {
                        packet = _RX.Receive(ref _RXPort);
                        if (WunderNetEvent != null)
                        {
                            Delegate[] registeredEvents = WunderNetEvent.GetInvocationList();
                            WunderNetPacketEventArgs e = new WunderNetPacketEventArgs();
                            e.RawData = packet;
                            foreach (Delegate el in registeredEvents)
                            {
                                ((EventHandler<WunderNetPacketEventArgs>)el).BeginInvoke(this, e, EndPacketReaderCallback, null);
                            }
                        }
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
            _RX.Close();
            Console.WriteLine("Reader Exiting...");
        }
        private void EndPacketReaderCallback(IAsyncResult iares)
        {
            var ares = (System.Runtime.Remoting.Messaging.AsyncResult)iares;
            var theinvoked = (EventHandler<WunderNetPacketEventArgs>)ares.AsyncDelegate;
            try
            {
                theinvoked.EndInvoke(ares);
            }
            catch
            {
                Console.WriteLine("Cannot End Thread Invocation");
            }
        }
    }
}
