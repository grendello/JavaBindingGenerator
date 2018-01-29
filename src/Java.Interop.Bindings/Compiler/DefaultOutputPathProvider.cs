//
// OutputPathProvider.cs
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
	public class DefaultOutputPathProvider : OutputPathProvider
	{
		public OutputTreeLayout TreeLayout { get; }

		public DefaultOutputPathProvider (GeneratorContext context, OutputTreeLayout treeLayout) : base (context)
		{
			TreeLayout = treeLayout ?? throw new ArgumentNullException (nameof (treeLayout));
		}

		public override void Validate ()
		{
			switch (TreeLayout.NestedTypesStyle) {
				case OutputNestedTypesStyle.SeparateFileDot:
				case OutputNestedTypesStyle.SeparateFileUnderscore:
					if (!Context.UsePartialClasses)
						throw new InvalidOperationException ("Nested types in separate files require partial classes");
					break;
			}
		}

		public override FilesystemPath GetPathFor (string rootDirectory, HierarchyElement element)
		{
			if (String.IsNullOrEmpty (rootDirectory))
				throw new ArgumentException ("Must not be null or empty", nameof (rootDirectory));

			if (element == null)
				throw new ArgumentNullException (nameof (element));

			switch (element) {
				case HierarchyNamespace ns:
				case HierarchyClass klass:
				case HierarchyInterface iface:
				case HierarchyEnum enm:
					break;

				default:
					throw new InvalidOperationException ($"Unsupported hierarchy type '{element.GetType ()}'");
			}

			Func<string, HierarchyElement, FilesystemPath> getter = null;
			switch (TreeLayout.NamespaceTreeStyle) {
				case OutputNamespaceTreeStyle.Single:
					getter = GetPathFor_Single;
					break;

				case OutputNamespaceTreeStyle.Shallow:
					getter = GetPathFor_Shallow;
					break;

				case OutputNamespaceTreeStyle.Deep:
					getter = GetPathFor_Deep;
					break;

				case OutputNamespaceTreeStyle.FirstLevelThenFullShallow:
					getter = GetPathFor_FirstLevelThenFullShallow;
					break;

				case OutputNamespaceTreeStyle.FirstLevelThenFullSingle:
					getter = GetPathFor_FirstLevelThenFullSingle;
					break;

				case OutputNamespaceTreeStyle.FirstLevelThenShortShallow:
					getter = GetPathFor_FirstLevelThenShortShallow;
					break;

				case OutputNamespaceTreeStyle.FirstLevelThenShortSingle:
					getter = GetPathFor_FirstLevelThenShortSingle;
					break;

				default:
					throw new InvalidOperationException ($"Unsupported namespace tree style {TreeLayout.NamespaceTreeStyle}");
			}

			return getter (rootDirectory, element);
		}

		protected FilesystemPath GetPathFor_Single (string rootDirectory, HierarchyElement element)
		{
			if (element is HierarchyNamespace)
				return null;

			throw new NotImplementedException ();
		}

		protected FilesystemPath GetPathFor_Shallow (string rootDirectory, HierarchyElement element)
		{
			throw new NotImplementedException ();
		}

		protected FilesystemPath GetPathFor_Deep (string rootDirectory, HierarchyElement element)
		{
			throw new NotImplementedException ();
		}

		protected FilesystemPath GetPathFor_FirstLevelThenFullShallow (string rootDirectory, HierarchyElement element)
		{
			string fullManagedName = element.GetManagedName (true);
			bool isDirectory;
			string path = GetFirstNameSegment (fullManagedName);

			Logger.Debug ($"Element: {element.FullName} (managed: {fullManagedName})");
			Logger.Debug ($"Initial path: {path}");
			if (element is HierarchyNamespace) {
				Logger.Debug ("Is namespace");
				isDirectory = true;
				path = Path.Combine (path, fullManagedName);
			} else {
				var obj = element as HierarchyObject;
				if (obj == null)
					throw new InvalidOperationException ("Only HierarchyObject and HierarchyNamespace instances can be used to compute output file name");
				Logger.Debug ("Is type");
				isDirectory = false;
				path = Path.Combine (path, obj.GetNamespace (), obj.GetNameWithoutNamespace ());
			}
			Logger.Debug ($"Final relative path: {path}");
			Logger.Debug (String.Empty);
			return GetFilePath (rootDirectory, path, isDirectory);
		}

		protected FilesystemPath GetPathFor_FirstLevelThenShortShallow (string rootDirectory, HierarchyElement element)
		{
			throw new NotImplementedException ();
		}

		protected FilesystemPath GetPathFor_FirstLevelThenFullSingle (string rootDirectory, HierarchyElement element)
		{
			throw new NotImplementedException ();
		}

		protected FilesystemPath GetPathFor_FirstLevelThenShortSingle (string rootDirectory, HierarchyElement element)
		{
			throw new NotImplementedException ();
		}

		protected string MakePath (string rootDirectory, string elementName, string extension = null)
		{
			string ret = Path.Combine (rootDirectory, elementName);
			if (!String.IsNullOrEmpty (extension))
				ret += $".{extension}";

			return ret;
		}

		protected FilesystemPath GetFilePath (string rootDirectory, string fileName, bool isDirectory = false)
		{
			return new FilesystemPath (MakePath (rootDirectory, fileName, isDirectory ? null : Context.CodeGenerator.FileExtension), isDirectory);
		}

		string GetLastNameSegment (string fullManagedName)
		{
			return GetNameSegment (fullManagedName, false);
		}

		string GetFirstNameSegment (string fullManagedName)
		{
			return GetNameSegment (fullManagedName, true);
		}

		string GetNameSegment (string fullManagedName, bool first)
		{
			if (String.IsNullOrEmpty (fullManagedName))
				throw new ArgumentException ("must not be null or empty", nameof (fullManagedName));

			int dot = first ? fullManagedName.IndexOf ('.') : fullManagedName.LastIndexOf ('.');
			if (dot == 0)
				throw new InvalidOperationException ($"Full valid managed type name (including namespace) expected, got '{fullManagedName}'");

			if (dot < 0)
				return fullManagedName;

			return first ? fullManagedName.Substring (0, dot) : fullManagedName.Substring (dot + 1);
		}
	}
}
