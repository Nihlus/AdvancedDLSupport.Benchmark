//
//  Program.cs
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AdvancedDLSupport.Loaders;
using JetBrains.Annotations;
using StrictEmit;
using static System.Reflection.MethodAttributes;

namespace AdvancedDLSupport.Benchmark
{
    /// <summary>
    /// The main program class.
    /// </summary>
    internal static class Program
    {
        internal const string LibraryName = "TestLibrary";

        private static readonly Matrix2 Source = new Matrix2 { Row0 = { X = 4, Y = 7 }, Row1 = { X = 2, Y = 6 } };
        private static readonly Matrix2 Result = new Matrix2 { Row0 = { X = 0.6f, Y = -0.7f }, Row1 = { X = -0.2f, Y = 0.4f } };

        private static ITest _adlLibrary;
        private static ITest _adlLibraryWithoutDisposeChecks;
        private static ITest _adlLibraryWithCalli;

        /// <summary>
        /// The main entry point.
        /// </summary>
        internal static void Main()
        {
            _adlLibrary = NativeLibraryBuilder.Default.ActivateInterface<ITest>(LibraryName);
            _adlLibraryWithoutDisposeChecks = new NativeLibraryBuilder().ActivateInterface<ITest>(LibraryName);
            _adlLibraryWithCalli = new NativeLibraryBuilder(ImplementationOptions.UseIndirectCalls).ActivateInterface<ITest>(LibraryName);

            var inverted = _adlLibrary.InvertMatrixByValue(Source);
            Debug.Assert(inverted == Result, "inverted == Result");

            ulong iterationCount = 10000;

            // Managed, by ref
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"Running {iterationCount} iterations of Matrix2 inversions, passing the matrix by reference.");
            var performanceResultsByRef = new[]
            {
                RunIterations("Managed", RunManagedByRefIteration, iterationCount),
                RunIterations("DllImport", RunDllImportByRefIteration, iterationCount),
                RunIterations("Delegates", RunADLByRefIteration, iterationCount),
                RunIterations("Delegates (safeties off)", RunUnsafeADLByRefIteration, iterationCount),
                RunIterations("calli", RunCalliByRefIteration, iterationCount)
            };

            PresentResults(performanceResultsByRef);

            Console.WriteLine();

            // Managed, by value
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"Running {iterationCount} iterations of Matrix2 inversions, passing the matrix by value.");

            var performanceResultsByValue = new[]
            {
                RunIterations("Managed", RunManagedByValueIteration, iterationCount),
                RunIterations("DllImport", RunDllImportByValueIteration, iterationCount),
                RunIterations("Delegates", RunADLByValueIteration, iterationCount),
                RunIterations("Delegates (safeties off)", RunUnsafeADLByValueIteration, iterationCount),
                RunIterations("calli", RunCalliByValueIteration, iterationCount)
            };

            PresentResults(performanceResultsByValue);
        }

        private static void PresentResults([NotNull] IReadOnlyCollection<(string Name, decimal Time)> results)
        {
            var orderedColours = new[] { ConsoleColor.Green, ConsoleColor.Yellow, ConsoleColor.Red };

            var longestNameLength = results.OrderByDescending(r => r.Name.Length).First().Name.Length;
            var longestTimeLength = results.OrderByDescending(r => r.Time.ToString(CultureInfo.CurrentCulture).Length).First().Time.ToString(CultureInfo.CurrentCulture).Length;

            var orderedResults = results.OrderBy(r => r.Time).ToList();

            var worstResult = orderedResults.Last();
            var formattedResults = orderedResults.Select
            (
                r =>
                    $"{r.Name.PadRight(longestNameLength, ' ')} : " +
                    $"{r.Time.ToString(CultureInfo.CurrentCulture).PadRight(longestTimeLength, ' ')}ms " +
                    $"({(worstResult.Time / r.Time) - 1:P0} improvement over worst time)"
            )
            .ToList();

            for (var i = 0; i < formattedResults.Count; ++i)
            {
                Console.ForegroundColor = orderedColours[i < 3 ? i : 2];
                Console.WriteLine(formattedResults[i]);
            }
        }

        private static void RunDllImportByRefIteration()
        {
            var matrixCopy = Source;
            DllImportTest.InvertMatrixByPtr(ref matrixCopy);
        }

        private static void RunDllImportByValueIteration()
        {
            var matrixCopy = Source;
            DllImportTest.InvertMatrixByValue(matrixCopy);
        }

        private static void RunADLByRefIteration()
        {
            var matrixCopy = Source;
            _adlLibrary.InvertMatrixByPtr(ref matrixCopy);
        }

        private static void RunADLByValueIteration()
        {
            var matrixCopy = Source;
            _adlLibrary.InvertMatrixByValue(matrixCopy);
        }

        private static void RunUnsafeADLByRefIteration()
        {
            var matrixCopy = Source;
            _adlLibraryWithoutDisposeChecks.InvertMatrixByPtr(ref matrixCopy);
        }

        private static void RunUnsafeADLByValueIteration()
        {
            var matrixCopy = Source;
            _adlLibraryWithoutDisposeChecks.InvertMatrixByValue(matrixCopy);
        }

        private static void RunCalliByRefIteration()
        {
            var matrixCopy = Source;
            _adlLibraryWithCalli.InvertMatrixByPtr(ref matrixCopy);
        }

        private static void RunCalliByValueIteration()
        {
            var matrixCopy = Source;
            _adlLibraryWithCalli.InvertMatrixByValue(matrixCopy);
        }

        private static void RunManagedByRefIteration()
        {
            var matrixCopy = Source;
            Matrix2.Invert(ref matrixCopy);
        }

        private static void RunManagedByValueIteration()
        {
            var matrixCopy = Source;
            Matrix2.Invert(matrixCopy);
        }

        private static (string Name, decimal Time) RunIterations(string name, Action action, ulong count)
        {
            var sw = new Stopwatch();

            // Warmup
            for (var i = 0; i < 100; ++i)
            {
                action();
            }

            // Time
            sw.Start();
            for (ulong i = 0; i < count; ++i)
            {
                action();
            }

            sw.Stop();

            var elapsed = sw.Elapsed.TotalMilliseconds;
            var perIteration = elapsed / count;

            return (name, new decimal(perIteration));
        }
    }
}
