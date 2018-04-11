//
//  Matrix2.cs
//
//  Copyright (c) 2018 Firwood Software
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System.Diagnostics.CodeAnalysis;

namespace AdvancedDLSupport.Benchmark
{
    /// <summary>
    /// A 2x2 matrix of 32-bit floating-point values.
    /// </summary>
    public struct Matrix2
    {
        /// <summary>
        /// The first row of the matrix.
        /// </summary>
        public Vector2 Row0;

        /// <summary>
        /// The second row of the matrix.
        /// </summary>
        public Vector2 Row1;

        /// <summary>
        /// Determines componentwise equality for two matrices.
        /// </summary>
        /// <param name="a">The first matrix.</param>
        /// <param name="b">The second matrix.</param>
        /// <returns>true if the matrices are equal, otherwise, false.</returns>
        public static bool operator ==(Matrix2 a, Matrix2 b)
        {
            return a.Row0 == b.Row0 && a.Row1 == b.Row1;
        }

        /// <summary>
        /// Determines componentwise inequality for two matrices.
        /// </summary>
        /// <param name="a">The first matrix.</param>
        /// <param name="b">The second matrix.</param>
        /// <returns>true if the matrices are not equal, otherwise, false.</returns>
        public static bool operator !=(Matrix2 a, Matrix2 b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Determines componentwise equality for the current and another matrix.
        /// </summary>
        /// <param name="other">The other matrix.</param>
        /// <returns>true if the matrices are equal, otherwise, false.</returns>
        public bool Equals(Matrix2 other)
        {
            return this.Row0.Equals(other.Row0) && this.Row1.Equals(other.Row1);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is Matrix2 matrix2 && Equals(matrix2);
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = "Struct is used for native interop.")]
        public override int GetHashCode()
        {
            unchecked
            {
                return (this.Row0.GetHashCode() * 397) ^ this.Row1.GetHashCode();
            }
        }
    }
}
