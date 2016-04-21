////-----------------------------------------------------------------------
//// <copyright file="Skeleton2DdataCoordEventArgs.cs" company="Rhemyst and Rymix">
////     Open Source. Do with this as you will. Include this statement or 
////     don't - whatever you like.
////
////     No warranty or support given. No guarantees this will work or meet
////     your needs. Some elements of this project have been tailored to
////     the authors' needs and therefore don't necessarily follow best
////     practice. Subsequent releases of this project will (probably) not
////     be compatible with different versions, so whatever you do, don't
////     overwrite your implementation with any new releases of this
////     project!
////
////     Enjoy working with Kinect!
//// </copyright>
////-----------------------------------------------------------------------
//using Microsoft.Kinect;

//namespace HCI2012
//{
//    using System.Windows;

//    /// <summary>
//    /// Takes Kinect SDK Skeletal Frame coordinates and converts them intoo a format useful to th DTW
//    /// </summary>
//    internal class SkeletonNormalizedDataEventArgs
//    {
//        /// <summary>
//        /// Positions of the elbows, the wrists and the hands (placed from left to right)
//        /// </summary>
//        private readonly Skeleton _skeleton;

//        /// <summary>
//        /// Initializes a new instance of the Skeleton2DdataCoordEventArgs class
//        /// </summary>
//        /// <param name="points">The points we need to handle in this class</param>
//        public SkeletonNormalizedDataEventArgs(Skeleton ske)
//        {
//            _skeleton = ske;
//        }

//        /// <summary>
//        /// Gets the point at a certain index
//        /// </summary>
//        /// <param name="index">The index we wish to retrieve</param>
//        /// <returns>The point at the sent index</returns>
//        public Skeleton GetData()
//        {
//            return _skeleton;
//        }
//    }

      
//}