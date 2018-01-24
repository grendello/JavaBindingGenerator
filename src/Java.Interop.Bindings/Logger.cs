//
// Logger.cs
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

namespace Java.Interop.Bindings
{
	public static class Logger
	{
		public static LogLevel Level { get; set; } = LogLevel.Info;

		public static void Fatal (string message)
		{
			Write ($"Fatal: {message}");
		}

		public static void Error (string message)
		{
			if (Level < LogLevel.Error)
				return;

			Write ($"Error: {message}");
		}

		public static void Warning (string message)
		{
			if (Level < LogLevel.Warning)
				return;

			Write ($"Warning: {message}");
		}

		public static void Info (string message)
		{
			if (Level < LogLevel.Info)
				return;

			Write ($"Info: {message}");
		}

		public static void Debug (string message)
		{
			if (Level < LogLevel.Debug)
				return;

			Write ($"Debug: {message}");
		}

		public static void Verbose (string message)
		{
			if (Level < LogLevel.Verbose)
				return;

			Write ($"Verbose: {message}");
		}

		public static void Excessive (string message)
		{
			if (Level < LogLevel.Excessive)
				return;

			Write ($"Excessive: {message}");
		}

		static void Write (string text)
		{
			Console.WriteLine (text);
		}
	}
}
