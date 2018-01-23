//
// ApiClass.cs
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

namespace BindingGenerator.Core.Parsing
{
	public class ApiClass : ApiType
	{
		public string Extends { get; set; }
		public string ExtendsGenericAware { get; set; }
		public string JniExtends { get; set; }
		public bool Obfuscated { get; set; }

		public ApiClass (string documentPath) : base (documentPath)
		{ }

		protected override Dictionary<string, Action<string, XAttribute>> GetKnownAttributes ()
		{
			Dictionary<string, Action<string, XAttribute>> ret = EnsureKnownAttributes (base.GetKnownAttributes ());

			ret ["extends"] = (string value, XAttribute attr) => Extends = value?.Trim ();
			ret ["extends-generic-aware"] = (string value, XAttribute attr) => ExtendsGenericAware = value?.Trim ();
			ret ["jni-extends"] = (string value, XAttribute attr) => JniExtends = value?.Trim ();
			ret ["obfuscated"] = (string value, XAttribute attr) => Obfuscated = value.AsBool (attr);

			return ret;
		}

		protected override void ParseChild (XElement parent, XElement child)
		{
			if (child.Is ("constructor")) {
				ParseAndAddChild (child, new ApiConstructor (DocumentPath));
			} else
				base.ParseChild (parent, child);
		}
	}
}
