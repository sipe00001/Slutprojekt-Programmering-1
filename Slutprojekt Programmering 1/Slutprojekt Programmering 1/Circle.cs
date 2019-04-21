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
        public Vector2 ClosestPoint(Vector2 a, Rectangle b)
        {
            Vector2 result = new Vector2(a.X, a.Y);
            var xmin = b.Left;
            var xmax = b.Right;
            var ymin = b.Top;
            var ymax = b.Bottom;

            result.X = Math.Max(xmin, result.X);
            result.X = Math.Min(xmax, result.X);
            result.Y = Math.Max(ymin, result.Y);
            result.Y = Math.Min(ymax, result.Y);

            return result;
        }
        public bool Intersect(Rectangle b)
        {
            Vector2 closePoint = ClosestPoint(this.Center, b);
            var hypotenuse = Math.Sqrt(Math.Pow((closePoint.X - this.Center.X), 2) + Math.Pow((closePoint.Y - this.Center.Y), 2));
            var difference = this.Radius - hypotenuse;
            if (difference < 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }
    }
}