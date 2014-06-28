//
// Author(s):
//     Pavlos Touboulidis <pav@pav.gr>
//
// Created on 2014-6-27
//
using System;

namespace Oratool.Commands
{
	public static class CommandExtensions
	{
		public static ExitCode WriteError(ExitCode code, string message)
		{
			Console.Error.WriteLine("Error {0} ({1}): {2}", (int)code, code, message);
			return code;
		}

		public static ExitCode WriteError(ExitCode code, Exception ex)
		{
			Console.Error.WriteLine("Error {0} ({1}): {2}", (int)code, code, ex.Message);
			Console.Error.WriteLine();
			Console.Error.WriteLine(ex);
			return code;
		}

		public static ExitCode WriteError(this ICommand command, ExitCode code, string message, bool showHelp = false)
		{
			Console.Error.WriteLine("Error {0} ({1}): {2}", (int)code, code, message);

			if (showHelp)
			{
				Console.Error.WriteLine();
				command.ShowHelp();
			}

			return code;
		}
	}
}

