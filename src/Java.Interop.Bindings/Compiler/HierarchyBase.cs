//
// HierarchyBase.cs
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

using Java.Interop.Bindings.Syntax;

namespace Java.Interop.Bindings.Compiler
{
	public abstract class HierarchyBase
	{
		// Intended for types that aren't in API.xml and yet are needed for other types to work, for instance
		// the IJavaObject interface (which is defined in Xamarin.Android's runtime)
		public bool IgnoreForCodeGeneration { get; set; }

		// Whether the instance was created based on API description
		public bool IsBoundAPI { get; protected set; }

		// If a Java name has a dot in it, preserve the dots when converting to managed name
		protected bool PreserveDotsInJavaNameTranslation { get; set; }

		// This is a hack used by the old generator - it uppercases a symbol segment if it consists only of two
		// characters
		protected bool UpperCaseTwoLetterSegments { get; set; } = true;

		public HierarchyBase ()
		{
		}

		protected string JavaNameToManagedName (string javaName)
		{
			if (String.IsNullOrEmpty (javaName))
				throw new ArgumentException ("must not be null or empty", nameof (javaName));

			if (javaName.Length == 1)
				return UpperFirst (javaName);

			int dot = javaName.IndexOf ('.');
			if (dot >= 0) {
				var segments = new List <string> ();
				foreach (string s in javaName.Split ('.')) {
					if (UpperCaseTwoLetterSegments && s.Length == 2)
						segments.Add (s.ToUpper ());
					else
						segments.Add (UpperFirst (s));
				}
				javaName = String.Join (PreserveDotsInJavaNameTranslation ? "." : String.Empty, segments);
			} else if (javaName.Length == 2)
				javaName = javaName.ToUpper ();

			return EnsureValidIdentifier (UpperFirst (javaName));

			string UpperFirst (string s)
			{
				char first = Char.ToUpper (s [0]);
				if (s.Length > 1)
					return $"{first}{s.Substring (1)}";
				return first.ToString ();
			}
		}

		protected virtual string EnsureValidIdentifier (string identifier)
		{
			if (String.IsNullOrEmpty (identifier))
				throw new InvalidOperationException ($".NET identifier must not be null or empty");

			// TODO: implement full validation (check for VALID characters)
			return identifier;
		}

		protected TApiElement EnsureApiElementType <TApiElement> (ApiElement apiElement) where TApiElement: ApiElement
		{
			if (apiElement == null)
				throw new ArgumentNullException (nameof (apiElement));

			TApiElement ret = apiElement as TApiElement;
			if (ret == null)
				throw new InvalidOperationException ($"Expected API element of type '{typeof (TApiElement).FullName}' but got '{apiElement.GetType ().FullName}' instead");

			return ret;
		}
	}
}
