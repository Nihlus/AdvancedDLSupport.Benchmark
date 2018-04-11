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
using System.Diagnostics;
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
        private static readonly Matrix2 Source = new Matrix2 { Row0 = { X = 4, Y = 7 }, Row1 = { X = 2, Y = 6 } };
        private static readonly Matrix2 Result = new Matrix2 { Row0 = { X = 0.6f, Y = -0.7f }, Row1 = { X = -0.2f, Y = 0.4f } };

        private static ITest _adlLibrary;
        private static ITest _adlLibraryWithoutDisposeChecks;
        private static ITest _calliImplementation;

        /// <summary>
        /// The main entry point.
        /// </summary>
        internal static void Main()
        {
            _adlLibrary = NativeLibraryBuilder.Default.ActivateInterface<ITest>("test");
            _adlLibraryWithoutDisposeChecks = new NativeLibraryBuilder().ActivateInterface<ITest>("test");

            _calliImplementation = GetCalliImplementation();

            var inverted = _adlLibrary.InvertMatrixByValue(Source);
            Debug.Assert(inverted == Result, "inverted == Result");

            ulong iterationCount = 10000;

            // DllImport, by ref
            Console.WriteLine($"Running {iterationCount} iterations of Matrix2 inversions using DllImport, passing the matrix by reference.");
            var withDllImportByRef = RunIterations(RunDllImportByRefIteration, iterationCount);

            Console.WriteLine($"Average result: {withDllImportByRef}ms per iteration.\n");

            // DllImport, by value
            Console.WriteLine($"Running {iterationCount} iterations of Matrix2 inversions using DllImport, passing the matrix by value.");
            var withDllImportByValue = RunIterations(RunDllImportByValueIteration, iterationCount);

            Console.WriteLine($"Average result: {withDllImportByValue}ms per iteration.\n");

            // Delegates, by ref
            Console.WriteLine($"Running {iterationCount} iterations of Matrix2 inversions using delegates, passing the matrix by reference.");
            var withADLByRef = RunIterations(RunADLByRefIteration, iterationCount);

            Console.WriteLine($"Average result: {withADLByRef}ms per iteration.\n");

            // Delegates, by value
            Console.WriteLine($"Running {iterationCount} iterations of Matrix2 inversions using delegates, passing the matrix by value.");
            var withADLByValue = RunIterations(RunADLByValueIteration, iterationCount);

            Console.WriteLine($"Average result: {withADLByValue}ms per iteration.\n");

            // Delegates (safeties off), by ref
            Console.WriteLine($"Running {iterationCount} iterations of Matrix2 inversions using delegates (safeties off), passing the matrix by reference.");
            var withUnsafeADLByRef = RunIterations(RunUnsafeADLByRefIteration, iterationCount);

            Console.WriteLine($"Average result: {withUnsafeADLByRef}ms per iteration.\n");

            // Delegates (safeties off), by value
            Console.WriteLine($"Running {iterationCount} iterations of Matrix2 inversions using delegates (safeties off), passing the matrix by value.");
            var withUnsafeADLByValue = RunIterations(RunUnsafeADLByValueIteration, iterationCount);

            Console.WriteLine($"Average result: {withUnsafeADLByValue}ms per iteration.\n");

            // calli, by ref
            Console.WriteLine($"Running {iterationCount} iterations of Matrix2 inversions using calli, passing the matrix by reference.");
            var withCalliByRef = RunIterations(RunCalliByRefIteration, iterationCount);

            Console.WriteLine($"Average result: {withCalliByRef}ms per iteration.\n");

            // calli, by value
            Console.WriteLine($"Running {iterationCount} iterations of Matrix2 inversions using calli, passing the matrix by value.");
            var withCalliByValue = RunIterations(RunCalliByValueIteration, iterationCount);

            Console.WriteLine($"Average result: {withCalliByValue}ms per iteration.\n");
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
            _calliImplementation.InvertMatrixByPtr(ref matrixCopy);
        }

        private static void RunCalliByValueIteration()
        {
            var matrixCopy = Source;
            _calliImplementation.InvertMatrixByValue(matrixCopy);
        }

        private static decimal RunIterations(Action action, ulong count)
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

            return new decimal(perIteration);
        }

        [NotNull]
        private static ITest GetCalliImplementation()
        {
            var libraryPtr = dl.dlopen("./libtest.so", SymbolFlag.RTLD_DEFAULT);
            var byRefPtr = dl.dlsym(libraryPtr, nameof(ITest.InvertMatrixByPtr));
            var byValPtr = dl.dlsym(libraryPtr, nameof(ITest.InvertMatrixByValue));

            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("DynamicAssembly"), AssemblyBuilderAccess.Run);
            var dynamicModule = dynamicAssembly.DefineDynamicModule("DynamicModule");

            var dynamicType = dynamicModule.DefineType
            (
                "CalliImplementation",
                TypeAttributes.Class | TypeAttributes.Public,
                typeof(object),
                new[] { typeof(ITest) }
            );

            var byRefMethod = dynamicType.DefineMethod
            (
                nameof(ITest.InvertMatrixByPtr),
                Public | Virtual | Final | NewSlot,
                typeof(void),
                new[] { typeof(Matrix2).MakeByRefType() }
            );

            var byRefIL = byRefMethod.GetILGenerator();

            byRefIL.EmitLoadArgument(1);
            byRefIL.EmitConstantLong(byRefPtr.ToInt64());
            byRefIL.EmitCalli(OpCodes.Calli, CallingConventions.Standard, typeof(void), new[] { typeof(Matrix2).MakeByRefType() }, null);
            byRefIL.EmitReturn();

            var byValMethod = dynamicType.DefineMethod
            (
                nameof(ITest.InvertMatrixByValue),
                Public | Virtual | Final | NewSlot,
                typeof(Matrix2),
                new[] { typeof(Matrix2) }
            );

            var byValIL = byValMethod.GetILGenerator();

            byValIL.EmitLoadArgument(1);
            byValIL.EmitConstantLong(byValPtr.ToInt64());
            byValIL.EmitCalli(OpCodes.Calli, CallingConventions.Standard, typeof(Matrix2), new[] { typeof(Matrix2) }, null);
            byValIL.EmitReturn();

            var type = dynamicType.CreateTypeInfo();
            return (ITest)Activator.CreateInstance(type);
        }
    }
}
