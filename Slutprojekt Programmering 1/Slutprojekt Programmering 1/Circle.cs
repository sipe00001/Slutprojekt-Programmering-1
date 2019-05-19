using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Slutprojekt_Programmering_1
{
    class Circle
    {
        public Vector2 Center;
        public float Radius;

        public Circle(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }
        //Real-Time Collision Detection - Christer Ericson
        //Sida 131 point aabb
        /// <summary>
        /// Hittar närmaste punkten på en rektangel till en punkt
        /// </summary>
        static public Vector2 ClosestPoint(Vector2 a, Rectangle b)
        {
            Vector2 result = new Vector2(a.X, a.Y);
            var xmin = b.Left;
            var xmax = b.Right;
            var ymin = b.Top;
            var ymax = b.Bottom;
            result.X = (float)Util.Clamp(xmin, result.X, xmax);
            result.Y = (float)Util.Clamp(ymin, result.Y, ymax);

            return result;
        }
        /// <summary>
        /// Säger ifall en rektangel och en punkt kolliderar.
        /// </summary>
        public bool Intersect(Rectangle b)
        {
            Vector2 closePoint = ClosestPoint(this.Center, b);
            var hypotenuse = Math.Sqrt(Math.Pow((closePoint.X - this.Center.X), 2) + Math.Pow((closePoint.Y - this.Center.Y), 2));
            var difference = hypotenuse - this.Radius;
            if (difference < 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}