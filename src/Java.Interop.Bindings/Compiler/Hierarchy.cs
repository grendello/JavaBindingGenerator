//
// Hierarchy.cs
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
using System.IO;
using System.Linq;
using System.Text;

using Java.Interop.Bindings.Syntax;

namespace Java.Interop.Bindings.Compiler
{
	public class Hierarchy : HierarchyBase
	{
		// These two must match the Java type name, in general, from api.xml as that's what gets placed in the
		// type index. However, the runtime namespace is not found in api.xml and thus we use the managed name
		// for it
		const string DefaultInterfaceBaseTypeNamespace = "Android.Runtime";
		const string DefaultClassBaseTypeNamespace = "java.lang";

		public const string DefaultInterfaceBaseType = DefaultInterfaceBaseTypeNamespace + ".IJavaObject";
		public const string DefaultClassBaseType = DefaultClassBaseTypeNamespace + ".Object";

		List<HierarchyNamespace> namespaces;
		List<HierarchyEnum> enums;
		Dictionary<string, List<HierarchyElement>> typeNameDependants;

		public IList<HierarchyNamespace> Namespaces => namespaces;
		protected HierarchyIndex TypeIndex { get; } = new HierarchyIndex ();

		public Hierarchy (GeneratorContext context) : base (context)
		{}

