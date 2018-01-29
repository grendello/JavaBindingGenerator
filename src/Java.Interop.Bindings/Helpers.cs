//
// Helpers.cs
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
using System.Collections.Generic;

namespace Java.Interop.Bindings
{
	public static class Helpers
	{
		public static void AddToList <T> (T v, ref List<T> list)
		{
			if (list == null)
				list = new List<T> ();
			list.Add (v);
		}

		public static void ForEachNotNull <T> (IList <T> list, Action <T> code) where T: class
		{
			if (list == null || list.Count == 0)
				return;

			if (code == null)
				throw new ArgumentNullException (nameof (code));

			foreach (T t in list) {
				if (t == null)
					continue;

				code (t);
			}
		}

		public static string GetBaseTypeName (string fullTypeName)
		{
			if (String.IsNullOrEmpty (fullTypeName))
				return String.Empty;

			int idx = fullTypeName.LastIndexOf ('.');
			if (idx <= 0)
				return fullTypeName;

			return fullTypeName.Substring (idx + 1);
		}

		public static string GetTypeNamespace (string fullTypeName)
		{
			if (String.IsNullOrEmpty (fullTypeName))
				return String.Empty;

			int lastDot = fullTypeName.LastIndexOf ('.');
			if (lastDot <= 0)
				return String.Empty;

			return fullTypeName.Substring (0, lastDot);
		}

		public static T EnsureNotNull<T> (string name, T value) where T: class
		{
			if (value == null)
				throw new InvalidOperationException ($"{name} must not be null");
			return value;
		}
	}
}
