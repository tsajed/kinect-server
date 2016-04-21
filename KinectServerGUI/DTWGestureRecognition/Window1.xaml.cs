using System.Threading;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Rhemyst and Rymix">
//     Open Source. Do with this as you will. Include this statement or 
//     don't - whatever you like.
//
//     No warranty or support given. No guarantees this will work or meet
//     your needs. Some elements of this project have been tailored to
//     the authors' needs and therefore don't necessarily follow best
//     practice. Subsequent releases of this project will (probably) not
//     be compatible with different versions, so whatever you do, don't
//     overwrite your implementation with any new releases of this
//     project!
//
//     Enjoy working with Kinect!
// </copyright>
//-----------------------------------------------------------------------

namespace HCI2012
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;
    using System.Linq;
    using Microsoft.Kinect;
    using System.IO;
    using System.Windows.Controls;
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Window1
    {

        Boolean isMoving = false;
        /// <summary>
        /// To do voice recognizing
        /// </summary>
        private SpeechRecognitionEngine speechRecognizer;

        //EnergyCalculatingPassThroughStream stream;

        KinectAudioSource kinectSource;

        Stream st;

        /// <summary>
        /// Server for networking
        /// </summary>
        private KinectBlenderServer networkServer;

        // We want to control how depth data gets converted into false-color data
        // for more intuitive visualization, so we keep 32-bit color frame buffer versions of
        // these, to be updated whenever we receive and process a 16-bit frame.

        /// <summary>
        /// The red index
        /// </summary>
        private const int RedIdx = 2;

        /// <summary>
        /// The green index
        /// </summary>
        private const int GreenIdx = 1;

        /// <summary>
        /// The blue index
        /// </summary>
        private const int BlueIdx = 0;


        /// <summary>
        /// Dictionary of all the joints Kinect SDK is capable of tracking. You might not want always to use them all but they are included here for thouroughness.
        /// </summary>
        private readonly Dictionary<JointType, Brush> _jointColors = new Dictionary<JointType, Brush>
        { 
            {JointType.HipCenter, new SolidColorBrush(Color.FromRgb(169, 176, 155))},
            {JointType.Spine, new SolidColorBrush(Color.FromRgb(169, 176, 155))},
            {JointType.ShoulderCenter, new SolidColorBrush(Color.FromRgb(168, 230, 29))},
            {JointType.Head, new SolidColorBrush(Color.FromRgb(200, 0, 0))},
            {JointType.ShoulderLeft, new SolidColorBrush(Color.FromRgb(79, 84, 33))},
            {JointType.ElbowLeft, new SolidColorBrush(Color.FromRgb(84, 33, 42))},
            {JointType.WristLeft, new SolidColorBrush(Color.FromRgb(255, 126, 0))},
            {JointType.HandLeft, new SolidColorBrush(Color.FromRgb(215, 86, 0))},
            {JointType.ShoulderRight, new SolidColorBrush(Color.FromRgb(33, 79,  84))},
            {JointType.ElbowRight, new SolidColorBrush(Color.FromRgb(33, 33, 84))},
            {JointType.WristRight, new SolidColorBrush(Color.FromRgb(77, 109, 243))},
            {JointType.HandRight, new SolidColorBrush(Color.FromRgb(37,  69, 243))},
            {JointType.HipLeft, new SolidColorBrush(Color.FromRgb(77, 109, 243))},
            {JointType.KneeLeft, new SolidColorBrush(Color.FromRgb(69, 33, 84))},
            {JointType.AnkleLeft, new SolidColorBrush(Color.FromRgb(229, 170, 122))},
            {JointType.FootLeft, new SolidColorBrush(Color.FromRgb(255, 126, 0))},
            {JointType.HipRight, new SolidColorBrush(Color.FromRgb(181, 165, 213))},
            {JointType.KneeRight, new SolidColorBrush(Color.FromRgb(71, 222, 76))},
            {JointType.AnkleRight, new SolidColorBrush(Color.FromRgb(245, 228, 156))},
            {JointType.FootRight, new SolidColorBrush(Color.FromRgb(77, 109, 243))}
        };

        /// <summary>
        /// The depth frame byte array. Only supports 320 * 240 at this time
        /// </summary>
        private readonly short[] _depthFrame32 = new short[320 * 240 * 4];
        /// <summary>
        /// How many frames occurred 'last time'. Used for calculating frames per second
        /// </summary>
        private int _lastFrames;

        /// <summary>
        /// The 'last time' DateTime. Used for calculating frames per second
        /// </summary>
        private DateTime _lastTime = DateTime.MaxValue;

        /// <summary>
        /// The Natural User Interface runtime
        /// </summary>
        private KinectSensor  kinectSensorT;

        /// <summary>
        /// Total number of framed that have occurred. Used for calculating frames per second
        /// </summary>
        private int _totalFrames;


        /// <summary>
        /// ArrayList of coordinates which are recorded in sequence to define one gesture
        /// </summary>
        private ArrayList _video;

        /// <summary>
        /// ArrayList of coordinates which are recorded in sequence to define one gesture
        /// </summary>
        private DateTime _captureCountdown = DateTime.Now;

        /// <summary>
        /// Initializes a new instance of the MainWindow class
        /// </summary>
        public Window1()
        {
            InitializeComponent();
        }

 
        /// <summary>
        /// Called each time a skeleton frame is ready. Passes skeletal data to the DTW processor
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Skeleton Frame Ready Event Args</param>
        private static void SkeletonExtractSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            //System.Windows.MessageBox.Show("SkeletonExtractSkeletonFrameReady");
            SkeletonFrame skeletonFrame = e.OpenSkeletonFrame();
            //using (var skeletonFrame = e.OpenSkeletonFrame())
            
                if (skeletonFrame == null) return; // sometimes frame image comes null, so skip it.
                var skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                skeletonFrame.CopySkeletonDataTo(skeletons);

                foreach (Skeleton data in skeletons)
                {
                    if (data.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        Skeleton normalizedData  = Skeleton2DDataExtract.NormalizeDataUseShoulder(data);
                        }
                }

        }

        /// <summary>
        /// Converts a 16-bit grayscale depth frame which includes player indexes into a 32-bit frame that displays different players in different colors
        /// </summary>
        /// <param name="depthFrame16">The depth frame byte array</param>
        /// <returns>A depth frame byte array containing a player image</returns>
        private short[] ConvertDepthFrame(short[] depthFrame16)
        {
            for (int i16 = 0, i32 = 0; i16 < depthFrame16.Length && i32 < _depthFrame32.Length; i16 += 2, i32 += 4)
            {
                int player = depthFrame16[i16] & 0x07;
                int realDepth = (depthFrame16[i16 + 1] << 5) | (depthFrame16[i16] >> 3);
                
                // transform 13-bit depth information into an 8-bit intensity appropriate
                // for display (we disregard information in most significant bit)
                var intensity = (short)(255 - (255 * realDepth / 0x0fff));

                _depthFrame32[i32 + RedIdx] = 0;
                _depthFrame32[i32 + GreenIdx] = 0;
                _depthFrame32[i32 + BlueIdx] = 0;

                // choose different display colors based on player
                switch (player)
                {
                    case 0:
                        _depthFrame32[i32 + RedIdx] = (byte)(intensity / 2);
                        _depthFrame32[i32 + GreenIdx] = (byte)(intensity / 2);
                        _depthFrame32[i32 + BlueIdx] = (byte)(intensity / 2);
                        break;
                    case 1:
                        _depthFrame32[i32 + RedIdx] = intensity;
                        break;
                    case 2:
                        _depthFrame32[i32 + GreenIdx] = intensity;
                        break;
                    case 3:
                        _depthFrame32[i32 + RedIdx] = (byte)(intensity / 4);
                        _depthFrame32[i32 + GreenIdx] = intensity;
                        _depthFrame32[i32 + BlueIdx] = intensity;
                        break;
                    case 4:
                        _depthFrame32[i32 + RedIdx] = intensity;
                        _depthFrame32[i32 + GreenIdx] = intensity;
                        _depthFrame32[i32 + BlueIdx] = (byte)(intensity / 4);
                        break;
                    case 5:
                        _depthFrame32[i32 + RedIdx] = intensity;
                        _depthFrame32[i32 + GreenIdx] = (byte)(intensity / 4);
                        _depthFrame32[i32 + BlueIdx] = intensity;
                        break;
                    case 6:
                        _depthFrame32[i32 + RedIdx] = (byte)(intensity / 2);
                        _depthFrame32[i32 + GreenIdx] = (byte)(intensity / 2);
                        _depthFrame32[i32 + BlueIdx] = intensity;
                        break;
                    case 7:
                        _depthFrame32[i32 + RedIdx] = (byte)(255 - intensity);
                        _depthFrame32[i32 + GreenIdx] = (byte)(255 - intensity);
                        _depthFrame32[i32 + BlueIdx] = (byte)(255 - intensity);
                        break;
                }
            }

            return _depthFrame32;
        }

        /// <summary>
        /// Called when each depth frame is ready
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Image Frame Ready Event Args</param>
        private void NuiDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            //PlanarImage image = e.ImageFrame.Image;
            using (var image = e.OpenDepthImageFrame())
            {
                if (image == null) return; // sometimes frame image comes null, so skip it.

                depthImage.Source = image.ToBitmapSource();
            }
            ++_totalFrames;

            DateTime cur = DateTime.Now;
            if (cur.Subtract(_lastTime) > TimeSpan.FromSeconds(1))
            {
                int frameDiff = _totalFrames - _lastFrames;
                _lastFrames = _totalFrames;
                _lastTime = cur;
                //frameRate.Text = frameDiff + " fps";
            }
        }

        /// <summary>
        /// Gets the display position (i.e. where in the display image) of a Joint
        /// </summary>
        /// <param name="joint">Kinect NUI Joint</param>
        /// <returns>Point mapped location of sent joint</returns>
        private Point GetDisplayPosition(Joint joint)
        {
            float depthX, depthY;
            var pos = kinectSensorT.MapSkeletonPointToDepth(joint.Position, DepthImageFormat.Resolution320x240Fps30);

            depthX = pos.X;
            depthY = pos.Y;

            int colorX, colorY;

            // Only ImageResolution.Resolution640x480 is supported at this point
            var pos2 = kinectSensorT.MapSkeletonPointToColor(joint.Position, ColorImageFormat.RgbResolution640x480Fps30);
            colorX = pos2.X;
            colorY = pos2.Y;

            // Map back to skeleton.Width & skeleton.Height
            return new Point((int)(skeletonCanvas.Width * colorX / 640.0), (int)(skeletonCanvas.Height * colorY / 480));
        }

        /// <summary>
        /// Works out how to draw a line ('bone') for sent Joints
        /// </summary>
        /// <param name="joints">Kinect NUI Joints</param>
        /// <param name="brush">The brush we'll use to colour the joints</param>
        /// <param name="ids">The JointsIDs we're interested in</param>
        /// <returns>A line or lines</returns>
        private Polyline GetBodySegment(JointCollection joints, Brush brush, params JointType[] ids)
        {
            var points = new PointCollection(ids.Length);
            foreach (JointType t in ids)
            {
                points.Add(GetDisplayPosition(joints[t]));
            }

            var polyline = new Polyline();
            polyline.Points = points;
            polyline.Stroke = brush;
            polyline.StrokeThickness = 5;
            return polyline;
        }

        /// <summary>
        /// Runds every time a skeleton frame is ready. Updates the skeleton canvas with new joint and polyline locations.
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Skeleton Frame Event Args</param>
        private void NuiSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons;
            using (var frame = e.OpenSkeletonFrame())
            {
                if (frame == null) return;
                skeletons = new Skeleton[frame.SkeletonArrayLength];
                frame.CopySkeletonDataTo(skeletons);
            }

            int iSkeleton = 0;
            var brushes = new Brush[6];
            brushes[0] = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            brushes[1] = new SolidColorBrush(Color.FromRgb(0, 255, 0));
            brushes[2] = new SolidColorBrush(Color.FromRgb(64, 255, 255));
            brushes[3] = new SolidColorBrush(Color.FromRgb(255, 255, 64));
            brushes[4] = new SolidColorBrush(Color.FromRgb(255, 64, 255));
            brushes[5] = new SolidColorBrush(Color.FromRgb(128, 128, 255));

            skeletonCanvas.Children.Clear();
            foreach (var data in skeletons)
            {
                if (SkeletonTrackingState.Tracked == data.TrackingState)
                {
                    // Draw bones
                    Brush brush = brushes[iSkeleton % brushes.Length];
                    skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointType.HipCenter, JointType.Spine, JointType.ShoulderCenter, JointType.Head));
                    skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointType.ShoulderCenter, JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft, JointType.HandLeft));
                    skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointType.ShoulderCenter, JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, JointType.HandRight));
                    skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointType.HipCenter, JointType.HipLeft, JointType.KneeLeft, JointType.AnkleLeft, JointType.FootLeft));
                    skeletonCanvas.Children.Add(GetBodySegment(data.Joints, brush, JointType.HipCenter, JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, JointType.FootRight));

                    // Draw joints
                    foreach (Joint joint in data.Joints)
                    {
                        Point jointPos = GetDisplayPosition(joint);
                        var jointLine = new Line();
                        jointLine.X1 = jointPos.X - 3;
                        jointLine.X2 = jointLine.X1 + 6;
                        jointLine.Y1 = jointLine.Y2 = jointPos.Y;
                        jointLine.Stroke = _jointColors[joint.JointType];
                        jointLine.StrokeThickness = 6;
                        skeletonCanvas.Children.Add(jointLine);
                    }
                }

                iSkeleton++;
            } // for each skeleton
        }

        /// <summary>
        /// Called every time a video (RGB) frame is ready
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Image Frame Ready Event Args</param>
        private void NuiColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            // 32-bit per pixel, RGBA image
            using (var image = e.OpenColorImageFrame())
            {
                if (image == null) return; // sometimes frame image comes null, so skip it.

                //colorImage.Source = image.ToBitmapSource();
            }
        }

        /// <summary>
        /// Runs after the window is loaded
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Routed Event Args</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            kinectSensorT = (from i in KinectSensor.KinectSensors
                    where i.Status == KinectStatus.Connected
                    select i).FirstOrDefault();

            if (kinectSensorT == null)
                throw new NotSupportedException("No kinectes connected!");

            try
            {
                kinectSensorT.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
                kinectSensorT.SkeletonStream.Enable();
                //kinectSensorT.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                kinectSensorT.SkeletonStream.Enable(new TransformSmoothParameters()
                                             {
                                                 Smoothing = 0.7f,
                                                 Correction = 0.5f,
                                                 Prediction = 0.5f,
                                                 JitterRadius = 0.01f,
                                                 MaxDeviationRadius = 0.04f
                                             });


                kinectSensorT.Start();
            }
            catch (InvalidOperationException)
            {
                System.Windows.MessageBox.Show("Runtime initialization failed. Please make sure Kinect device is plugged in.");
                return;
            }

            _lastTime = DateTime.Now;

            //_dtw = new DtwGestureRecognizer(12, 0.6, 2, 2, 10);
            _video = new ArrayList();

            // If you want to see the depth image and frames per second then include this
            // I'mma turn this off 'cos my 'puter is proper slow
            
            kinectSensorT.DepthFrameReady += NuiDepthFrameReady;

            kinectSensorT.SkeletonFrameReady += NuiSkeletonFrameReady;
            kinectSensorT.SkeletonFrameReady += SkeletonExtractSkeletonFrameReady;

            // If you want to see the RGB stream then include this
            kinectSensorT.ColorFrameReady += NuiColorFrameReady;

            //NuiSkeleton2DdataCoordReady will handle the event fire from Skeleton2DdataCoordReady event
            Skeleton2DDataExtract.SkeletonNormalizedDataReady += NuiSkeletonNormalizedDataCoordReady;


            CreateSpeechRecognizer();

            
            // Update the debug window with Sequences information
            //dtwTextOutput.Text = _dtw.RetrieveText();

            Debug.WriteLine("Finished Window Loading");

            Thread networkServerThread = new Thread(new ThreadStart(runServer));
            //start the new thread
            networkServerThread.Start();          
        }

        private void runServer()
        {
            try
            {
                networkServer = KinectBlenderServer.getServer();
            }
            catch
            {
                
            }
            
        }

        /// <summary>
        /// Runs some tidy-up code when the window is closed. This is especially important for our NUI instance because the Kinect SDK is very picky about this having been disposed of nicely.
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Event Args</param>
        private void WindowClosed(object sender, EventArgs e)
        {
            Debug.WriteLine("Stopping NUI");
            kinectSensorT.Stop();
            Debug.WriteLine("NUI stopped");
            Environment.Exit(0);
        }

        static SkeletonPoint rightHand = new SkeletonPoint();
        static SkeletonPoint leftHand = new SkeletonPoint();
        static bool isSynchronized = false;
        static bool isSynchronizing = false;

        private void NuiSkeletonNormalizedDataCoordReady(object sender, SkeletonNormalizedDataEventArgs a)
        {
            //System.Windows.MessageBox.Show("Event fired");
            Skeleton data = a.GetSkeleton();


            //double winLeft = canvas1.Margin.Left;
            //double winHeight = canvas1.RenderSize.Height;
            //double winWidth = canvas1.RenderSize.Width;

            double left = data.Joints[JointType.HandRight].Position.X;
            double top = 0.5 - data.Joints[JointType.HandRight].Position.Y;
            //double depth = data.Joints[JointType.HandRight].Position.Z;



            //double labelLeft = Math.Max(Math.Min(left * winWidth, winWidth), 0);
            //double labelTop = Math.Max(Math.Min(top * winHeight, winHeight), 0);

            //double zUp = 0.5 - data.Joints[JointType.HandLeft].Position.Y;
            double deltaLeftZ = ((data.Joints[JointType.HandLeft].Position.Z - leftHand.Z) * 5);


            //double newDepth = deltaLeftZ;//((depth - rightHand.Z))*2;
            //double labelTopZ = Math.Max(Math.Min(zUp * winHeight, winHeight), 0);

            //System.Windows.Controls.Label newLbl = labelCursor;


            //labelCursor.Margin = new Thickness(0, 0, 10, 10);
            //labelCursor.Margin = new Thickness(labelLeft,
            //                                    labelTop - top,
            //                                    0,
            //                                    0);

            //labelLeftZ.Margin = new Thickness(winLeft,
            //                                        labelTopZ - top,
            //                                        0,
            //                                        0);



            //labelLeftZ.Width += newDepth2;
            //labelCursor.Width = Math.Min(100, Math.Max(10, labelCursor.Width + newDepth));
            //labelLeftZ.Height += newDepth2;
            //labelCursor.Height = Math.Min(100, Math.Max(10, labelCursor.Height + newDepth));



            //labelCursor.Width = Math.Min(30, Math.Max(10, labelCursor.Width));



            if (isSynchronizing)
            {
                isSynchronized = true;
                //labelCenter.Margin = new Thickness(labelLeft,
                //                                    0,//labelTop - top,
                //                                    0,
                //                                    0);
                //labelZ.Margin = new Thickness(winLeft,
                //                                    labelTopZ - top,
                //                                    0,
                //                                    0);
                rightHand = data.Joints[JointType.HandRight].Position;
                leftHand = data.Joints[JointType.HandLeft].Position;

                isSynchronizing = false;
            }

            if (isSynchronized)
            {
                left -= rightHand.X;
                top -= rightHand.Y;
                deltaLeftZ -= rightHand.Z;

            }

            //left = normal((left * 2) - 1);
            //top = normal((top * 2) - 1);
            //left = normal((left * 2) - 1);
            deltaLeftZ = normal(deltaLeftZ);

            if (top > 0)
            {

            }
            else if (top < 0)
            {
                top *= 1.2;
            }

            if (left < 0)
            {
                left *= 1.1;
            }

            //left = left - 0.5;
            //left *= 2.0;

            lblCoordinate.Content = "x= " + left.ToString("#.##");
            lblCoord2.Content = "y= " + top.ToString("#.##");
            labelVoice.Content = "z= " + deltaLeftZ.ToString("#.##");

            KinectBlenderServer.PublishData(voiceCommand + "," + left + "," + top + "," + deltaLeftZ);
            //voiceCommand = "";

        }

        private static double normal(double a)
        {
            double calc = Math.Max(-1, Math.Min(1, a));
            return calc;//deadzone(calc);
        }

        private static double deadzone(double a)
        {
            double limit = 0.10;

            if(a < limit && a > -limit) return 0;
            else return a;
        }

        private static RecognizerInfo GetKinectRecognizer()
        {
            Func<RecognizerInfo, bool> matchingFunc = r =>
            {
                string value;
                r.AdditionalInfo.TryGetValue("Kinect", out value);
                return "True".Equals(value, StringComparison.InvariantCultureIgnoreCase) && "en-US".Equals(r.Culture.Name, StringComparison.InvariantCultureIgnoreCase);
            };
            return SpeechRecognitionEngine.InstalledRecognizers().Where(matchingFunc).FirstOrDefault();
        }

        private SpeechRecognitionEngine CreateSpeechRecognizer()
        {

            RecognizerInfo ri = GetKinectRecognizer();

            if (ri == null)
            {
                System.Windows.MessageBox.Show(
                    @"There was a problem initializing Speech Recognition.
Ensure you have the Microsoft Speech SDK installed.",
                    "Failed to load Speech SDK",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                this.Close();
                return null;
            }

            //SpeechRecognitionEngine sre;
            try
            {
                speechRecognizer = new SpeechRecognitionEngine(ri.Id);
                //speechRecognizer = SelectRecognizer("Kinect");
            }
            catch
            {
                System.Windows.MessageBox.Show(
                    @"There was a problem initializing Speech Recognition.
Ensure you have the Microsoft Speech SDK installed and configured.",
                    "Failed to load Speech SDK",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                this.Close();
                return null;
            }

            var grammar = new Choices();
            grammar.Add("start");
            grammar.Add("pick");
            grammar.Add("select");
            grammar.Add("a");
            grammar.Add("navigate");
            grammar.Add("b");
            grammar.Add("move");
            grammar.Add("stop");
            grammar.Add("hold");
            grammar.Add("synchronize");

            var gb = new GrammarBuilder {};
            gb.Append(grammar);

            // Create the actual Grammar instance, and then load it into the speech recognizer.
            var g = new Grammar(gb);

            speechRecognizer.LoadGrammar(g);
            speechRecognizer.SpeechRecognized += this.SreSpeechRecognized;
            speechRecognizer.SpeechHypothesized += SreSpeechHypothesized;
            speechRecognizer.SpeechRecognitionRejected += SreSpeechRecognitionRejected;

            kinectSource = kinectSensorT.AudioSource;
            kinectSource.BeamAngleMode = BeamAngleMode.Adaptive;
            kinectSource.EchoCancellationMode = EchoCancellationMode.None; // No AEC for this sample
            kinectSource.AutomaticGainControlEnabled = false; // Important to turn this off for speech recognition

            st = kinectSource.Start();

            //System.Windows.MessageBox.Show("audio source started");

            speechRecognizer.SetInputToAudioStream(st,
                  new SpeechAudioFormatInfo(
                      EncodingFormat.Pcm, 16000, 16, 1,
                      32000, 2, null));
            speechRecognizer.RecognizeAsync(RecognizeMode.Multiple);

            return speechRecognizer;
        }

        public static string voiceCommand = "";

        private void SreSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            //System.Windows.MessageBox.Show("Recognized");

            label2.Content = "" + e.Result.Confidence.ToString("#.##");

            if (e.Result.Confidence < 0.6)
            {
                string result = e.Result.Text.ToUpperInvariant();
                labelVoice.Content = result + "/" + e.Result.Confidence.ToString();
            }
            else
            {
                string result = e.Result.Text.ToUpperInvariant();
                voiceCommand = result;
                if (result.ToUpper() == "synchronize".ToUpper())
                {
                    isSynchronizing = true;
                    isMoving = true;
                } else if (result.ToUpper() == "stop".ToUpper())
                {
                    isSynchronizing = true;
                    isMoving = false;
                }
                labelVoice.Content = result + "/" + e.Result.Confidence.ToString(); ;
              

                //KinectBlenderServer.PublishData("Voice command "+ result + " - Confidence score: "+e.Result.Confidence);
                //lblCoordinate.Content = "Voice command "+ result + " - Confidence score: "+e.Result.Confidence;

            }

        }

        private static void SreSpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            //System.Windows.MessageBox.Show("Speech rejected");
        }

        private static void SreSpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            //System.Windows.MessageBox.Show("Speech Hypothesized");
        }

    
    }
}

