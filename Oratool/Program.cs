//
// Author(s):
//     Pavlos Touboulidis <pav@pav.gr>
//
// Created on 2014-6-26
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Oratool.Commands;

namespace Oratool
{
	class Program
	{
		public static readonly string Name = Process.GetCurrentProcess().ProcessName;

		public static int Main(string[] args)
		{
			string commandName = "help";

			if (args.Length > 1)
			{
				commandName = args[0];
				args = args.Skip(1).ToArray();
			}

			var command = CommandRegistry.Singleton.GetCommand(commandName);
			return (int)command.Execute(args);
		}

		public static string[] GlobArgs(IEnumerable<string> args)
		{
			return args.SelectMany(arg => Glob.Expand(arg).OrderBy(s => s)).ToArray();
		}
	}
}
