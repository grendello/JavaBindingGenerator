//
// PlainNameTranslationProvider.cs
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

namespace Java.Interop.Bindings.Compiler
{
	public abstract class PlainNameTranslationProvider : NameTranslationProvider
	{
		static readonly Guid guid = new Guid ("450b31d2-f65b-4f34-80cc-59b55388ab98");

		public PlainNameTranslationProvider () : base ("Plain", guid)
		{}

		protected PlainNameTranslationProvider (string name, Guid guid) : base (name, guid)
		{}

		public override string Translate (string javaName)
		{
			if (String.IsNullOrEmpty (javaName))
				throw new ArgumentException ("must not be null or empty", nameof (javaName));

			if (javaName.Length == 1)
				return UpperFirst (javaName);

			int dot = javaName.IndexOf ('.');
			if (dot >= 0) {
				var segments = new List <string> ();
				foreach (string s in javaName.Split ('.')) {
					string segment = TranslateSegment (s);
					if (String.IsNullOrEmpty (segment))
						continue;
					segments.Add (segment);
				}
				javaName = String.Join (PreserveDotsInJavaNameTranslation ? "." : String.Empty, segments);
			} else if (javaName.Length == 2)
				javaName = javaName.ToUpper ();

			return EnsureValidIdentifier (UpperFirst (javaName));
		}

		protected override string TranslateSegment (string segment)
		{
			return UpperFirst (segment);
		}
	}
}
