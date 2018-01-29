//
// XamarinNameTranslationProvider.cs
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

namespace Java.Interop.Bindings.Compiler
{
	public class XamarinNameTranslationProvider : PlainNameTranslationProvider
	{
		static readonly Guid guid = new Guid ("7fe9762d-ed0e-431b-b2ef-ab3a2b4306be");

		// This is a hack used by the old generator - it uppercases a symbol segment if it consists only of two
		// characters
		protected bool UpperCaseTwoLetterSegments { get; set; } = true;

		public XamarinNameTranslationProvider () : base ("Xamarin.Android", guid)
		{}

		protected override string TranslateSegment (string segment)
		{
			if (String.IsNullOrEmpty (segment))
				throw new ArgumentException ("must not be null or empty", nameof (segment));

			if (UpperCaseTwoLetterSegments && segment.Length == 2)
				return segment.ToUpper ();

			return base.TranslateSegment (segment);
		}
	}
}
