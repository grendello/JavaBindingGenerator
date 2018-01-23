//
// ApiFixupApplier.cs
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
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace BindingGenerator.Core.Parsing
{
	public class ApiFixupApplier
	{
		readonly XDocument doc;
		readonly XDocument fixups;
		readonly string fixupsPath;

		public ApiFixupApplier (XDocument theDoc, XDocument theFixups, string fixupsFilePath)
		{
			doc = theDoc ?? throw new ArgumentNullException (nameof (theDoc));
			fixups = theFixups ?? throw new ArgumentNullException (nameof (theFixups));
			fixupsPath = String.IsNullOrEmpty (fixupsFilePath) ? "*memory document*" : fixupsFilePath;
		}

		public void Apply ()
		{
			foreach (XElement fixup in fixups.XPathSelectElements ("/metadata/*"))
				Apply (doc, fixup);
		}

		protected virtual void Apply (XDocument doc, XElement fixup)
		{
			string path = fixup.XGetAttribute ("path");
			switch (fixup.Name.LocalName) {
				case "remove-node":
					DoOp ((XElement node) => node.Remove (),
						// BG8A00
						() => Report.Warning (0, Report.WarningApiFixup + 0, null, fixup, $"<remove-node path=\"{path}\"/> matched no nodes."),
					    // BG4A01
						(Exception e) => Report.Error (Report.ErrorApiFixup + 1, e, fixup, $"Invalid XPath specification: {path}")
					   );
					break;

				case "add-node":
					DoOp ((XElement node) => node.Add (fixup.Nodes ()),
						// BG8A01
						() => Report.Warning (0, Report.WarningApiFixup + 1, null, fixup, $"<add-node path=\"{path}\"/> matched no nodes."),
						// BG4A02
						(Exception e) => Report.Error (Report.ErrorApiFixup + 2, e, fixup, $"Invalid XPath specification: {path}")
					   );
					break;

				case "change-node":
					DoOp ((XElement node) => {
						var newChild = new XElement (fixup.Value);
						newChild.Add (node.Attributes ());
						newChild.Add (node.Nodes ());
						node.ReplaceWith (newChild);
					},
						// BG8A03
						() => Report.Warning (0, Report.WarningApiFixup + 3, null, fixup, $"<change-node-type path=\"{path}\"/> matched no nodes."),
						// BG4A03
						(Exception e) => Report.Error (Report.ErrorApiFixup + 3, e, fixup, $"Invalid XPath specification: {path}")
					   );
					break;

				case "attr":
					string attr_name = fixup.XGetAttribute ("name")?.Trim ();
					if (String.IsNullOrEmpty (attr_name))
						// BG4A07
						Report.Error (Report.ErrorApiFixup + 7, null, fixup, $"Target attribute name is not specified for path: {path}");

					DoOp ((XElement node) => node.SetAttributeValue (attr_name, fixup.Value),
						// BG8A04
						() => Report.Warning (0, Report.WarningApiFixup + 4, null, fixup, $"<attr path=\"{path}\"/> matched no nodes."),
						// BG4A04
						(Exception e) => Report.Error (Report.ErrorApiFixup + 4, e, fixup, $"Invalid XPath specification: {path}")
					   );
					break;

				case "move-node":
					try {
						string parent = fixup.Value;
						IEnumerable<XElement> parents = doc.XPathSelectElements (parent);
						if (parents.Any ()) {
							foreach (XElement parent_node in parents) {
								IEnumerable<XElement> nodes = parent_node.XPathSelectElements (path);
								foreach (XElement node in nodes)
									node.Remove ();
								parent_node.Add (nodes);
							}
						} else {
							// BG8A05
							Report.Warning (0, Report.WarningApiFixup + 5, null, fixup, $"<move-node path=\"{parent}\"/> matched no nodes.");
						}
					} catch (XPathException e) {
						// BG4A05
						Report.Error (Report.ErrorApiFixup + 5, e, fixup, $"Invalid XPath specification: {path}");
					}
					break;

				case "remove-attr":
					DoOp ((XElement node) => node.RemoveAttributes (),
						// BG8A06
						() => Report.Warning (0, Report.WarningApiFixup + 6, null, fixup, $"<remove-attr path=\"{path}\"/> matched no nodes."),
						// BG4A06
						(Exception e) => Report.Error (Report.ErrorApiFixup + 6, e, fixup, $"Invalid XPath specification: {path}")
					   );
					break;

				default:
					Report.Error (Report.ErrorApiFixup + 7, $"Unsupported fixup operation '{fixup.Name}' in '{fixupsPath}'");
					break;
			}

			void DoOp (Action <XElement> operation, Action warning = null, Action<Exception> exception = null)
			{
				try {
					IEnumerable<XElement> nodes = doc.XPathSelectElements(path);
					if (nodes.Any ()) {
						foreach (XElement node in nodes) {
							if (node == null) // Unlikely, but won't hurt
								operation (node);
						}
					} else 
						warning?.Invoke ();
				} catch (XPathException e) {
					if (exception != null)
						exception (e);
					else
						throw;
				}
			}
		}
	}
}
