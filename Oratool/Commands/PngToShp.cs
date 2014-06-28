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
using Mono.Options;
using System.Drawing;
using Oratool.Graphics;
using System.IO;
using System.Drawing.Imaging;

namespace Oratool.Commands
{
	public class PngToShp : ICommand
	{
		public string ShortDescription { get { return "Convert a series of PNG files to an SHP file"; } }

		class Options
		{
			public string PalettePath;
			public string OutputPath;

			public List<int> TeamColors = new List<int>();

			public void SetTeamColors(string s)
			{
				if (string.IsNullOrEmpty(s))
					return;

				s = s.Trim();
				if (s.Length == 0)
					return;

				this.TeamColors.AddRange(
					s.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
					.Select(si => int.Parse(si))
					.Distinct());
			}
		}

		private Options options = new Options();
		private OptionSet oset;

		public PngToShp()
		{
			this.oset = new OptionSet()
			{
				{ "p|palette=", "The palette file to use.", path => this.options.PalettePath = path },
				{ "t|team-colors=", "Comma separated indices of the team colors.", (string s) => this.options.SetTeamColors(s) },
				{ "o|output=", "Output file. If not specified, the output filename is based on the first input file name.", path => this.options.OutputPath = path }
			};
		}

		public void ShowHelp()
		{
			Console.WriteLine("Usage: {0} {1} [option]... <file>...", Program.Name, typeof(PngToShp).Name.ToLower());
			Console.WriteLine();
			this.oset.WriteOptionDescriptions(Console.Out);
		}

		public ExitCode Execute(string[] args)
		{
			try
			{
				string[] inputFiles = Program.GlobArgs(this.oset.Parse(args));

				if (inputFiles.Length == 0)
					return this.WriteError(ExitCode.SyntaxError, "No input files specified.", true);

				if (string.IsNullOrEmpty(this.options.PalettePath))
					return this.WriteError(ExitCode.SyntaxError, "No palette specified.", true);

				foreach (string path in inputFiles)
					if (!File.Exists(path))
						return this.WriteError(ExitCode.FileNotFound, string.Format("Cannot find input file '{0}'.", path));

				if (!File.Exists(this.options.PalettePath))
					return this.WriteError(ExitCode.FileNotFound, string.Format("Cannot find palette file '{0}'.", this.options.PalettePath));

				var availableColors = GetAvailableColors();
				var indexer = new ColorIndexer(availableColors);

				string outputPath = this.options.OutputPath;
				if (string.IsNullOrEmpty(outputPath))
					outputPath = inputFiles[0] + ".shp";

				using (var outputStream = File.Create(outputPath))
				{
					Size? size = null;
					var frames = new List<byte[]>();

					foreach (string path in inputFiles)
					{
						using (var bitmap = new Bitmap(path))
						{
							if (size == null)
							{
								size = bitmap.Size;
							}
							else if (!bitmap.Size.Equals(size.Value))
							{
								throw new InvalidOperationException(
									string.Format(
										"All input files must be of the same size. The first was {0}x{1} but '{2}' is {3}x{4}",
										size.Value.Width, size.Value.Height,
										path,
										bitmap.Size.Width, bitmap.Size.Height));
							}

							frames.Add(indexer.Index(bitmap));
						}
					}

					ShpWriter.Write(outputStream, size.Value, frames);
				}

				return ExitCode.OK;
			}
			catch (OptionException ex)
			{
				return this.WriteError(ExitCode.SyntaxError, ex.Message, true);
			}
		}

		IEnumerable<IndexedColor> GetAvailableColors()
		{
			if (string.IsNullOrEmpty(this.options.PalettePath))
				yield break;

			var palette = new CncPalette(this.options.PalettePath);

			for (int i = 0; i < palette.Colors.Length; i++)
			{
				if (!this.options.TeamColors.Contains(i))
					yield return new IndexedColor((byte)i, palette.Colors[i]);
			}
		}
	}
}
