using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConwaysLife.Utility
{
    public struct Location
    {
        long _x;
        public long X { get { return _x; } }

        long _y;
        public long Y { get { return _y; } }

        bool _isValid;
        public bool IsValid { get { return _isValid; } }

        public Location(long x, long y, Dimensions dim)
            : this()
        {
            _x = x;
            _y = y;
            _isValid = _x >= 0 && _x < dim.Width &&
                _y >= 0 && _y < dim.Height;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", X, Y);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Location))
                return false;
            Location loc = (Location)obj;
            return this.X == loc.X && this.Y == loc.Y;
        }

        public static bool operator !=(Location lhs, Location rhs)
        {
            return lhs.X != rhs.X || lhs.Y != rhs.Y;
        }

        public static bool operator ==(Location lhs, Location rhs)
        {
            return lhs.X == rhs.X && lhs.Y == rhs.Y;
        }

        public override int GetHashCode()
        {
            return _x.GetHashCode() ^ _y.GetHashCode();
        }
    }
}
