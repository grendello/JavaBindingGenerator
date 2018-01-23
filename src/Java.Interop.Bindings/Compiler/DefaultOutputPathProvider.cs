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
		public DefaultOutputPathProvider (GeneratorContext context) : base (context)
		{}

		public override FilesystemPath GetPathFor (string rootDirectory, HierarchyElement element)
		{
			if (String.IsNullOrEmpty (rootDirectory))
				throw new ArgumentException ("Must not be null or empty", nameof (rootDirectory));

			if (element == null)
				throw new ArgumentNullException (nameof (element));

			FilesystemPath relativePath = null;
			switch (element) {
				case HierarchyNamespace ns:
					relativePath = new FilesystemPath (MakePath (ns.GetManagedName (true)), true);
					break;

				case HierarchyClass klass:
					relativePath = GetFilePath (klass);
					break;

				case HierarchyInterface iface:
					relativePath = GetFilePath (iface);
					break;

				default:
					throw new InvalidOperationException ($"Unsupported hierarchy type '{element.GetType ()}'");
			}

			return relativePath;

			string MakePath (string elementName, string extension = null)
			{
				string relPath = elementName?.Replace ('.', Path.DirectorySeparatorChar);
				if (relPath == null)
					return null;

				string ret = Path.Combine (rootDirectory, relPath);
				if (!String.IsNullOrEmpty (extension))
					ret += $".{extension}";

				return ret;
			}

			FilesystemPath GetFilePath (HierarchyElement e)
			{
				return new FilesystemPath (MakePath (e.GetManagedName (true), Context.CodeGenerator.FileExtension), false);
			}
		}
	}
}
