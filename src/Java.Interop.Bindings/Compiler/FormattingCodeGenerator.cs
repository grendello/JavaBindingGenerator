//
// FormattingCodeGenerator.cs
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
using System.IO;

namespace Java.Interop.Bindings.Compiler
{
	public abstract class FormattingCodeGenerator
	{
		public FormattingContext Context { get; }

		public abstract string FileExtension { get; }
		public abstract string LanguageName { get; }

		protected FormattingCodeGenerator (FormattingContext context)
		{
			Context = context ?? throw new ArgumentNullException (nameof (context));
		}

		public abstract void AddComment (Stream stream, string text);
		public abstract void StartNamespace (Stream stream, string name);
		public abstract void EndNamespace (Stream stream, string name);
		public abstract void AddNamespaceImport (Stream stream, string name);

		public abstract void AddMember (HierarchyField field);
		public abstract void AddMember (HierarchyMethod method);
		public abstract void AddMember (HierarchyInterface iface);
		public abstract void AddMember (HierarchyEnum enm);
		public abstract void Addmember (HierarchyClass klass);

		protected abstract TypeMemberCodeGenerator GetTypeMemberGenerator (HierarchyTypeMember member);
		protected abstract ClassCodeGenerator GetClassGenerator (HierarchyClass klass);
		protected abstract EnumCodeGenerator GetEnumGenerator (HierarchyEnum enm);
		protected abstract InterfaceCodeGenerator GetInterfaceGenerator (HierarchyInterface iface);
	}
}
