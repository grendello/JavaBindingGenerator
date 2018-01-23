//
// ApiNameSpace.cs
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
	public class ApiNameSpace : ApiElement
	{
		public string JniName { get; set; }

		public ApiNameSpace (string documentPath) : base (documentPath)
		{}

		protected override Dictionary<string, Action<string, XAttribute>> GetKnownAttributes ()
		{
			Dictionary<string, Action<string, XAttribute>> ret = EnsureKnownAttributes (base.GetKnownAttributes ());

			ret ["jni-name"] = (string value, XAttribute attr) => JniName = value?.Trim ();

			return ret;
		}

		protected override void ParseChild (XElement parent, XElement child)
		{
			ApiType apiType;

			if (child.Is ("class"))
				apiType = new ApiClass (DocumentPath);
			else if (child.Is ("interface"))
				apiType = new ApiInterface (DocumentPath);
			else
				apiType = null;

			if (apiType == null) {
				base.ParseChild (parent, child);
				return;
			}

			ParseAndAddChild (child, apiType);
		}
	}
}
