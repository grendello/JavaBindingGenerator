//
// HierarchyCustomAttribute.cs
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

namespace BindingGenerator.Core.Generation
{
	// Custom attributes should have their parameters represented as an expression tree (a'la CodeDOM) to represent
	// all the possible combinations in the most flexible way (enabling for instance generation of code in different
	// programming languages). However, for our needs here this would be a major overkill since the generator uses a
	// small and finite set of custom atttributes with a well defined set of parameters. Therefore, we'll just
	// create a number of classes derived from this one to represent each of the attributes we used and extend the
	// collection on as-needed basis. KISS ftw!
	//
	// All properties starting with `Hierarchy` do not represent fields/properties/parameters etc of the represented
	// custom attribute. They are to be considered our metadata.
	//
	public abstract class HierarchyCustomAttribute
	{
		public string HierarchyAttributeName { get; }
		public bool HierarchyAttributeUseGlobal { get; }
		public HierarchyCustomAttributeTarget HierarchyTarget { get; }

		// This is for validation so that our AST validation can catch issues early
		public AttributeTargets HierarchyAttributeUsage { get; }

		protected HierarchyCustomAttribute (string name, bool useGlobal, AttributeTargets usage, HierarchyCustomAttributeTarget target = HierarchyCustomAttributeTarget.None)
		{
			name = name?.Trim ();
			if (String.IsNullOrEmpty (name))
				throw new ArgumentNullException (nameof (name));
			HierarchyAttributeName = name;
			HierarchyAttributeUseGlobal = useGlobal;
			HierarchyAttributeUsage = usage;
			HierarchyTarget = target;
		}
	}
}