		public void Build (IList<ApiElement> rawElements)
		{ 
			if (rawElements == null || rawElements.Count == 0)
				throw new ArgumentException("must be a non-empty collection", nameof(rawElements));

			// Pass 1: build basic hierarchy (no class or interface nesting yet)
			Helpers.ForEachNotNull (rawElements, (ApiElement e) => {
				switch (e) {
					case ApiNameSpace ns:
						Process (ns);
						break;

					case ApiEnum enm:
						Process (enm);
						break;

					default:
						Logger.Warning ($"Unsupported top-level element type: {e.GetType ().FullName}");
						break;
				}
			});

			HierarchyNamespace androidRuntime = namespaces?.Where (ns => String.Compare (DefaultInterfaceBaseTypeNamespace, ns?.FullName, StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault ();
			if (androidRuntime == null) {
				Logger.Verbose ("Creating Android.Runtime namespace (not found after parsing API description)");
				androidRuntime = CreateHierarchyElementInternal <HierarchyNamespace> (this);
				androidRuntime.Init ();
				androidRuntime.IgnoreForCodeGeneration = true;
				androidRuntime.FullName = DefaultInterfaceBaseTypeNamespace;
				androidRuntime.Name = Helpers.GetBaseTypeName (androidRuntime.FullName);
				androidRuntime.ManagedName = androidRuntime.Name;
				androidRuntime.FullManagedName = androidRuntime.FullName;
				Helpers.AddToList(androidRuntime, ref namespaces);
			}

			// IJavaObject is defined in Xamarin.Android's non-generated source and implmented by all
			// interfaces that don't derive from other interfaces
			HierarchyInterface iJavaLangObject = androidRuntime.Members?.OfType <HierarchyInterface> ().Where (iface => String.Compare (DefaultInterfaceBaseType, iface?.FullName, StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault ();
			if (iJavaLangObject == null) {
				Logger.Verbose ("Synthesizing Android.Runtime.IJavaObject interface (not found after parsing API description)");
				iJavaLangObject = CreateHierarchyElementInternal <HierarchyInterface> (androidRuntime);
				iJavaLangObject.Init ();
				iJavaLangObject.FullName = DefaultInterfaceBaseType;
				iJavaLangObject.Name = Helpers.GetBaseTypeName (iJavaLangObject.FullName);
				iJavaLangObject.IgnoreForCodeGeneration = true;
				iJavaLangObject.InvokerNotNeeded = true;
				iJavaLangObject.UseGlobal = true;
				TypeIndex.Add (iJavaLangObject);
			}

			// Pass 2: nest classes, interfaces
			NestNamespaces ();

			// Pass 3: generate managed names
			Helpers.ForEachNotNull (namespaces, (HierarchyNamespace ns) => GenerateManagedNames (ns));

			// TODO: fix generation of full managed names for enums
			Helpers.ForEachNotNull (enums, (HierarchyEnum enm) => GenerateManagedNames (enm));

			// Pass 4: nest enums (they need managed names)
			NestEnums ();

			// Pass 5: generate and inject synthetic elements
			SynthesizeElements ();

			// Pass 6: resolve base types since we have everything in place now
			ResolveBaseTypes ();

			// Pass 7: sort class members
		}

		protected void GenerateManagedNames (HierarchyElement root)
		{
			if (root == null)
				return;

			root.SetManagedNames ();
			if (root is HierarchyObject || root is HierarchyNamespace)
			 	TypeIndex.AddManaged (root);
			if (!root.HasMembers)
				return;

			Helpers.ForEachNotNull(root.Members, (HierarchyElement element) => GenerateManagedNames (element));
		}

		protected void ResolveBaseTypes ()
		{
			if (namespaces == null || namespaces.Count == 0)
				return;

			foreach (HierarchyNamespace ns in namespaces) {
				if (ns == null || ns.Members == null || ns.Members.Count == 0)
					continue;

				foreach (HierarchyObject type in ns.Members.OfType<HierarchyObject> ().Where (o => o != null)) {
					type.ResolveBaseTypes (TypeIndex);
				}
			}
		}

		protected virtual void SynthesizeElements ()
		{}

		void NestElements <TElement, TParent> (IList<TElement> elements, Dictionary <HierarchyElement, HierarchyElement> toReparent)
			where TElement: HierarchyElement
			where TParent: HierarchyElement
		{
			if (elements == null)
				return;

			foreach (HierarchyElement element in elements) {
				TParent newParent = SelectNewParent <TParent> (element);
				if (newParent == null)
					continue;

				if (toReparent.ContainsKey (element))
					Logger.Warning ($"Element {element.Name} ({element.GetType ()} was already re-parented");

				toReparent [element] = newParent;
			}
		}

		protected void NestElements (Action<Dictionary <HierarchyElement, HierarchyElement>> looper)
		{
			if (looper == null)
				throw new ArgumentNullException (nameof (looper));

			var toReparent = new Dictionary <HierarchyElement, HierarchyElement> ();
			looper (toReparent);

			if (toReparent.Count == 0)
				return;

			foreach (var kvp in toReparent) {
				HierarchyElement element = kvp.Key;
				HierarchyElement newParent = kvp.Value;

				element.ParentElement?.RemoveMember (element);
				newParent.AddMember (element);
			}
		}

		protected void NestNamespaces ()
		{
			NestElements (
				toReparent => {
					foreach (HierarchyNamespace ns in namespaces) {
						NestElements <HierarchyElement, HierarchyClass> (ns?.Members, toReparent);
					}
				}
			);
		}

		protected void NestEnums ()
		{
			NestElements (toReparent => NestElements <HierarchyEnum, HierarchyNamespace> (enums, toReparent));
		}

		protected virtual T SelectNewParent <T> (HierarchyElement element) where T: HierarchyElement
		{
			if (element == null)
				return null;

			if (String.IsNullOrEmpty (element.FullName))
				throw new InvalidOperationException ("Each element must have a full name");

			if (TypeIndex.Count == 0) {
				Logger.Warning ($"Unable to select new parent for element {element.Name}, type index does not exist");
				return null;
			}

			// The way API description is constructed we can check whether a class/interface/enum is nested
			// by simply looking at its "namespace" name and checking whether it corresponds to another
			// class or interfaces
			string nsOrTypeName = Helpers.GetTypeNamespace (element.FullName);
			if (String.IsNullOrEmpty (nsOrTypeName))
				return null;

			try {
				HierarchyElement maybeParent = TypeIndex.Lookup (nsOrTypeName);
				if (maybeParent == null)
					return null;

				var ret = maybeParent as T;
				if (ret == null)
					return null;

				return ret;
			} catch {
				Logger.Fatal ($"Error selecting new parent for element '{element.FullName}' ({element.GetLocation ()})");
				throw;
			}
		}

		public void Dump (string outputFile)
		{
			if (String.IsNullOrEmpty (outputFile) || namespaces == null || namespaces.Count == 0)
				return;

			string indent = String.Empty;
			using (var sw = new StreamWriter (outputFile, false, Encoding.UTF8)) {
				foreach (HierarchyNamespace ns in namespaces) {
					sw.WriteLine ($"Namespace: [native: {ns.Name} ({ns.FullName})] [managed: {ns.ManagedName} ({ns.FullManagedName})]");
					Dump (sw, indent + "\t", ns.Members);
				}
			}
		}

		void Dump (StreamWriter sw, string indent, IList<HierarchyElement> members)
		{
			if (members == null || members.Count == 0)
				return;

			foreach (HierarchyElement element in members) {
				sw.WriteLine ($"{indent}{TypeToName (element)}: [native: {element.Name} ({element.FullName})] [managed: {element.ManagedName} ({element.FullManagedName})]");
				Dump (sw, indent + "\t", element.Members);
			}
		}

		string TypeToName (HierarchyBase element)
		{
			if (element == null)
				return "[null]";

			Type type = element.GetType ();
			if (type == typeof (HierarchyNamespace))
				return "Namespace";
			else if (type == typeof (HierarchyClass))
				return "Class";
			else if (type == typeof (HierarchyInterfaceInvoker))
				return "Class (invoker)";
			else if (type == typeof (HierarchyTypeMember))
				return "Type Member";
			else if (type == typeof (HierarchyImplements))
				return "Implements";
			else if (type == typeof (HierarchyMethod))
				return "Method";
			else if (type == typeof (HierarchyConstructor))
				return "Constructor";
			else if (type == typeof (HierarchyException))
				return "Exception";
			else if (type == typeof (HierarchyTypeParameter))
				return "Type Parameter";
			else if (type == typeof (HierarchyTypeParameterGenericConstraint))
				return "Type Parameter Generic Constraint";
			else if (type == typeof (HierarchyMethodParameter))
				return "Method Parameter";
			else if (type == typeof (HierarchyField))
				return "Field";
			else if (type == typeof (HierarchyInterface))
				return "Interface";
			else if (type == typeof (HierarchyEnum))
				return "Enum";
			else
				return element.GetType ().ToString ();
		}

		protected virtual void Process (ApiNameSpace apiNameSpace)
		{
			var ns = CreateHierarchyElementInternal <HierarchyNamespace> (this);
			ns.Init (apiNameSpace);
			Helpers.ForEachNotNull (apiNameSpace.ChildElements, (ApiElement e) => {
				switch (e) {
					case ApiClass klass:
						Process (ns, klass);
						break;

					case ApiInterface iface:
						Process (ns, iface);
						break;

					default:
						Logger.Warning ($"Unsupported element type in namespace {apiNameSpace.Name}: {e.GetType ().FullName}");
						break;
				}
			});

			Helpers.AddToList (ns, ref namespaces);
			TypeIndex.Add (ns);
		}

		void AddLocationComment (ApiElement element, HierarchyElement hierarchyElement)
		{
			if (String.IsNullOrEmpty (element.DocumentPath) || element.SourceLine < 0)
				return;

			var sb = new StringBuilder ();
			sb.Append (element.DocumentPath);
			if (element.SourceColumn > 0)
				sb.Append ($" [{element.SourceLine}:{element.SourceColumn}]");
			else
				sb.Append ($":{element.SourceLine}");
			hierarchyElement.AddComment( sb.ToString ());
		}

		protected virtual void Process (HierarchyNamespace parent, ApiClass klass)
		{
			var hierarchyClass = CreateHierarchyElementInternal <HierarchyClass> (parent);
			hierarchyClass.Init (klass);

			AddLocationComment (klass, hierarchyClass);
			Helpers.ForEachNotNull (klass.ChildElements, (ApiElement e) => {
				switch (e) {
					case ApiField field:
						Process (hierarchyClass, field);
						break;

					case ApiConstructor constructor:
						Process (hierarchyClass, constructor);
						break;

					case ApiMethod method:
						Process (hierarchyClass, method);
						break;

					case ApiImplements implements:
						Process (hierarchyClass, implements);
						break;

					case ApiTypeParameter typeParameter:
						Process (hierarchyClass, typeParameter);
						break;

					default:
						Logger.Warning ($"Unexpected member type for ApiClass: '{e.GetType ().FullName}'");
						break;
				}
			});

			parent.AddMember (hierarchyClass);
			TypeIndex.Add (hierarchyClass);
		}

		protected virtual void Process (HierarchyObject parent, ApiTypeParameter typeParameter)
		{
			var hierarchyTypeParameter = CreateHierarchyElementInternal <HierarchyTypeParameter> (parent);
			hierarchyTypeParameter.Init (typeParameter);
			AddTypeParameterGenericConstraints (hierarchyTypeParameter, typeParameter);
			parent.AddMember (hierarchyTypeParameter);
		}

		protected void AddTypeParameterGenericConstraints (HierarchyTypeParameter parameter, ApiTypeParameter typeParameter)
		{
			Helpers.ForEachNotNull (typeParameter.ChildElements, (ApiElement e) => {
				switch (e) {
					case ApiTypeParameterGenericConstraint constraint:
						Process (parameter, constraint);
						break;

					default:
						Logger.Warning ($"Unexpected member type for ApiTypeParameter: '{e.GetType ().FullName}'");
						break;
				}
			});
		}

		protected virtual void Process (HierarchyTypeParameter parent, ApiTypeParameterGenericConstraint constraint)
		{
			var hierarchyTypeParameterGenericConstraint = CreateHierarchyElementInternal <HierarchyTypeParameterGenericConstraint> (parent);
			hierarchyTypeParameterGenericConstraint.Init (constraint);
			parent.AddMember (hierarchyTypeParameterGenericConstraint);
		}

		protected virtual void Process (HierarchyObject parent, ApiImplements implements)
		{
			var hierarchyImplements = CreateHierarchyElementInternal <HierarchyImplements> (parent);
			hierarchyImplements.Init (implements);
			parent.AddMember (hierarchyImplements);
		}

		protected virtual void Process (HierarchyObject parent, ApiMethod method)
		{
			var hierarchyMethod = CreateHierarchyElementInternal <HierarchyMethod> (parent);
			hierarchyMethod.Init (method);
			AddLocationComment (method, hierarchyMethod);
			AddMethodMembers (hierarchyMethod, method);
			parent.AddMember (hierarchyMethod);
		}

		protected virtual void Process (HierarchyObject parent, ApiConstructor constructor)
		{
			var hierarchyConstructor = CreateHierarchyElementInternal <HierarchyConstructor> (parent);
			hierarchyConstructor.Init (constructor);
			AddLocationComment (constructor, hierarchyConstructor);
			AddMethodMembers (hierarchyConstructor, constructor);
			parent.AddMember (hierarchyConstructor);
		}

		protected virtual void AddMethodMembers (HierarchyMethod method, ApiMethod apiMethod)
		{
			Helpers.ForEachNotNull (apiMethod.ChildElements, (ApiElement e) => {
				switch (e) {
					case ApiMethodParameter parameter:
						Process (method, parameter);
						break;

					case ApiTypeParameter typeParameter:
						Process (method, typeParameter);
						break;

					case ApiException exception:
						Process (method, exception);
						break;

					default:
						Logger.Warning ($"Unexpected member type for {apiMethod.GetType ()}: '{e.GetType ().FullName}'");
						break;
				}
			});
		}

		protected virtual void Process (HierarchyMethod parent, ApiException exception)
		{
			var hierarchyException = CreateHierarchyElementInternal <HierarchyException> (parent);
			hierarchyException.Init (exception);
			parent.AddMember (hierarchyException);
		}

		protected virtual void Process (HierarchyMethod parent, ApiTypeParameter typeParameter)
		{
			var hierarchyTypeParameter = CreateHierarchyElementInternal <HierarchyTypeParameter> (parent);
			hierarchyTypeParameter.Init (typeParameter);
			AddTypeParameterGenericConstraints (hierarchyTypeParameter, typeParameter);
			parent.AddMember (hierarchyTypeParameter);
		}

		protected virtual void Process (HierarchyMethod parent, ApiMethodParameter parameter)
		{
			var hierarchyMethodParameter = CreateHierarchyElementInternal <HierarchyMethodParameter> (parent);
			hierarchyMethodParameter.Init (parameter);
			parent.AddMember (hierarchyMethodParameter);
		}

		protected virtual void Process (HierarchyObject parent, ApiField field)
		{
			var hierarchyField = CreateHierarchyElementInternal <HierarchyField> (parent);
			hierarchyField.Init (field);
			AddLocationComment (field, hierarchyField);
			parent.AddMember (hierarchyField);
		}

		protected virtual void Process (HierarchyNamespace parent, ApiInterface iface)
		{
			var hierarchyInterface = CreateHierarchyElementInternal <HierarchyInterface> (parent);
			hierarchyInterface.Init (iface);

			AddLocationComment (iface, hierarchyInterface);
			Helpers.ForEachNotNull (iface.ChildElements, (ApiElement e) => {
				switch (e) {
					case ApiField field:
						Process (hierarchyInterface, field);
						break;

					case ApiMethod method:
						Process (hierarchyInterface, method);
						break;

					case ApiImplements implements:
						Process (hierarchyInterface, implements);
						break;

					case ApiTypeParameter typeParameter:
						Process (hierarchyInterface, typeParameter);
						break;

					default:
						Logger.Warning ($"Unexpected member type for ApiInterface: '{e.GetType ().FullName}'");
						break;
				}
			});

			parent.AddMember (hierarchyInterface);
			TypeIndex.Add (hierarchyInterface);
		}

		protected virtual void Process (ApiEnum apiEnum)
		{
			var enm = CreateHierarchyElementInternal <HierarchyEnum> (this);
			enm.Init (apiEnum);
			Helpers.AddToList (enm, ref enums);
			TypeIndex.Add (enm);
		}

		protected T CreateHierarchyElementInternal <T> (HierarchyBase parent) where T: HierarchyElement
		{
			T ret = CreateHierarchyElement <T> (parent);
			if (ret == null)
				throw new InvalidOperationException ($"Hierarchy element of type {typeof (T)} not created");
			return ret;
		}

		// A bit clumsy but better than full-blown reflection use or an open set of Create* methods for each
		// type we support now or in the future
		protected virtual T CreateHierarchyElement <T> (HierarchyBase parent) where T: HierarchyElement
		{
			Type type = typeof (T);
			HierarchyBase ret = null;

			if (type == typeof (HierarchyNamespace))
				ret = new HierarchyNamespace (Context, parent as Hierarchy);
			else if (type == typeof (HierarchyClass))
				ret = new HierarchyClass (Context, parent as HierarchyNamespace);
			else if (type == typeof (HierarchyImplements))
				ret = new HierarchyImplements (Context, parent as HierarchyObject);
			else if (type == typeof (HierarchyMethod))
				ret = new HierarchyMethod (Context, parent as HierarchyObject);
			else if (type == typeof (HierarchyConstructor))
				ret = new HierarchyConstructor (Context, parent as HierarchyObject);
			else if (type == typeof (HierarchyException))
				ret = new HierarchyException (Context, parent as HierarchyMethod);
			else if (type == typeof (HierarchyTypeParameter))
				ret = new HierarchyTypeParameter (Context, parent as HierarchyElement);
			else if (type == typeof (HierarchyTypeParameterGenericConstraint))
				ret = new HierarchyTypeParameterGenericConstraint (Context, parent as HierarchyTypeParameter);
			else if (type == typeof (HierarchyMethodParameter))
				ret = new HierarchyMethodParameter (Context, parent as HierarchyMethod);
			else if (type == typeof (HierarchyField))
				ret = new HierarchyField (Context, parent as HierarchyObject);
			else if (type == typeof (HierarchyInterface))
				ret = new HierarchyInterface (Context, parent as HierarchyNamespace);
			else if (type == typeof (HierarchyEnum))
				ret = new HierarchyEnum (Context, parent as Hierarchy);
			else
				throw new InvalidOperationException ($"Unsupported hierarchy element type {type}");

			return ret as T;
		}
	}
}
