using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Java.Interop.Bindings;
using Java.Interop.Bindings.Compiler;
using Java.Interop.Bindings.Compiler.CSharp;
using Java.Interop.Bindings.Syntax;
using Mono.Options;

namespace BindingGenerator
{
	class BindingGeneratorProgram
	{
		static StringComparer fileNameComparer = StringComparer.Ordinal; // TODO: detect OS

		public static void Main (string[] args)
		{
			GeneratorOptions options = ParseOptions (args);
			if (options == null) {
				Environment.Exit (1);
				return;
			}
			Run (options);
		}

		static void Run (GeneratorOptions options)
		{
			if (!File.Exists (options.ApiDescriptionFilePath))
				throw new InvalidOperationException ($"API description file '{options.ApiDescriptionFilePath}' does not exist.");

			List<string> fixups = null;
			ApiDescriptionReader apiReader = null;
			try {
				if (options.ApiFixupFiles != null && options.ApiFixupFiles.Count > 0) {
					foreach (string afp in options.ApiFixupFiles.Distinct (fileNameComparer)) {
						if (String.IsNullOrEmpty (afp))
							continue;
						if (!File.Exists (afp))
							throw new InvalidOperationException ($"API fixup file '{afp}' does not exist.");
						if (fixups == null)
							fixups = new List<string> ();
						fixups.Add (afp);
					}
				}

				apiReader = new ApiDescriptionReader (options.ApiDescriptionFilePath, fixups) {
					DumpFixedUp = options.DumpFixedUp
				};

				FormattingCodeGenerator codeGenerator = GetCodeGenerator (options.CodeGenerator, new FormattingContext (GetCodeStyle (options.CodeStyle)));
				var context = new GeneratorContext (codeGenerator, options.FileEncoding) {
					DumpHierarchy = options.DumpHierarchy,
					HierarchyDumpFilePath = options.ApiDescriptionFilePath + ".hierarchy"
				};
				var generator = new Generator (context, options.OverwriteFiles);
				generator.Generate (options.OutputPath, apiReader.Parse ());
			} finally {
				apiReader?.Dispose ();
			}
		}

		static FormattingStyle GetCodeStyle (string name)
		{
			return new FormattingStyle ();
		}

		static FormattingCodeGenerator GetCodeGenerator (string name, FormattingContext context)
		{
			return new DefaultCSharpCodeGenerator (context);
		}

		static GeneratorOptions ParseOptions (string [] args)
		{
			bool show_help = false;
			var ret = new GeneratorOptions ();
			var opts = new OptionSet {
				"Usage: generator2 OPTIONS+ API_DESCRIPTION_FILE",
				"",
				"Options:",
				{"fixup=|fixups=",
					"XML {FILE} controlling the generated API.\n" +
					"http://www.mono-project.com/GAPI#Altering_and_Extending_the_API_File",
					v => ret.ApiFixupFiles.Add (v)
				},
				{"dump-fixedup", "Write fixed up API description to a file with .fixedup extension and the same base name as the source API file", v => ret.DumpFixedUp = true},
				{"dump-hierarchy", "Write element hierarchy as parsed from the API description to a file with .hierarchy extension and the same base name as the API description file", v => ret.DumpHierarchy = true},
				{"o=|output=", "{DIRECTORY} in which to place the generated sources", v => ret.OutputPath = v},
				{"log-level=|l=", $"Sets the lowest log message level (one of: {GetLogLevelNames ()}). Default: {Logger.Level}", v => Logger.Level = ParseLogLevel (v)},
				{"w|overwrite-files", "Overwrite existing output files. Default: {ret.OverwriteFiles}", v => ret.OverwriteFiles = true},
				{"e=|encoding=", "Use the specified encoding when writing files. Default: {ret.Encoding.WebName}", v => ret.FileEncoding = Encoding.GetEncoding (v)},
				{"h|help", "Show this help screen", v => show_help = true}
			};

			List<string> apis = opts.Parse (args);
			if (args.Length < 2 || show_help) {
				opts.WriteOptionDescriptions (Console.Out);
				return null;
			}

			if (apis.Count == 0)
				throw new InvalidOperationException ("A .xml file must be specified.");

			ret.ApiDescriptionFilePath = apis [0];
			return ret;
		}

		static string GetLogLevelNames ()
		{
			return String.Join (", ", Enum.GetNames (typeof (LogLevel)).Select (l => l.ToLowerInvariant ()));
		}

		static LogLevel ParseLogLevel (string l)
		{
			l = l?.Trim ();
			if (String.IsNullOrEmpty (l))
				return Logger.Level;

			LogLevel ret;
			if (Enum.TryParse (l, true, out ret))
				return ret;

			throw new InvalidOperationException ($"Invalid log level name: {l}");
		}
	}
}
