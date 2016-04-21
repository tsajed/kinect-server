using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace HCI2012
{
    class UserSynchronizeInfo
    {
        public Skeleton basicSkeleton;

        public UserSynchronizeInfo(Skeleton skel)
        {
            basicSkeleton = skel; 
        }

        public double getLeftHandLength()
        {
            return getDistance(JointType.HandLeft, JointType.WristLeft)
                + getDistance(JointType.ElbowLeft, JointType.WristLeft)
                + getDistance(JointType.ElbowLeft, JointType.ShoulderLeft);
        }

        public double getRightHandLength()
        {
            return getDistance(JointType.HandRight, JointType.WristRight)
                + getDistance(JointType.ElbowRight, JointType.WristRight)
                + getDistance(JointType.ElbowRight, JointType.ShoulderRight);
        }

        /// <summary>
        /// Return the vector start from the center of the two feet point to user's head
        /// </summary>
        /// <returns></returns>
        public Vector3D getBodyVector()
        {
            Point3D pointing = new Point3D(basicSkeleton.Joints[JointType.Head].Position);
            Point3D origin = new Point3D(basicSkeleton.Joints[JointType.Head].Position.X, basicSkeleton.Joints[JointType.FootLeft].Position.Y, basicSkeleton.Joints[JointType.FootLeft].Position.Z);
            return new Vector3D(origin, pointing);
        }

        /// <summary>
        /// Return the distance between any two joints of the skeleton of this user
        /// </summary>
        /// <param name="j1">The first joint</param>
        /// <param name="j2">The second joint</param>
        /// <returns></returns>
        public  double getDistance(JointType j1, JointType j2)
        {
            return Math.Sqrt(Math.Pow((basicSkeleton.Joints[j1].Position.X - basicSkeleton.Joints[j2].Position.X), 2) +
                          Math.Pow((basicSkeleton.Joints[j1].Position.Y - basicSkeleton.Joints[j2].Position.Y), 2) +
                          Math.Pow((basicSkeleton.Joints[j1].Position.Z - basicSkeleton.Joints[j2].Position.Z), 2)
                          );
        }

    }
}
