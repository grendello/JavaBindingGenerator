//
// Extensions.String.cs
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
using System.Collections.Generic;

using Java.Interop.Bindings.Compiler;

namespace Java.Interop.Bindings
{
	partial class Extensions
	{
		public static string GetManagedName (this HierarchyElement element, bool full = false)
		{
			if (element == null)
				return null;

			string name = full ? element.FullManagedName :  element.ManagedName;
			if (!String.IsNullOrEmpty (name))
				return name;

			name = MakeManagedName (full ? element.FullName : element.Name);
			if (!String.IsNullOrEmpty (name))
				return name;

			return null;
		}

		static string MakeManagedName (string name)
		{
			if (name == null)
				return null;

			if (name.Length == 0)
				return name;

			string[] parts = name.Split ('.');
			var mangled = new List <string> ();

			foreach (string p in parts) {
				if (String.IsNullOrEmpty (p))
					throw new InvalidOperationException ($"Unable to convert Java name '{name}' to a valid managed name: empty segments after splitting");

				mangled.Add ($"{Char.ToUpper (p [0])}{p.Substring (1)}");
			}

			return String.Join (".", mangled);
		}
	}
}
