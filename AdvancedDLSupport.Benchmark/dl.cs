//
//  dl.cs
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
using System.Runtime.InteropServices;
using AdvancedDLSupport.Loaders;

// ReSharper disable InconsistentNaming
#pragma warning disable SA1300 // Elements should begin with an uppercase letter
#pragma warning disable SA1600, CS1591 // Elements should be documented

namespace AdvancedDLSupport.Benchmark
{
    internal static class dl
    {
        private const string LibraryNameUnix = "dl";

        [DllImport(LibraryNameUnix)]
        public static extern IntPtr dlopen(string fileName, SymbolFlag flags);

        [DllImport(LibraryNameUnix)]
        public static extern IntPtr dlsym(IntPtr handle, string name);

        [DllImport(LibraryNameUnix)]
        public static extern int dlclose(IntPtr handle);

        [DllImport(LibraryNameUnix)]
        public static extern IntPtr dlerror();
    }
}
