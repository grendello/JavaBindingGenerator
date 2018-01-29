//
// OutputPathProvider.cs
//
// Author:
//       Marek Habersack <grendel@twistedcode.net>
//
// Copyright (c) 2018 Microsoft, Inc (http://microsoft.com/)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.IO;

namespace Java.Interop.Bindings.Compiler
{
	/// <summary>
	/// Helper class to make it easier to return paths to both directories and files and
	/// let the caller decide what action to take.
	/// </summary>
	public class FilesystemPath
	{
		/// <summary>
		/// Whether or not this istance points to a directory
		/// </summary>
		public bool IsDirectory { get; set; }

		/// <summary>
		/// Full path to the filesystem entry
		/// </summary>
		public string FullPath { get; set; }

		/// <summary>
		/// Create instance pointing to fullPath and stating whether it is a directory or a file
		/// </summary>
		public FilesystemPath (string fullPath, bool isDirectory)
		{
			FullPath = fullPath;
			IsDirectory = isDirectory;
		}

		public override string ToString ()
		{
			return $"{GetType ()} ({FullPath}; {IsDirectory})";
		}
	}
}
