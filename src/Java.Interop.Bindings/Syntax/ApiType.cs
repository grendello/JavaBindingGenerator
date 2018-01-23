//
// ApiType.cs
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
using System.Xml.Linq;

namespace Java.Interop.Bindings.Syntax
{
	public abstract class ApiType : ApiElement
	{
		public bool Abstract { get; set; }
		public bool Static { get; set; }
		public ApiVisibility Visibility { get; set; }
		public bool Final { get; set; }

		protected ApiType (string documentPath) : base (documentPath)
		{ }

		protected override Dictionary<string, Action<string, XAttribute>> GetKnownAttributes()
		{
			Dictionary<string, Action<string, XAttribute>> ret = EnsureKnownAttributes (base.GetKnownAttributes ());

			ret ["abstract"] = (string value, XAttribute attr) => Abstract = value.AsBool (attr);
			ret ["final"] = (string value, XAttribute attr) => Final = value.AsBool (attr);
			ret ["static"] = (string value, XAttribute attr) => Static = value.AsBool (attr);
			ret ["visibility"] = (string value, XAttribute attr) => Visibility = ParseVisibility (value);

			return ret;
		}

		protected override void ParseChild (XElement parent, XElement child)
		{
			if (child.Is ("field")) {
				ParseAndAddChild (child, new ApiField (DocumentPath));
			} else if (child.Is ("implements")) {
				ParseAndAddChild (child, new ApiImplements (DocumentPath));
			} else if (child.Is ("method")) {
				ParseAndAddChild (child, new ApiMethod (DocumentPath));
			} else if (child.Is ("typeParameters")) {
				var tp = new ApiTypeParameters (DocumentPath);
				tp.Parse (child);
				AddChildRange (tp.ChildElements);
			} else
				base.ParseChild(parent, child);
		}
	}
}
