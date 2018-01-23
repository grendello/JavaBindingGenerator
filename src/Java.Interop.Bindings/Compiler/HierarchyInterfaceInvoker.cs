//
// HierarchyInterfaceInvoker.cs
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

namespace Java.Interop.Bindings.Compiler
{
	// TODO: invoker managed names must be generated after we have the interface name resolved - add amechanism to
	// do that in Hierarchy
	public class HierarchyInterfaceInvoker : HierarchyClass
	{
		HierarchyInterface InvokedInterface { get; }

		public HierarchyInterfaceInvoker (HierarchyElement parent, HierarchyInterface invokedInterface) : base (parent)
		{
			InvokedInterface = invokedInterface ?? throw new ArgumentNullException (nameof (invokedInterface));
		}

		public override void Init ()
		{
			base.Init ();

			Name = MakeInvokerName (InvokedInterface.Name);
			ManagedName = MakeInvokerName (InvokedInterface.ManagedName, InvokedInterface.Name);
			FullName = MakeInvokerName (InvokedInterface.FullName, isFullName: true);
			FullManagedName = MakeInvokerName (InvokedInterface.FullManagedName, InvokedInterface.FullName, true);
		}

		string MakeInvokerName (string interfaceName, string fallBack = null, bool isFullName = false)
		{
			string name;
			if (String.IsNullOrEmpty (interfaceName)) {
				if (String.IsNullOrEmpty (fallBack))
					throw new InvalidOperationException ("Cannot generate interface invoker classw without a name");
				name = fallBack;
			} else
				name = interfaceName;

			string ns = String.Empty;
			if (isFullName) {
				ns = Helpers.GetTypeNamespace (name);
				if (!String.IsNullOrEmpty (ns))
					ns += ".";
			}

			return $"{ns}{name}Invoker";
		}
	}
}
