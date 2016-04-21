using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HCI2012
{
    class Vector3D
    {
        public Point3D origin;
        public Point3D point;

        public Vector3D(Point3D org, Point3D p)
        {
            origin = org;
            point = p;
        }
    }
}
