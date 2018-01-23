//
// ApiMethod.cs
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
	public class ApiMethod : ApiTypeMember
	{
		public bool Abstract { get; set; }
		public string ArgsType { get; set; }
		public string EnumReturn { get; set; }
		public string EventName { get; set; }
		public bool GenerateAsyncWrapper { get; set; }
		public bool GenerateDispatchingSetter { get; set; }
		public string ManagedReturn { get; set; }
		public bool Native { get; set; }
		public string PropertyName { get; set; }
		public string Return { get; set; }
		public string JniReturn { get; set; }
		public bool Synchronized { get; set; }
		public bool IsBridge { get; set; }
		public bool IsSynthetic { get; set; }

		public ApiMethod (string documentPath) : base (documentPath)
		{ }

		protected override Dictionary<string, Action<string, XAttribute>> GetKnownAttributes ()
		{
			Dictionary<string, Action<string, XAttribute>> ret = EnsureKnownAttributes (base.GetKnownAttributes ());

			ret ["abstract"] = (string value, XAttribute attr) => Abstract = value.AsBool (attr);
			ret ["argsType"] = (string value, XAttribute attr) => ArgsType = value?.Trim ();
			ret ["enumReturn"] = (string value, XAttribute attr) => EnumReturn = value?.Trim ();
			ret ["eventName"] = (string value, XAttribute attr) => EventName = value?.Trim ();
			ret ["generateAsyncWrapper"] = (string value, XAttribute attr) => GenerateAsyncWrapper = value.AsBool (attr);
			ret ["generateDispatchingSetter"] = (string value, XAttribute attr) => GenerateDispatchingSetter = value.AsBool (attr);
			ret ["managedReturn"] = (string value, XAttribute attr) => ManagedReturn = value?.Trim ();
			ret ["native"] = (string value, XAttribute attr) => Native = value.AsBool (attr);
			ret ["propertyName"] = (string value, XAttribute attr) => PropertyName = value?.Trim ();
			ret ["return"] = (string value, XAttribute attr) => Return = value?.Trim ();
			ret ["synchronized"] = (string value, XAttribute attr) => Synchronized = value.AsBool (attr);
			ret ["bridge"] = (string value, XAttribute attr) => IsBridge = value.AsBool (attr);
			ret ["jni-return"] = (string value, XAttribute attr) => JniReturn = value?.Trim ();
			ret ["synthetic"] = (string value, XAttribute attr) => IsSynthetic = value.AsBool (attr);

			return ret;
		}

		protected override void ParseChild (XElement parent, XElement child)
		{
			if (child.Is ("parameter")) {
				ParseAndAddChild (child, new ApiMethodParameter (DocumentPath));
			} else if (child.Is ("exception")) {
				ParseAndAddChild (child, new ApiException (DocumentPath));
			} else if (child.Is ("typeParameters")) {
				var tp = new ApiTypeParameters (DocumentPath);
				tp.Parse (child);
				AddChildRange (tp.ChildElements);
			} else
				base.ParseChild (parent, child);
		}
	}
}
