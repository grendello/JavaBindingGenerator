﻿//
// InputDocument.cs
//
// Author:
//       Marek Habersack <grendel@twistedcode.net>
//
// Copyright (c) 2017 Microsoft, Inc (http://microsoft.com/)
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

namespace Java.Interop.Bindings
{
	public sealed class InputDocument : IDisposable
	{
		bool disposedValue;
		string path;
		Stream stream;

		public string Path => path;
		public Stream Stream => stream;

		public InputDocument (string path)
		{
			if (String.IsNullOrEmpty (path))
				throw new ArgumentException ("must not be null or empty", nameof (path));
			Init (path, null);
		}

		public InputDocument (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));
			Init (null, stream);
		}

		public InputDocument (string path, Stream stream)
		{
			if (String.IsNullOrEmpty (path))
				throw new ArgumentException ("must not be null or empty", nameof (path));
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));
			Init (path, stream);
		}

		void Init (string inputPath, Stream inputStream)
		{
			path = inputPath;
			stream = inputStream;
			if (inputPath != null && inputStream != null)
				return;

			if (inputPath == null) {
				path = "*memory stream*";
				return;
			}

			stream = File.OpenRead (inputPath);
		}

		void Dispose (bool disposing)
		{
			if (!disposedValue) {
				if (disposing) {
					Stream?.Dispose ();
				}

				disposedValue = true;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
		}
	}
}
