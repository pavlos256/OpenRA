//
// Author(s):
//     Pavlos Touboulidis <pav@pav.gr>
//
// Created on 2014-6-27
//
using System;

namespace Oratool.Commands
{
	public enum ExitCode
	{
		OK = 0,
		UnhandledException,
		SyntaxError,
		FileNotFound
	}
}
