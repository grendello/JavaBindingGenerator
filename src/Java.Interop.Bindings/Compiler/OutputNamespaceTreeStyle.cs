//
// OutputNamespaceTreeStyle.cs
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

namespace Java.Interop.Bindings.Compiler
{
	public enum OutputNamespaceTreeStyle
	{
		// Each file has the class full name + extension, no subdirectories
		//
		// .
		// ├── Android.Media.TV.File.cs
		// ├── Android.OS.File.cs
		// └── Android.OS.Health.File.cs
		//
		Single,

		// Use full namespace as the name of the directory, no subdirectories, e.g.
		// .
		// ├── Android.Media.TV
		// │   └── File.cs
		// ├── Android.OS
		// │   └── File.cs
		// └── Android.OS.Health
		//     └── File.cs
		//
		Shallow,

		// Each namespace segment is a separate level of directories, e.g.
		//
		// .
		// └── Android
		//     ├── File.cs
		//     ├── Media
		//     │   ├── File.cs
		//     │   └── TV
		//     │       └── File.cs
		//     └── OS
		//         ├── File.cs
		//         └── Health
		//             └── File.cs
		//
		Deep,

		// First level is the namespace name's first segment, below that there's one level of subdirectories, each
		// having full namespace name
		// .
		// └── Android
		//     ├── Android.Media.TV
		//     │   └── File.cs
		//     ├── Android.OS
		//     │   └── File.cs
		//     ├── Android.OS.Health
		//     │   └── File.cs
		//     └── File.cs
		//
		FirstLevelThenFullShallow,

		// First level is namespace name's first segment, below that there's one level of subdirectories, each
		// having the namespace name without the first segment
		// .
		// └── Android
		//     ├── File.cs
		//     ├── Media.TV
		//     │   └── File.cs
		//     ├── OS
		//     │   └── File.cs
		//     └── OS.Health
		//         └── File.cs
		//
		FirstLevelThenShortShallow,

		// First level is the namespace name's first segment, inside each file has the class full name +
		// extension, no subdirectories
		//
		// .
		// └── Android
		//     ├── Android.Media.TV.File.cs
		//     ├── Android.OS.File.cs
		//     └── Android.OS.Health.File.cs
		//
		FirstLevelThenFullSingle,

		// First level is the namespace name's first segment, inside each file has the class full name without
		// the namespace name's first segment + extension, no subdirectories
		//
		// .
		// └── Android
		//     ├── Media.TV.File.cs
		//     ├── OS.File.cs
		//     └── OS.Health.File.cs
		//
		FirstLevelThenShortSingle,
	}
}
