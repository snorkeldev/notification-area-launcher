using System.Configuration;
using System.Text.Json;
using System.Windows.Forms;

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

		public static void LoadData()
		{
			var jsonFilePath = ConfigurationManager.AppSettings["paths"];
			var jsonData = File.ReadAllText(jsonFilePath);
			var parsedData = JsonSerializer.Deserialize<JsonFile>(jsonData);
			Data = parsedData;
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