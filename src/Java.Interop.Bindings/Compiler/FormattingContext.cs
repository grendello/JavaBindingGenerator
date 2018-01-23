//
// FormattingContext.cs
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
using System.Text;

namespace Java.Interop.Bindings.Compiler
{
	public class FormattingContext
	{
		Stack <uint> indents;
		string currentIndent;

		public FormattingStyle Style { get; }
		public uint CurrentIndentLevel => indents.Peek ();
		public string CurrentIndent => currentIndent;

		public FormattingContext (FormattingStyle style)
		{
			Style = style ?? throw new ArgumentNullException (nameof (style));
			indents = new Stack <uint> ();
			indents.Push (0);
			currentIndent = String.Empty;
		}

		public void IncreaseIndent ()
		{
			uint newIndentLevel = CurrentIndentLevel + 1;
			indents.Push (newIndentLevel);
			UpdateIndentString (newIndentLevel);
		}

		public void DecreaseIndent ()
		{
			if (indents == null || indents.Count == 1)
				return;
			indents.Pop ();
			UpdateIndentString (CurrentIndentLevel);
		}

		void UpdateIndentString (uint newIndentLevel)
		{
			if (String.IsNullOrEmpty (Style.IndentString)) {
				currentIndent = String.Empty;
				return;
			}

			var sb = new StringBuilder ();
			for (uint i = 0; i < newIndentLevel; i++)
				sb.Append (Style.IndentString);
			currentIndent = sb.ToString ();
		}
	}
}
