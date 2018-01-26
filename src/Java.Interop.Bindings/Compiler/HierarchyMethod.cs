//
// HierarchyMethod.cs
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
	public class HierarchyMethod : HierarchyTypeMember
	{
		ApiMethod apiMethod;

		protected ApiMethod ApiMethod => apiMethod;

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

		public HierarchyMethod (HierarchyObject parent) : base (parent)
		{ }

		public override void Init (ApiElement apiElement)
		{
			base.Init (apiElement);

			apiMethod = EnsureApiElementType<ApiMethod> (apiElement);
			Abstract = apiMethod.Abstract;
			ArgsType = apiMethod.ArgsType;
			EnumReturn = apiMethod.EnumReturn;
			EventName = apiMethod.EventName;
			GenerateAsyncWrapper = apiMethod.GenerateAsyncWrapper;
			GenerateDispatchingSetter = apiMethod.GenerateDispatchingSetter;
			ManagedReturn = apiMethod.ManagedReturn;
			Native = apiMethod.Native;
			PropertyName = apiMethod.PropertyName;
			Return = apiMethod.Return;
			JniReturn = apiMethod.JniReturn;
			Synchronized = apiMethod.Synchronized;
			IsBridge = apiMethod.IsBridge;
			IsSynthetic = apiMethod.IsSynthetic;
		}

		protected override (string ManagedName, string FullManagedName) GenerateManagedNames ()
		{
			string managedName = GetManagedName ();
			return (managedName, managedName);
		}
	}
}
