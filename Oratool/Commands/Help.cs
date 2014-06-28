//
// Author(s):
//     Pavlos Touboulidis <pav@pav.gr>
//
// Created on 2014-6-27
//
using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

namespace Oratool.Commands
{
	public class Help : ICommand
	{
		public string ShortDescription { get { return "Show this help text"; } }

		public void ShowHelp()
		{
			Console.WriteLine("Usage: {0} <command> [option]...", Process.GetCurrentProcess().ProcessName);
			Console.WriteLine();
			Console.WriteLine("The available commands are:");
			foreach (string name in CommandRegistry.Singleton.GetCommandNames().OrderBy(name => name))
			{
				var command = CommandRegistry.Singleton.GetCommand(name);
				Console.WriteLine("{0,-20} {1}", name, command.ShortDescription);
			}
			Console.WriteLine();
			Console.WriteLine("See '{0} {1} <command>' for help about a command", Program.Name, typeof(Help).Name.ToLower());
		}

		public ExitCode Execute(string[] args)
		{
			if (args.Length == 0)
			{
				ShowHelp();
				return ExitCode.OK;
			}

			string commandName = args[0];
			ICommand command;
			try
			{
				command = CommandRegistry.Singleton.GetCommand(commandName);
			}
			catch (KeyNotFoundException)
			{
				ShowHelp();
				return ExitCode.SyntaxError;
			}

			command.ShowHelp();
			return ExitCode.OK;
		}
	}
}

