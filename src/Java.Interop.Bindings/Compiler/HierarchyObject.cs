//
// HierarchyObject.cs
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
using System.Linq;

using Java.Interop.Bindings.Syntax;

namespace Java.Interop.Bindings.Compiler
{
	public abstract class HierarchyObject : HierarchyElement
	{
		ApiType apiType;
		List<string> baseTypeNames;
		List<HierarchyObject> baseTypes;

		protected ApiType ApiType => apiType;
		protected bool DoNotAddBaseTypes { get; set; }

		public bool Abstract { get; set; }
		public bool Static { get; set; }
		public ApiVisibility Visibility { get; set; }
		public bool Final { get; set; }

		public IList<HierarchyObject> BaseTypes => baseTypes;

		protected HierarchyObject (HierarchyBase parent) : base (parent)
		{
		}

		public override void Init (ApiElement apiElement)
		{
			base.Init (apiElement);

			apiType = EnsureApiElementType<ApiType>(apiElement);
			if (apiElement.ChildElements != null) {
				foreach (ApiImplements ai in apiElement.ChildElements.OfType<ApiImplements> ().Where (o => o != null)) {
					string name = ai.NameGenericAware?.Trim ();
//					if (String.IsNullOrEmpty (name))
						name = ai.Name;
					AddImplements (name);
				}
			}

			Abstract = apiType.Abstract;
			Static = apiType.Static;
			Visibility = apiType.Visibility;
			Final = apiType.Final;
		}

		public void AddImplements (string typeName)
		{
			AddBaseType (typeName);
		}

		public void AddBaseType (string typeName)
		{
			if (String.IsNullOrEmpty (typeName))
				return;
			Helpers.AddToList (typeName, ref baseTypeNames);
		}

		public void ResolveBaseTypes (HierarchyIndex typeIndex)
		{
			if (DoNotAddBaseTypes || baseTypes != null)
				return;

			if (typeIndex == null)
				throw new ArgumentNullException (nameof (typeIndex));

			if (baseTypeNames != null && baseTypeNames.Count > 0) {
				string kindHint = HierarchyIndex.TypeToPrefix<HierarchyInterface> ();
				foreach (string typeName in baseTypeNames) {
					HierarchyElement e = typeIndex.Lookup (typeName, kindHint, this);
				}
			}

			AddBaseTypes (typeIndex);

			if(baseTypes == null)
				AddDefaultBaseType (typeIndex);
		}

		protected virtual void AddBaseTypes (HierarchyIndex typeIndex)
		{
			if (typeIndex == null)
				throw new ArgumentNullException (nameof (typeIndex));
		}

		void AddDefaultBaseType (HierarchyIndex typeIndex)
		{
			string typeName = null;
			string kindHint = null;

			if (this is HierarchyInterface) {
				typeName = Hierarchy.DefaultInterfaceBaseType;
				kindHint = HierarchyIndex.TypeToPrefix<HierarchyInterface> ();
			} else if (this is HierarchyClass) {
				typeName = Hierarchy.DefaultClassBaseType;
				kindHint = HierarchyIndex.TypeToPrefix<HierarchyClass> ();
			} else
				throw new InvalidOperationException($"Unsupported hierarchy type {GetType ()}");

			AddBaseType (typeName);
		}

		protected void AddBaseType (HierarchyObject baseType)
		{
			if (baseType == null)
				throw new ArgumentNullException (nameof (baseType));

			if (baseTypes == null)
				baseTypes = new List<HierarchyObject> ();
			else if (baseTypes.Contains (baseType))
				return;
			baseTypes.Add (baseType);
		}

		protected override (string ManagedName, string FullManagedName) GenerateManagedNames ()
		{
			if (String.IsNullOrEmpty (Name))
				throw new InvalidOperationException ("Name is required");

			string managedName = GetManagedName ();
			return (managedName, BuildFullName (managedName, (HierarchyElement parent) => parent.GetManagedName ()));
		}
	}
}
