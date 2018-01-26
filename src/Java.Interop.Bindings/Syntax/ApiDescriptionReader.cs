//
// ApiDescriptionReader.cs
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
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Java.Interop.Bindings.Syntax
{
	public class ApiDescriptionReader : IDisposable
	{
		readonly InputDocument apiDoc;
		readonly List<InputDocument> fixupDocs;

		bool disposedValue;

		public bool DumpFixedUp { get; set; }

		public ApiDescriptionReader (string apiInput, List<string> fixupInputs = null) : this (new InputDocument (apiInput), OpenFixups (fixupInputs))
		{}

		public ApiDescriptionReader (InputDocument apiInput, List<InputDocument> fixupInputs = null)
		{
			apiDoc = apiInput ?? throw new ArgumentNullException (nameof (apiInput));
			if (fixupInputs != null && fixupInputs.Count > 0)
				fixupDocs = fixupInputs;
		}

		static List<InputDocument> OpenFixups (List<string> fixupInputs)
		{
			if (fixupInputs == null || fixupInputs.Count == 0)
				return null;

			var ret = new List<InputDocument> ();
			foreach (string path in fixupInputs) {
				if (String.IsNullOrEmpty (path))
					continue;
				if (!File.Exists (path))
					throw new InvalidOperationException ($"API fixup file '{path}' does not exist");
				ret.Add (new InputDocument (path));
			}

			return ret.Count > 0 ? ret : null;
		}

		public IList<ApiElement> Parse ()
		{
			var doc = XDocument.Load (apiDoc.Stream, LoadOptions.SetLineInfo);
			ApplyFixups (doc);

			var elements = new List<ApiElement> ();
			foreach (XElement element in doc.Root.Elements ()) {
				if (element.Is ("package")) {
					elements.Add (ParseNamespace (element));
					continue;
				}

				if (element.Is ("enum")) {
					elements.Add (ParseEnum (element));
					continue;
				}

				Logger.Warning ($"Unknown element '{element.Name}' at {element.GetLineInfo ()}");
			}

			return elements;
		}

		protected virtual ApiNameSpace ParseNamespace (XElement element)
		{
			var ns = new ApiNameSpace (apiDoc.Path);
			ns.Parse (element);
			return ns;
		}

		protected virtual ApiEnum ParseEnum (XElement element)
		{
			var enm = new ApiEnum (apiDoc.Path);
			enm.Parse (element);
			return enm;
		}

		void ApplyFixups (XDocument doc)
		{
			if (fixupDocs == null)
				return;

			foreach (InputDocument fixup in fixupDocs)
				ApplyFixups (doc, fixup);

			if (!DumpFixedUp)
				return;

			bool haveApiPath = !String.IsNullOrEmpty (apiDoc.Path);
			string fixedPath = haveApiPath ? apiDoc.Path + ".fixedup" : "standard output";
			Logger.Info ($"Dumping fixed-up API description file to {fixedPath}");

			var writerSettings = new XmlWriterSettings {
				Encoding = Encoding.UTF8,
				Indent = true,
				IndentChars = "\t",
				NamespaceHandling = NamespaceHandling.OmitDuplicates,
				NewLineHandling = NewLineHandling.None,
				NewLineOnAttributes = true,
				WriteEndDocumentOnClose = true
			};

			XmlWriter writer = null;
			try {
				if (haveApiPath)
					writer = XmlWriter.Create (fixedPath, writerSettings);
				else
					writer = XmlWriter.Create (Console.Out, writerSettings);
				doc.Save (writer);
			} finally {
				writer?.Dispose ();
			}
		}

		protected virtual void ApplyFixups (XDocument doc, InputDocument fixup)
		{
			if (fixup == null)
				return;

			var applier = CreateFixupApplier (doc, XDocument.Load (fixup.Stream), fixup.Path);
			if (applier == null)
				throw new InvalidOperationException ("No fixup applier instance");
			applier.Apply ();
		}

		protected virtual ApiFixupApplier CreateFixupApplier (XDocument doc, XDocument fixups, string fixupsPath) => new ApiFixupApplier (doc, fixups, fixupsPath);

		void Dispose (bool disposing)
		{
			if (!disposedValue) {
				if (disposing) {
					apiDoc?.Dispose ();
					if (fixupDocs != null) {
						foreach (InputDocument doc in fixupDocs)
							doc?.Dispose ();
					}
				}

				disposedValue = true;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
		}
	}
}
