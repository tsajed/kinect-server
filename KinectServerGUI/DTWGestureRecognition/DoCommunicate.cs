using System.IO;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Collections;
using HCI2012;

namespace HCI2012
{
    class DoCommunicate
    {
        TcpClient client;
        StreamReader reader;
        StreamWriter writer;

        public DoCommunicate(System.Net.Sockets.TcpClient tcpClient)
        {
            //create our TcpClient
            client = tcpClient;
            //create a new thread
            Thread dataStreamThread = new Thread(new ThreadStart(startDataStream));
            //start the new thread
            dataStreamThread.Start();
        }



        //private void runServer()
        //    //use a try...catch to catch any exceptions
        //{
        //    try
        //    {
        //        //set out line variable to an empty string
        //        string line = "";
        //        while (true)
        //        {
        //            line = reader.ReadLine();
        //            //send our message to all clients, msg source: appName
        //            HCI2012.KinectBlenderServer.PublishData(line);
        //        }
        //    }
        //    catch (Exception e44) 
        //    { 
        //        Console.WriteLine(e44); 
        //    }
        //}

        private void startDataStream()
        {
            //create our StreamReader object to read the current stream
            reader = new StreamReader(client.GetStream());
            //create our StreamWriter objec to write to the current stream
            writer = new StreamWriter(client.GetStream());
            writer.WriteLine("Client connected");
            //retrieve the appName they provided

                        
            HCI2012.KinectBlenderServer.appList.Add(client);

            //send a system message letting the other user
            //know that a new user has joined the chat
//This is where the server announce new app has connected to the server
            HCI2012.KinectBlenderServer.PublishData("An app has connected the server");

            ////create a new thread for this user
            //Thread dataStreamThread = new Thread(new ThreadStart(runServer));
            ////start the thread
            //dataStreamThread.Start();
        }
    }
}

