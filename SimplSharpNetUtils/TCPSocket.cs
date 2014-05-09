/* TCPSocket.cs
 * 
 * This is a simple TCP Client bridge between SimplSharp and Simpl+
 * 
 * Note that the FilterVtCmds flag tries to filter extra control characters and VT Esc characters from
 * the received content.
 *
 */



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;

namespace SimplSharpNetUtils
{
    public class TCPSocket
    {
        TCPClient client;
        bool bFilterVtCmds = false;

        public delegate void ConnectedHandler();
        public ConnectedHandler OnConnect { set; get; }

        public delegate void DisconnectedHandler();
        public DisconnectedHandler OnDisconnect { set; get; }

        public delegate void RxHandler(SimplSharpString data);
        public RxHandler OnRx { set; get; }

        public int FilterVtCmds
        {
            set { bFilterVtCmds = value != 0; }
            get { return bFilterVtCmds ? 1 : 0; }
        }

        public TCPSocket()
        {
        }

        public int Connect(String IPAddress, int port, int buffersz)
        {
            CrestronConsole.PrintLine("Connect({0},{1},{2})", IPAddress, port, buffersz);
            client = new TCPClient(IPAddress, port, buffersz);

            SocketErrorCodes err = client.ConnectToServerAsync(myConnectCallback);

            CrestronConsole.PrintLine("Connect() - " + err);

            return Convert.ToInt32(err);
        }

        public void Disconnect()
        {
            CrestronConsole.PrintLine("Disconnect()");
            if (client != null)
                client.DisconnectFromServer();
        }

        String DoFilterVtCmds(String sIn)
        {
            StringBuilder sOut = new StringBuilder();

            bool IsVt = false;

            foreach (char c in sIn)
            {
                if (c < 127)
                {
                    if (IsVt)
                    {
                        if ((c == 'h') || (c == 'H'))
                            IsVt = false;
                    }
                    else
                        if (c == (char) 0x1B)
                            IsVt = true;
                        else
                            if (!(Char.IsControl(c) && (c != (char) 0x0d) && (c != (char) 0x0a)))
                                sOut.Append(c);
                }
            }
            
            return (sOut.ToString());
        }

        void myReceiveCallback(TCPClient tcpClient, int rxSize)
        {
            CrestronConsole.PrintLine("Received " + rxSize);
            String rxBuff = System.Text.Encoding.Default.GetString(tcpClient.IncomingDataBuffer, 0, rxSize);

            if (rxSize == 0)
            {
                CrestronConsole.PrintLine("Size triggers Disconnect");
                if (OnDisconnect != null)
                    OnDisconnect();
            }
            else
            {
                if (bFilterVtCmds)
                    rxBuff = DoFilterVtCmds(rxBuff);

                if (rxBuff.Length > 0)
                {
                    if (OnRx != null)
                        OnRx(new SimplSharpString(rxBuff));
                    client.ReceiveDataAsync(myReceiveCallback);
                }
            }
        }

        void myConnectCallback(TCPClient tcpClient)
        {
            CrestronConsole.PrintLine("Connect Callback " + tcpClient.ClientStatus);

            if (tcpClient.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                if (OnConnect != null)
                    OnConnect();
                client.ReceiveDataAsync(myReceiveCallback);
            }
            else
            {
                if (OnDisconnect != null)
                    OnDisconnect();
            }
        }

        public int Send(SimplSharpString data)
        {
            if (client != null)
            {
                byte[] db = System.Text.Encoding.ASCII.GetBytes(data.ToString());
                SocketErrorCodes err = client.SendData(db, db.Length);
                CrestronConsole.PrintLine("Send() = " + err);
                return Convert.ToInt32(err);
            }
            else
                CrestronConsole.PrintLine("Called Send() on a Null client!");
            return -1;
        }


    }
}