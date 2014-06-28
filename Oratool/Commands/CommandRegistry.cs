//
// Author(s):
//     Pavlos Touboulidis <pav@pav.gr>
//
// Created on 2014-6-27
//
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Oratool.Commands
{
	class CommandRegistry
	{
		public static readonly CommandRegistry Singleton = new CommandRegistry();

		static CommandRegistry()
		{
			var i = typeof(ICommand);

			foreach (var commandType in typeof(CommandRegistry).Assembly.GetTypes().Where(t => t.IsClass && i.IsAssignableFrom(t)))
			{
				Singleton.Register(commandType.Name.ToLower(), (ICommand)Activator.CreateInstance(commandType));
			}
		}

		private Dictionary<string, ICommand> commandsByName = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase);

		public void Register(string name, ICommand command)
		{
			if (name == null)
				throw new ArgumentNullException("name");

			if (command == null)
				throw new ArgumentNullException("command");

			this.commandsByName.Add(name, command);
		}

		public ICommand GetCommand(string name)
		{
			if (name == null)
				throw new ArgumentNullException("name");

			ICommand command;
			if (this.commandsByName.TryGetValue(name, out command))
				return command;

			throw new KeyNotFoundException(string.Format("No command with the name '{0}' was found.", name));
		}

		public IEnumerable<string> GetCommandNames()
		{
			return this.commandsByName.Keys;
		}
	}
}
