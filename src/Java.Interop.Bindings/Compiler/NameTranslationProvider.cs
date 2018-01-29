//
// NameTranslationProvider.cs
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

namespace Java.Interop.Bindings.Compiler
{
	public abstract class NameTranslationProvider
	{
		public string Name { get; }
		public Guid ID { get; }

		// If a Java name has a dot in it, preserve the dots when converting to managed name
		public bool PreserveDotsInJavaNameTranslation { get; set; }

		protected NameTranslationProvider (string name, Guid id)
		{
			if (String.IsNullOrEmpty (name))
				throw new ArgumentException ("must not be null or empty", nameof (name));
			if (id == Guid.Empty)
				throw new ArgumentException ("must be a non-empty UUID", nameof (id));

			Name = name;
			ID = id;
		}

		public virtual string EnsureValidIdentifier (string identifier)
		{
			if (String.IsNullOrEmpty (identifier))
				throw new InvalidOperationException ($".NET identifier must not be null or empty");

			// TODO: implement full validation (check for VALID characters)
			return identifier;
		}

		protected string UpperFirst (string s)
		{
			char first = Char.ToUpper (s [0]);
			if (s.Length > 1)
				return $"{first}{s.Substring (1)}";
			return first.ToString ();
		}

		protected abstract string TranslateSegment (string segment);
		public abstract string Translate (string javaName);
	}
}
