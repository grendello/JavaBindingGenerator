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
	public abstract class HierarchyElement : HierarchyBase//, IEquatable <HierarchyElement>
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
		public bool HasMembers => members != null && members.Count > 0;
		public string Name { get; internal set; }
		public string NameGenericAware { get; internal set; }
		public string JniSignature { get; internal set; }
		public HierarchyBase Parent { get; private set; }
		public HierarchyElement ParentElement => Parent as HierarchyElement;
		public IList<HierarchyCustomAttribute> CustomAttributes => customAttributes;
		public IList<string> Comments => comments;

		public string DocumentPath { get; private set; }
		public int SourceLine { get; private set; } = -1;
		public int SourceColumn { get; private set; } = -1;
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

			this.apiElement = apiElement;

			IsBoundAPI = true;

			Name = apiElement.Name;
			FullName = GenerateFullJavaName ();
			ManagedName = apiElement.ManagedName;
			FullManagedName = null;
			JniSignature = apiElement.JniSignature;

			DocumentPath = apiElement.DocumentPath;
			DeprecatedMessage = apiElement.DeprecatedMessage;
			IsDeprecated = apiElement.IsDeprecated;
			DeprecatedSinceAPI = apiElement.DeprecatedSinceAPI;
			AvailableSinceAPI = apiElement.AvailableSinceAPI;
			AvailableUntilAPI = apiElement.AvailableUntilAPI;
			SourceLine = apiElement.SourceLine;
			SourceColumn = apiElement.SourceColumn;
		}

		public virtual void Init ()
		{}

		internal void SetManagedNames ()
		{
			(string ManagedName, string FullManagedName) names = GenerateManagedNames ();
			FullManagedName = names.FullManagedName ?? String.Empty;
			ManagedName = names.ManagedName;
		}

		protected string GetManagedName ()
		{
			if (String.IsNullOrEmpty (ManagedName))
				return JavaNameToManagedName (Name);
			return EnsureValidIdentifier (ManagedName);
		}

		protected void EnsureName ()
		{
			if (String.IsNullOrEmpty (Name))
				throw new InvalidOperationException ("Name must not be null or empty");
		}

		protected virtual string GenerateFullJavaName ()
		{
			string name = Name;
			EnsureName ();
			return BuildFullName (Name, (HierarchyElement parent) => parent.Name);
		}

		protected virtual string BuildFullName (string firstSegment, Func<HierarchyElement, string> getSegment)
		{
			if (String.IsNullOrEmpty (firstSegment))
				throw new ArgumentException ("must not be null or empty", nameof (firstSegment));
			if (getSegment == null)
				throw new ArgumentNullException (nameof (getSegment));

			var segments = new List<string> {};
			if (SplitName (firstSegment, out List<string> split))
				segments.AddRange (split);
			else
				segments.Add (firstSegment);

			HierarchyBase parent = Parent;
			while (parent != null) {
				var element = parent as HierarchyElement;
				if (element == null)
					break;

				string segment = getSegment (element);
				if (String.IsNullOrEmpty (segment))
					throw new InvalidOperationException ("Element name segment must not be null or empty");
				if (SplitName (segment, out split))
					segments.AddRange (split);
				else
					segments.Add (segment);
				parent = element.Parent;
			}

			// string dbg = String.Join (" → ", segments);
			// Logger.Debug ($"Full name segments for {Name} ({GetType ().FullName}): {dbg}");
			return MakeFullName (segments);
		}

		bool SplitName (string name, out List<string> split)
		{
			split = null;
			if (name.IndexOf ('.') < 0)
				return false;
			split = new List <string> ();
			foreach (string n in name.Split ('.'))
				split.Insert (0, n);
			return true;
		}

		string MakeFullName (List<string> segments)
		{
			if (segments == null)
				return String.Empty;
			segments.Reverse ();
			return String.Join (".", segments);
		}

		protected abstract (string ManagedName, string FullManagedName) GenerateManagedNames ();

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
			member.Parent = this;
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

		// public bool Equals (HierarchyElement other)
		// {
		// 	if (other == null)
		// 		return false;

		// 	throw new NotImplementedException();
		// }

		// public override bool Equals (object other)
		// {
		// 	if (other == null)
		// 		return false;

		// 	return Equals (other as HierarchyElement);
		// }

		// public override int GetHashCode ()
		// {
		// 	return base.GetHashCode ();
		// }
	}
}
