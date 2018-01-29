//
// HierarchyField.cs
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
using System.Text;

using Java.Interop.Bindings.Syntax;

namespace Java.Interop.Bindings.Compiler
{
	public class HierarchyField : HierarchyTypeMember
	{
		const char CamelCaseSeparator = '_';
		static readonly char[] CamelCaseSplitChars = {CamelCaseSeparator};

		ApiField apiField;

		protected ApiField ApiField => apiField;

		public bool Transient { get; set; }
		public string Type { get; set; }
		public string TypeGenericAware { get; set; }
		public string Value { get; set; }
		public bool Volatile { get; set; }

		public HierarchyField (GeneratorContext context, HierarchyObject parent) : base (context, parent)
		{ }

		public override void Init (ApiElement apiElement)
		{
			base.Init (apiElement);

			apiField = EnsureApiElementType<ApiField> (apiElement);
			Transient = apiField.Transient;
			Type = apiField.Type;
			TypeGenericAware = apiField.TypeGenericAware;
			Value = apiField.Value;
			Volatile = apiField.Volatile;
		}

		protected override (string ManagedName, string FullManagedName) GenerateManagedNames ()
		{
			string managedName;

			if (!String.IsNullOrEmpty (ManagedName))
				managedName = ManagedName;
			else if (Name.IndexOf (CamelCaseSeparator) >= 0) {
				var sb = new StringBuilder ();
				foreach (string segment in Name.Split(CamelCaseSplitChars, StringSplitOptions.RemoveEmptyEntries)) {
					sb.Append (JavaNameToManagedName (segment.ToLower ()));
				}
				managedName = sb.ToString ();
			} else
				managedName = JavaNameToManagedName (Name);

			return (managedName, managedName);
		}
	}
}
