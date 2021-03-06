﻿//
// Extensions.XElement.cs
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
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Java.Interop.Bindings
{
	partial class Extensions
	{
		public static string XGetAttribute (this XElement element, string name)
		{
			if (element == null)
				return null;

			XAttribute attr = element.Attribute (name);
			return attr?.Value?.Trim ();
		}

		public static string XGetLastAttribute (this XElement element, string name)
		{
			if (element == null)
				return null;

			XAttribute attr = element.Attributes (name).LastOrDefault ();
			return attr?.Value?.Trim ();
		}

		public static bool Is (this XElement element, string name)
		{
			if (element == null || String.IsNullOrEmpty (name))
				return false;

			return String.Compare (name, element.Name.LocalName, StringComparison.Ordinal) == 0;
		}

		public static string ToShortString (this XElement element, bool includeLocation = false)
		{
			if (element == null)
				return String.Empty;

			var sb = new StringBuilder ();
			sb.Append ($"<{element.Name}");
			if (element.HasAttributes) {
				foreach (XAttribute attr in element.Attributes ()) {
					sb.Append ($" {attr.Name}=\"{attr.Value}\"");
				}
			}
			sb.Append (" />");

			if (includeLocation) {
				if (!String.IsNullOrEmpty (element.Document.BaseUri))
					sb.Append ($" {element.Document.BaseUri}");
				string lineInfo = element.GetLineInfo ();
				if (!String.IsNullOrEmpty (lineInfo))
					sb.Append ($":[{lineInfo}]");
			}

			return sb.ToString ();
		}
	}
}
