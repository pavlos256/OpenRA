//
// Author(s):
//     Pavlos Touboulidis <pav@pav.gr>
//
// Created on 2014-6-27
//
using System;

namespace Oratool.Commands
{
	public interface ICommand
	{
		string ShortDescription { get; }

		void ShowHelp();

		ExitCode Execute(string[] args);
	}
}
