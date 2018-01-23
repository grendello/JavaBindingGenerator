//
// HierarchyClass.cs
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

using BindingGenerator.Core.Parsing;

namespace BindingGenerator.Core.Generation
{
	public class HierarchyClass : HierarchyObject
	{
		ApiClass apiClass;

		protected ApiClass ApiClass => apiClass;

		public string Extends { get; set; }
		public string ExtendsGenericAware { get; set; }
		public bool Obfuscated { get; set; }

		public HierarchyClass (HierarchyElement parent) : base (parent)
		{
		}

		public override void Init (ApiElement apiElement)
		{
			base.Init (apiElement);

			apiClass = ApiElement as ApiClass;
			if (apiClass == null)
				return;

			Extends = apiClass.Extends;
			ExtendsGenericAware = apiClass.ExtendsGenericAware;
			Obfuscated = apiClass.Obfuscated;
		}

		protected override void AddBaseTypes (Dictionary<string, HierarchyObject> typeIndex)
		{
			base.AddBaseTypes (typeIndex);

			if (String.IsNullOrEmpty (Extends))
				return;

			AddBaseType(LookupType (typeIndex, Extends));
		}
	}
}
