//
// ApiElement.cs
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
using System.Xml;
using System.Xml.Linq;

namespace BindingGenerator.Core.Parsing
{
	/// <summary>
	/// Represents a single API member (type, method, field) read from the API description
	/// file. The class contains only properties that constitue API member "metadata", that
	/// is aren't used to set C# language properties in the generated code (such as visibility,
	/// return type etc).
	/// </summary>
	public class ApiElement
	{
		static readonly char[] visibilitySplitChars = {' '};

		Dictionary<string, string> otherAttributes;
		List<ApiElement> childElements;

		public string DeprecatedMessage { get; set; }

		/// <summary>
		/// Path to the XML document this element was read from
		/// </summary>
		/// <value>The document path.</value>
		public string DocumentPath { get; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:JavaGenerator.Core.ApiElement"/> is deprecated.
		/// </summary>
		/// <value><c>true</c> if is deprecated; otherwise, <c>false</c>.</value>
		public bool IsDeprecated { get; set; }

		/// <summary>
		/// If <see cref="IsDeprecated"/> is <c>true</c>, gets the platform number/code name since which the API member has been 
		/// deprecated.
		/// </summary>
		/// <value>Platform number/code name</value>
		public string DeprecatedSinceAPI { get; set; }

		/// <summary>
		/// Gets or sets platform number/code name since which the API member is available
		/// </summary>
		/// <value>The available since API.</value>
		public string AvailableSinceAPI { get; set; }

		/// <summary>
		/// Gets or sets platform number/code name since until which the API member is available
		/// </summary>
		/// <value>The available until API.</value>
		public string AvailableUntilAPI { get; set; }

		/// <summary>
		/// A dictionary of XML attributes that weren't converted into properties or otherwise interpreted by this
		/// class or any of its descendants.
		/// </summary>
		/// <value>Dictionary of XML attributes.</value>
		public Dictionary<string, string> OtherAttributes => otherAttributes;

		public IList<ApiElement> ChildElements => childElements;
		public string MergeSourceFile { get; set; }
		public string Name { get; set; }
		public string NameGenericAware { get; set; }
		public string ManagedName { get; set; }
		public string JniSignature { get; set; }
		public int SourceLine { get; private set; } = -1;
		public int SourceColumn { get; private set; } = -1;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:JavaGenerator.Core.ApiElement"/> class.
		/// </summary>
		/// <param name="documentPath">Source XML document path.</param>
		public ApiElement (string documentPath)
		{
			DocumentPath = documentPath;
		}

		/// <summary>
		/// Processes/interprets an XML attribute. If an attribute isn't known to the class (<see cref="GetKnownAttributes"/>)
		/// it's name and value are stored in the <see cref="OtherAttributes"/> dictionary. There's usually no need to override
		/// this method, unless a child class needs to process the "unknown" attributes differently or has special requirements
		/// when handling the attribute element itself.
		/// </summary>
		/// <param name="attribute">XML Attribute.</param>
		public virtual void SetAttribute (Dictionary<string, Action<string, XAttribute>> knownAttributes, XAttribute attribute)
		{
			if (attribute == null)
				throw new ArgumentNullException (nameof (attribute));

			Action<string, XAttribute> setter;
			if (knownAttributes != null && knownAttributes.TryGetValue (attribute.Name.LocalName, out setter) && setter != null) {
				setter (attribute.Value, attribute);
				return;
			}

			if (otherAttributes == null)
				otherAttributes = new Dictionary<string, string> (StringComparer.Ordinal);

			if (otherAttributes.ContainsKey (attribute.Name.LocalName))
				Logger.Warning ($"Attribute '{attribute.Name.LocalName}' redefined for element {attribute.Parent.Name} at {DocumentPath}[{attribute.GetLineInfo ()}]");

			otherAttributes [attribute.Name.LocalName] = attribute.Value;
		}

		public void Parse (XElement element)
		{
			if (element == null)
				throw new ArgumentNullException (nameof (element));

			var lineInfo = element as IXmlLineInfo;
			if (lineInfo != null) {
				SourceLine = lineInfo.LineNumber;
				SourceColumn = lineInfo.LinePosition;
			}

			Dictionary<string, Action<string, XAttribute>> knownAttributes = GetKnownAttributes ();
			foreach (XAttribute attr in element.Attributes ())
				SetAttribute (knownAttributes, attr);

			if (!element.HasElements)
				return;

			foreach (XElement childElement in element.Elements ()) {
				ParseChild (element, childElement);
			}

			ParseCompleted (element);
			DumpOtherAttributes (element);
		}

		protected virtual void ParseChild (XElement parent, XElement child)
		{
			Logger.Warning ($"Unknown '{parent.Name}' child element '{child.Name}' at {DocumentPath}:{child.GetLineInfo ()}");
		}

		protected virtual void ParseCompleted (XElement element)
		{ }

		public virtual void EnsureValid (XElement element)
		{ }

		/// <summary>
		/// Returns a dictionary mapping names of known attributes to code which handles them. If a child class
		/// overrides this method it should call the base class's method in most cases and modify the returned
		/// dictionary.
		/// Method which handles an attribute is passed the attribute's value as its first parameter and the
		/// attribute itself as the second parameter.
		/// </summary>
		/// <returns>Attributes known to this class and their handler methods.</returns>
		protected virtual Dictionary<string, Action<string, XAttribute>> GetKnownAttributes ()
		{
			Dictionary<string, Action<string, XAttribute>> ret = EnsureKnownAttributes (null);

			ret ["deprecated"] = (string value, XAttribute attr) => {
				IsDeprecated = String.Compare ("deprecated", value, StringComparison.OrdinalIgnoreCase) == 0;
				if (!IsDeprecated || String.Compare ("not deprecated", value, StringComparison.OrdinalIgnoreCase) == 0)
					return;
				DeprecatedMessage = value;
			};
			ret ["deprecated-since"] = (string value, XAttribute attr) => DeprecatedSinceAPI = value?.Trim ();
			ret ["api-since"] = (string value, XAttribute attr) => AvailableSinceAPI = value?.Trim ();
			ret ["api-until"] = (string value, XAttribute attr) => AvailableUntilAPI = value?.Trim ();
			ret ["merge.SourceFile"] = (string value, XAttribute attr) => MergeSourceFile = value?.Trim ();
			ret ["name"] = (string value, XAttribute attr) => Name = value?.Trim ();
			ret ["managedName"] = (string value, XAttribute attr) => ManagedName = value?.Trim ();
			ret ["jni-signature"] = (string value, XAttribute attr) => JniSignature = value?.Trim ();

			return ret;
		}

		protected Dictionary<string, Action<string, XAttribute>> EnsureKnownAttributes (Dictionary<string, Action<string, XAttribute>> knownAttributes)
		{
			if (knownAttributes != null)
				return knownAttributes;

			return new Dictionary<string, Action<string, XAttribute>> (StringComparer.Ordinal);
		}

		protected void DumpOtherAttributes (XElement element)
		{
			if (otherAttributes == null || otherAttributes.Count == 0)
				return;

			Logger.Debug ($"Unprocessed attributes for element '{element.Name}' at {element.GetLineInfo ()}");
			foreach (var kvp in otherAttributes) {
				Logger.Debug ($"   {kvp.Key} == {kvp.Value}");
			}
		}

		protected void ParseAndAddChild (XElement element, ApiElement thing)
		{
			if (thing == null)
				return;
			thing.Parse (element);
			AddToList (thing, ref childElements);
		}

		protected void AddChildRange (IList<ApiElement> things)
		{
			if (things == null || things.Count == 0)
				return;
			AddRangeToList (things, ref childElements);
		}

		protected void AddToList<T> (T thing, ref List<T> thingList) where T : ApiElement
		{
			if (thing == null)
				return;

			if (thingList == null)
				thingList = new List<T> ();
			thingList.Add (thing);
		}

		protected void AddRangeToList<T> (IList <T> things, ref List<T> thingList) where T : ApiElement
		{
			if (things == null || things.Count == 0)
				return;

			if (thingList == null)
				thingList = new List<T> ();
			thingList.AddRange (things);
		}

		protected ApiVisibility ParseVisibility (string visibility)
		{
			string v = visibility?.Trim ();
			if (String.IsNullOrEmpty (v))
				return ApiVisibility.Unknown;

			if (String.Compare ("public", visibility, StringComparison.OrdinalIgnoreCase) == 0)
				return ApiVisibility.Public;

			if (String.Compare ("private", visibility, StringComparison.OrdinalIgnoreCase) == 0)
				return ApiVisibility.Private;

			if (String.Compare ("protected", visibility, StringComparison.OrdinalIgnoreCase) == 0)
				return ApiVisibility.Protected;

			if (String.Compare ("internal", visibility, StringComparison.OrdinalIgnoreCase) == 0)
				return ApiVisibility.Internal;

			string[] parts = visibility.Split (visibilitySplitChars, StringSplitOptions.RemoveEmptyEntries);
			if (PartsEqual ("protected", "internal", parts))
				return ApiVisibility.ProtectedInternal;

			if (PartsEqual ("private", "protected", parts))
				return ApiVisibility.PrivateProtected;

			throw new InvalidOperationException ($"Unsupported visibility value: {visibility}");
		}

		bool PartsEqual (string first, string second, string[] parts)
		{
			if (parts == null || parts.Length != 2)
				return false;

			return (String.Compare (first, parts [0], StringComparison.OrdinalIgnoreCase) == 0 && String.Compare (second, parts [1], StringComparison.OrdinalIgnoreCase) == 0) ||
				(String.Compare (first, parts [1], StringComparison.OrdinalIgnoreCase) == 0 && String.Compare (second, parts [1], StringComparison.OrdinalIgnoreCase) == 0);
		}
	}
}
