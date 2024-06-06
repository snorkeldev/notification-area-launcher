using System.Configuration;
using System.Text.Json;

namespace LauncherWinFormsCore
{
	internal static class Program
	{
		public class JsonItem
		{
			public string name { get; set; }
			public string url { get; set; }
			public string icon { get; set; }
			public string args { get; set; }
			public string clipboard { get; set; }
		}

		public class JsonGroup
		{
			public string group { get; set; }
			public JsonItem[] items { get; set; }
		}

		public class JsonFile
		{
			public string title { get; set; } = "Launcher";
			public bool fileIcons { get; set; } = true;
			public bool folderIcons { get; set; } = true;
			public bool webIcons { get; set; } = true;
			public JsonGroup[] groups { get; set; }
		}

		public static JsonFile Data;

		public static void SetNewFile(string path)
		{
			Properties.Settings.Default.LastSelectedFilePath = path;
			Properties.Settings.Default.Save();
		}

		public static string GetFileDialogInitialDirectory()
		{
			return Properties.Settings.Default.LastSelectedFilePath;
		}

		public static void LoadData()
		{
			var jsonFilePath = Properties.Settings.Default.LastSelectedFilePath;
			try
			{
				var jsonData = File.ReadAllText(jsonFilePath);
				var parsedData = JsonSerializer.Deserialize<JsonFile>(jsonData);
				Data = parsedData;
				foreach (var g in Data.groups)
				{
					foreach(var e in g.items)
					{
						if (!string.IsNullOrEmpty(e.url))
							e.url = ProcessPath(e.url);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Data = new JsonFile();
				Data.groups = new JsonGroup[0];
			}
		}

		static string ProcessPath(string path)
		{
			try
			{
				return Environment.ExpandEnvironmentVariables(path);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return path;
			}
		}

		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// To customize application configuration such as set high DPI settings or default font,
			// see https://aka.ms/applicationconfiguration.
			ApplicationConfiguration.Initialize();

			LoadData();

			Application.Run(new ContextMenuApp());

		}
	}
}