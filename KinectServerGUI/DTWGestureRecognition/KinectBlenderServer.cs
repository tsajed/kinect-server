using System.IO;
using System.Net;
using System;
using System.Threading;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;


namespace HCI2012
{
    public class KinectBlenderServer
    {
        private static KinectBlenderServer server;

        public static KinectBlenderServer getServer()
        {
            if (server == null)
                server = new KinectBlenderServer();
            return server;
        }



        TcpListener kinectBlenderServer;
        
        public static List<TcpClient> appList;

        

        //public static void Main()
        //{
        //    KinectBlenderServer server = new KinectBlenderServer();
        //}

        public KinectBlenderServer()
        {
            //create our nickname and nickname by connection variables
            appList = new List<TcpClient>(10);

            //create our TCPListener object
kinectBlenderServer = new System.Net.Sockets.TcpListener(IPAddress.Parse("127.0.0.1"), 7933);
            //kinectBlenderServer = new System.Net.Sockets.TcpListener(IPAddress.Parse("142.244.218.248"), 7933);
            //check to see if the server is running
            //while (true) do the commands
            while (true)
            {
                //start server
                kinectBlenderServer.Start();
                //check if there are any pending connection requests
                if (kinectBlenderServer.Pending())
                {
                    //if there are pending requests create a new connection
                    TcpClient clientConnection = kinectBlenderServer.AcceptTcpClient();
                    //display a message letting the user know they're connected
                    Console.WriteLine("You are now connected");
                    //create a new DoCommunicate Object
                    DoCommunicate comm = new DoCommunicate(clientConnection);
                }
            }
        }

        public static void PublishData(string msg)
        {
            //create our StreamWriter object
            StreamWriter writer;

            for (int i = 0; i < appList.Count; i++)
            {
                try
                {
                    //check if the message is empty, of the particular
                    //index of out array is null, if it is then continue
                    if (msg.Trim() == "" || appList[i] == null)
                        continue;
                    //Use the GetStream method to get the current memory
                    //stream for this index of our TCPClient array
                    writer = new StreamWriter(appList[i].GetStream());
                    //send our message
                    writer.WriteLine(msg);
                    //make sure the buffer is empty
                    writer.Flush();
                    //dispose of our writer
                    writer = null;
                }
                catch
                {
                    //KinectBlenderServer.appNameList.Remove(KinectBlenderServer.appNameByConnectList[tcpClient[i]]);
                    //KinectBlenderServer.appNameByConnectList.Remove(tcpClient[i]);
                }
            }
        }

    }
}
