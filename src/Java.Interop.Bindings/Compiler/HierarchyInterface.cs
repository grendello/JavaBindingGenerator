﻿//
// HierarchyInterface.cs
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

namespace Java.Interop.Bindings.Compiler
{
	public class HierarchyInterface : HierarchyObject
	{
		public HierarchyInterfaceInvoker Invoker { get; set; }
		public bool InvokerNotNeeded { get; set; }

		public HierarchyInterface (GeneratorContext context, HierarchyElement parent) : base (context, parent)
		{}

		protected override (string ManagedName, string FullManagedName) GenerateManagedNames ()
		{
			if (String.IsNullOrEmpty (Name))
				throw new InvalidOperationException ("Name is required");

			string managedName = GetManagedName ();
			if (managedName [0] != 'I')
				managedName = $"I{managedName}";

			return (managedName, BuildFullName (managedName, (HierarchyElement parent) => parent.GetManagedName ()));
		}
	}
}
