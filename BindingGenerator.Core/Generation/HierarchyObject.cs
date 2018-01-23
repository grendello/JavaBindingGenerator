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

using BindingGenerator.Core.Parsing;

namespace BindingGenerator.Core.Generation
{
	public abstract class HierarchyObject : HierarchyElement
	{
		ApiType apiType;
		List<string> baseTypeNames;
		List<HierarchyObject> baseTypes;

		protected ApiType ApiType => apiType;

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

			apiType = ApiElement as ApiType;

			(string FullName, string FullManagedName) names = MakeFullyQualifiedName ();
			FullName = names.FullName;
			FullManagedName = names.FullManagedName;

			if (apiElement.ChildElements != null) {
				foreach (ApiImplements ai in apiElement.ChildElements.OfType<ApiImplements> ().Where (o => o != null)) {
					string name = ai.NameGenericAware?.Trim ();
//					if (String.IsNullOrEmpty (name))
						name = ai.Name;
					AddImplements (name);
				}
			}

			if (apiType == null)
				return;

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

		public void ResolveBaseTypes (Dictionary<string, HierarchyObject> typeIndex)
		{
			if (baseTypes != null)
				return;

			if (typeIndex == null)
				throw new ArgumentNullException (nameof (typeIndex));

			if (baseTypeNames != null && baseTypeNames.Count > 0) {
				foreach (string typeName in baseTypeNames) {
					HierarchyObject o = LookupType (typeIndex, typeName);
				}
			}

			AddBaseTypes (typeIndex);

			if(baseTypes == null)
				AddDefaultBaseType (typeIndex);
		}

		protected virtual void AddBaseTypes (Dictionary<string, HierarchyObject> typeIndex)
		{}

		void AddDefaultBaseType (Dictionary<string, HierarchyObject> typeIndex)
		{
			string typeName = null;

			if (this is HierarchyInterface) {
				typeName = Hierarchy.DefaultInterfaceBaseType;
			} else if (this is HierarchyClass) {
				typeName = Hierarchy.DefaultClassBaseType;
			} else
				throw new InvalidOperationException($"Unsupported hierarchy type {GetType ()}");

			AddBaseType (LookupType (typeIndex, typeName));
		}

		protected HierarchyObject LookupType (Dictionary<string, HierarchyObject> typeIndex, string typeName)
		{
			if (!typeIndex.TryGetValue (typeName, out HierarchyObject type) || type == null)
				throw new InvalidOperationException ($"Type '{typeName}' not known.");
			return type;
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

		(string FullName, string FullManagedName) MakeFullyQualifiedName ()
		{
			if (String.IsNullOrEmpty (Name))
				throw new InvalidOperationException ("Name is required");

			var name = new List<string> {
				Name
			};

			List <string> managedName;
			if (String.IsNullOrEmpty (ManagedName))
				managedName = null;
			else {
				managedName = new List<string> {
					ManagedName
				};
			}

			HierarchyBase parent = Parent;
			while (parent != null) {
				var element = parent as HierarchyElement;
				if (element == null)
					break;

				name.Add (element.Name);
				managedName?.Add (element.ManagedName ?? element.Name);
				parent = element.Parent;
			}

			return (MakeName (name), MakeName (managedName));
		}

		string MakeName (List<string> components)
		{
			if (components == null)
				return String.Empty;
			components.Reverse ();
			return String.Join (".", components);
		}
	}
}
