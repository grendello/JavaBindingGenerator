//
// GeneratorOptions.cs
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
using System.Text;

using BindingGenerator.Core.Generation;

namespace BindingGenerator
{
	public class GeneratorOptions
	{
		public string ApiDescriptionFilePath { get; set; }
		public List<string> ApiFixupFiles { get; } = new List<string> ();
		public bool DumpFixedUp { get; set; }
		public bool DumpHierarchy { get; set; }
		public string OutputPath { get; set; } = "generated";
		public bool OverwriteFiles { get; set; }
		public Encoding FileEncoding { get; set; } = Encoding.UTF8;
		public string CodeGenerator { get; set; }
		public string CodeStyle { get; set; }
	}
}
