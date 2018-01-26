//
// HierarchyIndex.cs
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

namespace Java.Interop.Bindings.Compiler
{
	public class HierarchyIndex
	{
		Dictionary <string, HierarchyElement> nativeIndex;
		Dictionary <string, HierarchyElement> managedIndex;

		public int Count => nativeIndex.Count + managedIndex.Count;

		public HierarchyIndex ()
		{
			nativeIndex = new Dictionary<string, HierarchyElement> (StringComparer.Ordinal);
			managedIndex = new Dictionary<string, HierarchyElement> (StringComparer.Ordinal);
		}

		public void Add (HierarchyElement type)
		{
			AddNative (type);
			AddManaged (type);
		}

		public void AddNative (HierarchyElement type)
		{
			if (type == null)
				throw new ArgumentNullException (nameof (type));

			Add (nativeIndex, "native", type.FullName, type);
		}

		public void AddManaged (HierarchyElement type)
		{
			if (type == null)
				throw new ArgumentNullException (nameof (type));

			if (!String.IsNullOrEmpty (type.FullManagedName))
				Add (managedIndex, "managed", type.FullManagedName, type);
		}

		void Add (Dictionary <string, HierarchyElement> index, string indexName, string typeName, HierarchyElement type)
		{
			if (String.IsNullOrEmpty (typeName))
				throw new ArgumentException ("must not be null or empty", nameof (typeName));

			if (String.Compare ("java.lang.Comparable", typeName, StringComparison.OrdinalIgnoreCase) == 0) {
				Logger.Debug ($"java.lang.Comparable being added: {type} ({type.FullName})");
			}

			//Logger.Verbose ($"Index: trying to add '{typeName}' ({type.GetType ().FullName}), parent: {type.ParentElement?.FullName} ({type.ParentElement?.GetType ()?.FullName})");
			HierarchyElement t;
			if (index.TryGetValue (typeName, out t) && t != null) {
				// if (t.GetType () == type.GetType ()) // TODO: implement IEquatable<T> on namespaces,
				// 				     // classes and indexes and don't throw if the two
				// 				     // objects have actually equal contents
				// 	throw new InvalidOperationException ($"Duplicate type entry for type '{typeName}' ({type.GetType ().FullName})' in {indexName} index");

				string prefix = TypeToPrefix (type);
				Logger.Warning($"Duplicate type name '{typeName}' in {indexName} index. Existing entry is of type {t.GetType ().FullName}, new entry of type {type.GetType ().FullName}");
				Logger.Debug ($"Prefixing new type entry with '{prefix}:'");
				// This is necessary because Xamarin.Android adds invoker classes for deprecated
				// interfaces and, initially, the class has exactly the same Java name as the interface
				// and only later in the process the interface name is prefixed with 'I' creating a
				// unique managed type name
				typeName = $"{prefix}:{typeName}";
			}

			index [typeName] = type;
		}

		public HierarchyElement Lookup (string typeName, string kindHint = null, HierarchyElement context = null)
		{
			HierarchyElement ret = Lookup (nativeIndex, "native", typeName, kindHint, context);

			if (ret == null)
				ret = Lookup (managedIndex, "managed", typeName, kindHint, context);

			if (ret == null)
				throw new InvalidOperationException ($"Type '{typeName}' not found in index.{GetContextLocation (context)}");

			return ret;
		}

		HierarchyElement Lookup (Dictionary <string, HierarchyElement> index, string indexName, string typeName, string kindHint, HierarchyElement context)
		{
			if (!index.TryGetValue (typeName, out HierarchyElement ret)) {
				string caller = GetContextLocation (context);

				Logger.Debug ($"Type '{typeName}' not found in {indexName} index.{caller}");
				if (!String.IsNullOrEmpty (kindHint)) {
					typeName = $"{kindHint}:{typeName}";
					Logger.Debug ($"Trying to look up using prefixed name: '{typeName}'.{caller}");
					if (!index.TryGetValue (typeName, out ret))
						Logger.Debug ($"Prefixed type name not found in {indexName} index either.{caller}");
				}
			}

			return ret;
		}

		static string GetContextLocation (HierarchyElement context)
		{
			if (context == null)
				return String.Empty;

			string ret = $" Called on behalf of: {context.FullName}";
			string location = context.GetLocation ();
			if (!String.IsNullOrEmpty (location))
				ret += $" at {location}";

			return ret;
		}

		public static string TypeToPrefix <T> () where T: HierarchyBase
		{
			return TypeToPrefix (typeof (T));
		}

		static string TypeToPrefix (HierarchyBase element)
		{
			if (element == null)
				return "[null]";

			return TypeToPrefix (element.GetType ());
		}

		static string TypeToPrefix (Type type)
		{
			if (type == typeof (HierarchyNamespace))
				return "namespace";
			else if (type == typeof (HierarchyClass))
				return "class";
			else if (type == typeof (HierarchyInterfaceInvoker))
				return "invoker";
			else if (type == typeof (HierarchyTypeMember))
				return "typeMember";
			else if (type == typeof (HierarchyImplements))
				return "implements";
			else if (type == typeof (HierarchyMethod))
				return "method";
			else if (type == typeof (HierarchyConstructor))
				return "constructor";
			else if (type == typeof (HierarchyException))
				return "exception";
			else if (type == typeof (HierarchyTypeParameter))
				return "typeParameter";
			else if (type == typeof (HierarchyTypeParameterGenericConstraint))
				return "typeParameterGenericConstraint";
			else if (type == typeof (HierarchyMethodParameter))
				return "methodParameter";
			else if (type == typeof (HierarchyField))
				return "field";
			else if (type == typeof (HierarchyInterface))
				return "interface";
			else if (type == typeof (HierarchyEnum))
				return "enum";
			else
				return type.FullName.Replace (".", String.Empty);
		}
	}
}
