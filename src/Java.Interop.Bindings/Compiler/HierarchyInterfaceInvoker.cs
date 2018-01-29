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

using Java.Interop.Bindings.Syntax;

namespace Java.Interop.Bindings.Compiler
{
	public class HierarchyInterfaceInvoker : HierarchyClass
	{
		HierarchyInterface InvokedInterface { get; }

		public HierarchyInterfaceInvoker (GeneratorContext context, HierarchyElement parent, HierarchyInterface invokedInterface) : base (context, parent)
		{
			InvokedInterface = invokedInterface ?? throw new ArgumentNullException (nameof (invokedInterface));
			Visibility = ApiVisibility.Internal;
		}

		public override void Init ()
		{
			base.Init ();

			Name = MakeInvokerName (InvokedInterface.Name);
			FullName = GenerateFullJavaName ();

			Logger.Debug ($"Invoker init: name {Name} (for interface {InvokedInterface.Name})");
		}

		protected override (string ManagedName, string FullManagedName) GenerateManagedNames ()
		{
			(string ManagedName, string FullManagedName) names = base.GenerateManagedNames ();

			return (MakeInvokerName (names.ManagedName), MakeInvokerName (names.FullManagedName));
		}

		string MakeInvokerName (string baseName)
		{
			if (String.IsNullOrEmpty (baseName))
				return String.Empty;

			if (baseName.EndsWith ("Invoker", StringComparison.Ordinal))
				return baseName;

			return $"{baseName}Invoker";
		}
	}
}
