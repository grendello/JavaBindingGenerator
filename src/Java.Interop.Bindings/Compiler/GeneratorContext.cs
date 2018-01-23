//
// GeneratorContext.cs
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
using System.Text;

using Java.Interop.Bindings.Compiler.Xamarin;

namespace Java.Interop.Bindings.Compiler
{
	public class GeneratorContext
	{
		NameTranslationProvider nameTranslationProvider;
		OutputPathProvider outputPathProvider;
		FormattingCodeGenerator codeGenerator;
		Encoding fileEncoding;
		FormattingContext formattingContext;
		Hierarchy hierarchyBuilder;

		public NameTranslationProvider NameTranslationProvider {
			get => nameTranslationProvider;
			set => nameTranslationProvider = EnsureNotNull ("NameTranslationProvider", value);
		}

		public OutputPathProvider OutputPathProvider {
			get => outputPathProvider;
			set => outputPathProvider = EnsureNotNull ("OutputPathProvider", value);
		}

		public FormattingCodeGenerator CodeGenerator {
			get => codeGenerator;
			set => codeGenerator = EnsureNotNull ("CodeGenerator", value);
		}

		public Encoding FileEncoding {
			get => fileEncoding;
			set => fileEncoding = EnsureNotNull ("FileEncoding", value);
		}

		public Hierarchy HierarchyBuilder {
			get => hierarchyBuilder;
			set => hierarchyBuilder = EnsureNotNull ("HierarchyBuilder", value);
		}

		public string FileHeaderComment { get; set; }
		public bool DumpHierarchy { get; set; }
		public string HierarchyDumpFilePath { get; set; }

		public GeneratorContext (FormattingCodeGenerator codeGenerator, Encoding fileEncoding = null)
		{
			CodeGenerator = codeGenerator ?? throw new ArgumentNullException (nameof (codeGenerator));
			OutputPathProvider = new DefaultOutputPathProvider (this);
			FileEncoding = fileEncoding ?? Encoding.UTF8;
			HierarchyBuilder = new XamarinAndroidHierarchy ();
		}

		internal void AssertSaneEnvironment ()
		{
			/*
			if (NameTranslationProvider == null)
				throw new InvalidOperationException ("Name translation provider not defined");
				*/

			if (OutputPathProvider == null)
				throw new InvalidOperationException ("Output path provider not defined");

			if (CodeGenerator == null)
				throw new InvalidOperationException ("Code generator not defined");

			if (FileEncoding == null)
				throw new InvalidOperationException ("File encoding not defined");

			if (HierarchyBuilder == null)
				throw new InvalidOperationException ("Hierarchy builder not defined");
		}

		T EnsureNotNull<T> (string name, T value) where T: class
		{
			if (value == null)
				throw new InvalidOperationException ($"{name} must not be null");
			return value;
		}
	}
}
