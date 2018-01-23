//
// XamarinAndroidHierarchy.cs
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
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Java.Interop.Bindings.Syntax;

namespace Java.Interop.Bindings.Compiler.Xamarin
{
	public class XamarinAndroidHierarchy : Hierarchy
	{
		protected override void SynthesizeElements ()
		{
			foreach (HierarchyNamespace ns in Namespaces) {
				if (ns == null || ns.Members == null || ns.Members.Count == 0)
					continue;

				foreach (HierarchyInterface iface in ns.Members.OfType<HierarchyInterface> ().ToList ()) {
					GenerateInvoker (iface);
				}
			}
		}

		protected virtual void GenerateInvoker (HierarchyInterface iface)
		{
			if (iface.Invoker != null) {
				Logger.Verbose ($"Interface {iface.Name} already has an invoker class attached");
				return;
			}

			var parent = iface.Parent as HierarchyElement;
			if (parent == null)
				throw new InvalidOperationException($"Invoker requires that its associated interface ({iface.FullName}) is a child of HierarchyElement");

			var invoker = new HierarchyInterfaceInvoker (iface.Parent as HierarchyElement, iface) {
				Visibility = ApiVisibility.Internal
			};
			invoker.Init ();
			invoker.AddBaseType (Hierarchy.DefaultClassBaseType);
			invoker.AddImplements (iface.FullName);

			parent.AddMember (invoker);
			AddTypeToIndex (invoker);
			iface.Invoker = invoker;
		}
	}
}
