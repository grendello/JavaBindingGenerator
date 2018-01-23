//
// CodeGenerator.cs
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

using BindingGenerator.Core.Parsing;

namespace BindingGenerator.Core.Generation
{
	public class Generator
	{
		public GeneratorContext Context { get; }
		public bool OverwriteFiles { get; }

		public Generator (GeneratorContext context, bool overwriteFiles)
		{
			Context = context ?? throw new ArgumentNullException (nameof (context));
			OverwriteFiles = overwriteFiles;
		}

		public void Generate (string outputDirectoryRoot, IList<ApiElement> rawElements)
		{
			if (String.IsNullOrEmpty (outputDirectoryRoot))
				throw new ArgumentException ("must not be null or empty", nameof (outputDirectoryRoot));
			if (rawElements == null || rawElements.Count == 0)
				throw new ArgumentException ("must be a non-empty collection", nameof (rawElements));

			Context.AssertSaneEnvironment ();

			Logger.Verbose ("Instantiating hierarchy");
			Hierarchy hierarchy = Context.HierarchyBuilder;
			Logger.Verbose ($"Hierarchy instantiated, type {hierarchy.GetType ()}");
			Logger.Info ("Building hierarchy");
			hierarchy.Build (rawElements);
			if (Context.DumpHierarchy)
				hierarchy.Dump (Context.HierarchyDumpFilePath);
			Logger.Info ("Generating sources from hierarchy");
			Helpers.ForEachNotNull (hierarchy.Namespaces, (ns) => GenerateNamespace (outputDirectoryRoot, ns));
		}

		protected virtual void GenerateNamespace (string outputDirectoryRoot, HierarchyNamespace ns)
		{
			FilesystemPath path = Context.OutputPathProvider.GetPathFor (outputDirectoryRoot, ns);
			if (String.IsNullOrEmpty (path?.FullPath))
				return;

			Stream nsFile = null;
			StreamWriter nsFileWriter = null;
			try {
				string targetKind;
				if (path.IsDirectory) {
					targetKind = "directory";
					EnsureDirectory (path.FullPath);
				} else {
					targetKind = "file";
					EnsureDirectory (Path.GetDirectoryName (path.FullPath));
					CheckOverwrite (path.FullPath);
					nsFile = File.Open (path.FullPath, FileMode.Create);
					nsFileWriter = new StreamWriter (nsFile, Context.FileEncoding);
				}
				Logger.Debug ($"Creating {targetKind} for namespace {ns.GetManagedName (true)}: {path.FullPath}");
				Helpers.ForEachNotNull (
					ns.Members,
					(HierarchyElement nsm) => {
						if (nsFileWriter != null) {
							WriteFileHeader (ns, nsFileWriter);
							GenerateNamespaceMember (nsm, nsFileWriter, path.FullPath);
							WriteFileHeader (ns, nsFileWriter);
						} else
							GenerateNamespaceMember (ns, nsm, path.FullPath);
					}
				);
			} finally {
				if (nsFileWriter != null) {
					nsFileWriter.Dispose ();
					nsFileWriter = null;
				}

				if (nsFile != null) {
					nsFile.Dispose ();
					nsFile = null;
				}
			}
		}

		protected virtual void GenerateNamespaceMember (HierarchyElement element, StreamWriter writer, string outputFileName)
		{
			Logger.Debug ($"Generating {element.GetManagedName (true)} in namespace output file: {outputFileName}");
			OutputNamespaceMember (element, writer, outputFileName);
		}

		protected virtual void GenerateNamespaceMember (HierarchyNamespace ns, HierarchyElement element, string outputDirectory)
		{
			FilesystemPath path = Context.OutputPathProvider.GetPathFor (outputDirectory, element);
			if (String.IsNullOrEmpty (path?.FullPath)) {
				Logger.Warning ($"Unable to generate output for element {element.GetManagedName (true)} since no full path was given");
				return;
			}

			if (path.IsDirectory)
				throw new InvalidOperationException ($"Namespace member must be written to a file ({ns.FullName})");

			EnsureDirectory (Path.GetDirectoryName (path.FullPath));
			CheckOverwrite (path.FullPath);

			using (Stream fs = File.Open (path.FullPath, FileMode.Create)) {
				using (StreamWriter writer = new StreamWriter (fs, Context.FileEncoding)) {
					WriteFileHeader (ns, writer);
					OutputNamespaceMember (element, writer, path.FullPath);
					WriteFileFooter (ns, writer);
				}
			}
		}

		protected virtual void OutputNamespaceMember (HierarchyElement element, StreamWriter writer, string fileName)
		{
		}

		protected virtual void WriteFileHeader (HierarchyNamespace ns, StreamWriter writer)
		{
		}

		protected virtual void WriteFileFooter (HierarchyNamespace ns, StreamWriter writer)
		{
		}

		void CheckOverwrite (string path)
		{
			if (OverwriteFiles || String.IsNullOrEmpty (path))
				return;

			if (File.Exists (path))
				throw new InvalidOperationException ($"File {path} already exists");
		}

		void EnsureDirectory (string path)
		{
			if (String.IsNullOrEmpty (path) || Directory.Exists (path))
				return;
			Directory.CreateDirectory (path);
		}
	}
}
