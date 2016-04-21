#################################
# CMPUT302 W2012 - U of Alberta #
# Team Discover                 #
# Member: Tri Lai               #
#         Daniel Kritsky        #
#         Dana Hurlbut          #
#         Tanvir Sajed          #
#################################

Software Requirements:
Microsoft Visual Studio 2010
Microsoft Kinect SDK v1.0
Microsoft Speech Runtime 11.0
Blender

System Requirements:
At least Intel Core2Duo 2.80GHz
4GB RAM
3MB Harddisk for the source code.

HOW TO USE
This is a server-client application. The server code is compressed in the file KinectServerGUI.zip

To start the server:
1) Extract the compressed file.
2) Double-click "DTWGestureRecognition.sln" to open the C# solution in Microsoft Visual Studio 2010.
3) Choose the menu "Debug" -> "Start debugging" to run the server. This can be done by pressing the shortcut key F5.
Please wait for enough time for the server to start. After the depth video appears in the lower right corner of the screen, start the client.

To start the client:
1) Download navigation.blend and object_manipulation.blend from the dropbox
   _ object_manipulation.blend contains the python code for manipulating 3D object
   _ navigation.blend contains the python code of navigating through a corridor
2) Double-click the .blend files desired modules
3) Press P to start the Blender's built-in engine
4) Start navigating/manipulating using your hand!

How to use your hands? Try to use it intuitively! Or look at our demo video in the shared dropbox folder.