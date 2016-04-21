using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace HCI2012
{
    class Point3D
    {

        public double x;
        public double y;
        public double z;


        public Point3D(double a,double b, double c)
        {
            x =a ;
            y=b;
            z=c;
        }


        public Point3D(SkeletonPoint skePoint)
        {
            x = skePoint.X ;
            y = skePoint.Y;
            z = skePoint.Z;
        }

    }
}
