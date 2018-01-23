//
// HierarchyElement.cs
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

using Java.Interop.Bindings.Syntax;

namespace Java.Interop.Bindings.Compiler
{
	public abstract class HierarchyElement : HierarchyBase
	{
		List<HierarchyElement> members;
		List<HierarchyCustomAttribute> customAttributes;
		List<string> comments;

		ApiElement apiElement;
		protected ApiElement ApiElement => apiElement;

		public string FullName { get; internal set; }
		public string ManagedName { get; internal set; }
		public string FullManagedName { get; internal set; }
		public IList<HierarchyElement> Members => members;
		public string Name { get; internal set; }
		public string NameGenericAware { get; internal set; }
		public HierarchyBase Parent { get; }
		public HierarchyElement ParentElement => Parent as HierarchyElement;
		public IList<HierarchyCustomAttribute> CustomAttributes => customAttributes;
		public IList<string> Comments => comments;

		public string DocumentPath { get; set; }
		public string DeprecatedMessage { get; set; }
		public bool IsDeprecated { get; set; }
		public string DeprecatedSinceAPI { get; set; }
		public string AvailableSinceAPI { get; set; }
		public string AvailableUntilAPI { get; set; }
		public IDictionary<string, string> UnprocessedAttributes => apiElement?.OtherAttributes;

		public bool UseGlobal { get; set; } = false;

		protected HierarchyElement (HierarchyBase parent) 
		{
			Parent = parent ?? throw new ArgumentNullException (nameof (parent));
			IsBoundAPI = false;
		}

		public virtual void Init (ApiElement apiElement)
		{
			if (apiElement == null)
				throw new ArgumentNullException (nameof (apiElement));

			IsBoundAPI = true;

			Name = apiElement.Name;
			FullName = Name;
			ManagedName = apiElement.ManagedName;
			FullManagedName = ManagedName;

			this.apiElement = apiElement;
			DocumentPath = apiElement.DocumentPath;
			DeprecatedMessage = apiElement.DeprecatedMessage;
			IsDeprecated = apiElement.IsDeprecated;
			DeprecatedSinceAPI = apiElement.DeprecatedSinceAPI;
			AvailableSinceAPI = apiElement.AvailableSinceAPI;
			AvailableUntilAPI = apiElement.AvailableUntilAPI;
		}

		public virtual void Init ()
		{}

		public void AddComment (string commentLine)
		{
			if (comments == null)
				comments = new List <string> ();
			comments.Add (commentLine ?? String.Empty);
		}

		public void AddCustomAttribute (HierarchyCustomAttribute attribute)
		{
			if (attribute == null)
				throw new ArgumentNullException (nameof (attribute));
			if (customAttributes == null)
				customAttributes = new List<HierarchyCustomAttribute> ();
			if (customAttributes.Contains (attribute))
				return;
			customAttributes.Add (attribute);
		}

		public void AddMember (HierarchyElement member)
		{
			if (member == null)
				throw new ArgumentNullException (nameof (member));

			Helpers.AddToList (member, ref members);
			MemberAdded (member);
		}

		protected virtual void MemberAdded (HierarchyElement member)
		{
		}

		public void RemoveMember (HierarchyElement member)
		{
			if (member == null)
				throw new ArgumentNullException (nameof (member));
			if (members == null || members.Count == 0)
				return;
			members.Remove (member);
		}

		protected void AssertType <T> (T element) where T: ApiElement
		{
			if (element == null)
				throw new ArgumentException ($"expected argument of type '{typeof (T).FullName}'", nameof (element));
		}
	}
}
