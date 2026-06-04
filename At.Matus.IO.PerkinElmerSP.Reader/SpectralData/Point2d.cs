// ============================================================================
// File: Point2d.cs
// Project: At.Matus.IO.PerkinElmerSP.Reader
// Description: Represents a two-dimensional point with X and Y coordinates
//              used for spectral data representation.
// 
// Copyright (c) 2026 Michael Matus
// All rights reserved.
// ============================================================================

namespace At.Matus.IO.PerkinElmerSP.Reader
{
    public struct Point2d : IComparable<Point2d>
    {
        public double X;
        public double Y;

        public int CompareTo(Point2d other) => X.CompareTo(other.X);
    }
}
