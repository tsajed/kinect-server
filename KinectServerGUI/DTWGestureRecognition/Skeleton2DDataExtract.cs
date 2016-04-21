//-----------------------------------------------------------------------
// <copyright file="Skeleton2DDataExtract.cs" company="Rhemyst and Rymix">
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
    using System.Windows;
    using Microsoft.Kinect;
    using System.Windows.Controls;

    /// <summary>
    /// This class is used to transform the data of the skeleton
    /// </summary>
    internal class Skeleton2DDataExtract
    {
        /// <summary>
        /// Skeleton2DdataCoordEventHandler delegate
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="a">Skeleton 2Ddata Coord Event Args</param>
        //public delegate void Skeleton2DdataCoordEventHandler(object sender, Skeleton2DdataCoordEventArgs a);


        public delegate void SkeletonNormalizedDataEventHandler(object sender, SkeletonNormalizedDataEventArgs a);

        /// <summary>
        /// The Skeleton 2Ddata Coord Ready event
        /// </summary>
        //public static event Skeleton2DdataCoordEventHandler Skeleton2DdataCoordReady;

        public static event SkeletonNormalizedDataEventHandler SkeletonNormalizedDataReady;

        /// <summary>
        /// Crunches Kinect SDK's Skeleton Data and spits out a format more useful for DTW
        /// </summary>
        /// <param name="data">Kinect SDK's Skeleton Data</param>
        public static void ProcessData(Skeleton data)
        {
            if (data.TrackingState == Microsoft.Kinect.SkeletonTrackingState.Tracked)
            {
                // Extract the coordinates of the points.
                var p = new Point[6];
                Point shoulderRight = new Point(), shoulderLeft = new Point();

                foreach (Joint j in data.Joints)
                {
                    switch (j.JointType)
                    {
                        case JointType.HandLeft:
                            p[0] = new Point(j.Position.X, j.Position.Y);
                            break;
                        case JointType.WristLeft:
                            p[1] = new Point(j.Position.X, j.Position.Y);
                            break;
                        case JointType.ElbowLeft:
                            p[2] = new Point(j.Position.X, j.Position.Y);
                            break;
                        case JointType.ElbowRight:
                            p[3] = new Point(j.Position.X, j.Position.Y);
                            break;
                        case JointType.WristRight:
                            p[4] = new Point(j.Position.X, j.Position.Y);
                            break;
                        case JointType.HandRight:
                            p[5] = new Point(j.Position.X, j.Position.Y);
                            //KinectBlenderServer.PublishData(j.Position.Y + " A");
                            break;
                        case JointType.ShoulderLeft:
                            shoulderLeft = new Point(j.Position.X, j.Position.Y);

                            break;
                        case JointType.ShoulderRight:
                            shoulderRight = new Point(j.Position.X, j.Position.Y);
                            break;
                    }
                }

                // Centre the data
                var center = new Point((shoulderLeft.X + shoulderRight.X) / 2, (shoulderLeft.Y + shoulderRight.Y) / 2);
                for (int i = 0; i < 6; i++)
                {
                    p[i].X -= center.X;
                    p[i].Y -= center.Y;
                }

                // Normalization of the coordinates
                double shoulderDist =
                    Math.Sqrt(Math.Pow((shoulderLeft.X - shoulderRight.X), 2) +
                              Math.Pow((shoulderLeft.Y - shoulderRight.Y), 2));
                for (int i = 0; i < 6; i++)
                {
                    p[i].X /= shoulderDist;
                    p[i].Y /= shoulderDist;
                }

                var left = data.Joints[JointType.HandLeft];
                var right = data.Joints[JointType.HandRight];
                //var middle = data.Joints[JointType.];
                //KinectBlenderServer.PublishData(left.Position.X + "," + left.Position.Y + "," + left.Position.Z + "," +
                //    right.Position.X + "," + right.Position.Y + "," + right.Position.Z);

                // Launch the event!
                //Skeleton2DdataCoordReady(null, new Skeleton2DdataCoordEventArgs(p));
            }
        }

        public static Skeleton NormalizeDataUseShoulder(Skeleton data)
        {
            if (data.TrackingState == Microsoft.Kinect.SkeletonTrackingState.Tracked)
            {
            //    // Extract the coordinates of the points.
                SkeletonPoint center = getCenterPoint(data.Joints[JointType.ShoulderLeft], data.Joints[JointType.ShoulderRight]);

                SkeletonPoint slcenter = data.Joints[JointType.ShoulderLeft].Position;

            //    float shoulderDist = (float) getDistance(data.Joints[JointType.ShoulderLeft], data.Joints[JointType.ShoulderRight]);

                float handLength = (float)(getDistance(data.Joints[JointType.HandLeft], data.Joints[JointType.WristLeft])
                                + getDistance(data.Joints[JointType.WristLeft], data.Joints[JointType.ElbowLeft])
                                + getDistance(data.Joints[JointType.ElbowLeft], data.Joints[JointType.ShoulderLeft]));

                
            //    //change origin
                foreach (Joint j in data.Joints)
                {
                    SkeletonPoint normalizedOriginPoint = new SkeletonPoint();

            //        normalizedOriginPoint.X = j.Position.X - slcenter.X;
            //        normalizedOriginPoint.Y = j.Position.Y - slcenter.Y;

                    normalizedOriginPoint.X = j.Position.X;
                    normalizedOriginPoint.Y = j.Position.Y;
            //        // Use only hand position in font of the user
                    normalizedOriginPoint.Z = (j.Position.Z - slcenter.Z) + handLength / 2;// +0.1f;

                    Joint joint = data.Joints[j.JointType];
                    joint.Position = normalizedOriginPoint;
                    data.Joints[j.JointType] = joint;
                }

            //    // Normalization of the coordinates
                //foreach (Joint j in data.Joints)
                //{
                //    SkeletonPoint normalizedDistancePoint = new SkeletonPoint();

                //    //normalizedDistancePoint.X = j.Position.X / (handLength);
                //    //normalizedDistancePoint.Y = j.Position.Y / handLength;

                //    normalizedDistancePoint.X = j.Position.X;
                //    normalizedDistancePoint.Y = j.Position.Y;
                //    normalizedDistancePoint.Z = j.Position.Z / (handLength / -2);

                //    Joint joint = data.Joints[j.JointType];
                //    joint.Position = normalizedDistancePoint;
                //    data.Joints[j.JointType] = joint;
                //}


            //    var left = data.Joints[JointType.HandLeft];
            //    var right = data.Joints[JointType.HandRight];
            //    //var middle = data.Joints[JointType.];
            //    //KinectBlenderServer.PublishData(left.Position.X + "," + left.Position.Y + "," + left.Position.Z + "," +
            //    //    right.Position.X + "," + right.Position.Y + "," + right.Position.Z);

            //    KinectBlenderServer.PublishData(left.Position.X + "," + left.Position.Y + "," + left.Position.Z + "," +
            //        right.Position.X + "," + right.Position.Y + "," + right.Position.Z);
               
            //    // Launch the event!
                SkeletonNormalizedDataReady(null, new SkeletonNormalizedDataEventArgs(data));
            }

            return data;
        }

        public static double getDistance(Joint j1, Joint j2)
        {
            return Math.Sqrt(Math.Pow((j1.Position.X - j2.Position.X), 2) +
                          Math.Pow((j1.Position.Y - j2.Position.Y), 2) +
                          Math.Pow((j1.Position.Z - j2.Position.Z), 2)
                          );
        }

        public static SkeletonPoint getCenterPoint(Joint j1, Joint j2)
        {
            SkeletonPoint center = new SkeletonPoint();
            center.X = (j1.Position.X + j2.Position.X) / 2;
            center.Y = (j1.Position.Y + j2.Position.Y) / 2;
            center.Z = (j1.Position.Z + j2.Position.Z) / 2;
            return center;
        }

        public static SkeletonPoint getCenterPoint(SkeletonPoint j1, SkeletonPoint j2)
        {
            SkeletonPoint center = new SkeletonPoint();
            center.X = (j1.X + j2.X) / 2;
            center.Y = (j1.Y + j2.Y) / 2;
            center.Z = (j1.Z + j2.Z) / 2;
            return center;
        }

        public static Skeleton changeOrigin(SkeletonPoint newCenter, Skeleton data)
        {
            foreach (Joint j in data.Joints)
            {
                SkeletonPoint normalizedOriginPoint = new SkeletonPoint();

                normalizedOriginPoint.X = j.Position.X - newCenter.X;
                normalizedOriginPoint.Y = j.Position.Y - newCenter.Y;
                normalizedOriginPoint.Z = j.Position.Z - newCenter.Z;

                Joint joint = data.Joints[j.JointType];
                joint.Position = normalizedOriginPoint;
                data.Joints[j.JointType] = joint;
            }
            return data;
        }

        public static Skeleton normalizeByDistance(double normalizingParameter, Skeleton data)
        {
            foreach (Joint j in data.Joints)
            {
                SkeletonPoint normalizedDistancePoint = new SkeletonPoint();

                normalizedDistancePoint.X = j.Position.X / ((float)normalizingParameter);
                normalizedDistancePoint.Y = j.Position.Y / ((float)normalizingParameter);
                normalizedDistancePoint.Z = j.Position.Z / ((float)normalizingParameter);

                Joint joint = data.Joints[j.JointType];
                joint.Position = normalizedDistancePoint;
                data.Joints[j.JointType] = joint;
            }
            return data;
        }

        public static double getAngle(SkeletonPoint left, SkeletonPoint right)
        {
            Point3D center = new Point3D(getCenterPoint(left, right));

            Vector3D origin = new Vector3D(center, new Point3D(center.x,0,0));
            Vector3D hand = new Vector3D(new Point3D(left), new Point3D(right));

            double angle = 0f;


            return angle;
        }


    }
}
