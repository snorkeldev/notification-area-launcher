using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LauncherWinFormsCore
{
	public class ContextMenuApp : ApplicationContext
	{
		ContextMenuStrip _contextMenu;
		NotifyIcon _notifyIcon;

		public ContextMenuApp()
		{
			var data = Program.Data;

			_notifyIcon = new NotifyIcon();

			// icon hover text
			_notifyIcon.Text = data.title;
			_notifyIcon.Icon = Properties.Resources.Icon1;


			_contextMenu = new ContextMenuStrip();
			_notifyIcon.ContextMenuStrip = _contextMenu;
			_notifyIcon.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
			//notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

			CreateContextMenu();

			_notifyIcon.Visible = true;
		}

		private void ContextMenuStrip_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
		{
			var strip = _notifyIcon.ContextMenuStrip;
			//Screen screen = Screen.FromPoint(Cursor.Position);
			//int x = screen.WorkingArea.Left - strip.Width;
			//int y = screen.WorkingArea.Bottom - strip.Height;
			int x = Cursor.Position.X;
			int y = Cursor.Position.Y;

			strip.Show(x, y);
		}

		private async void CreateContextMenu()
		{
			var data = Program.Data;

			_contextMenu.Items.Clear();

			for (int i = 0; i < data.groups.Length; i++)
			{
				var group = data.groups[i];
				for (int j = 0; j < group.items.Length; j++)
				{
					var item = group.items[j];

					Image image = null;
					if (!string.IsNullOrWhiteSpace(item.url))
					{
						if (data.webIcons
							&& (item.url.StartsWith("https://")
							|| item.url.StartsWith("http://")))
						{
							if (!string.IsNullOrWhiteSpace(item.icon))
								image = await GetFaviconAsync(item.icon);
						}
						else if (data.fileIcons && File.Exists(item.url))
						{
							var fileIcon = GetFileIcon(item.url);
							if (fileIcon != null)
							{
								image = fileIcon.ToBitmap();
							}
						}
						else if (data.folderIcons && Directory.Exists(item.url))
						{
							var folderIcon = GetFolderIcon(item.url);
							if (folderIcon != null)
							{
								image = folderIcon.ToBitmap();
							}
						}
					}

					_contextMenu.Items.Add(item.name, image, (sender, e) => ExecuteTask(item));
				}

				_contextMenu.Items.Add(new ToolStripSeparator());
			}

			_contextMenu.Items.Add("Select file", null, SelectFileMenuItem_Click);
			_contextMenu.Items.Add("Reload", null, ReloadMenuItem_Click);
			_contextMenu.Items.Add("Exit", null, ExitMenuItem_Click);
		}

		private void SelectFileMenuItem_Click(object? sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.InitialDirectory = Program.GetFileDialogInitialDirectory();// "c:\\";
			openFileDialog.Filter = "Json files (*.json)|*.json"; //|All files (*.*)|*.*";

			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				string selectedFilePath = openFileDialog.FileName;
				Program.SetNewFile(selectedFilePath);
				Program.LoadData();
				CreateContextMenu();

				Console.WriteLine("Selected file: " + selectedFilePath);
			}
			else
			{
				// User canceled the dialog
				Console.WriteLine("File selection canceled.");
			}
		}

		private void ReloadMenuItem_Click(object? sender, EventArgs e)
		{
			Program.LoadData();
			CreateContextMenu();
		}

		private void ExecuteTask(Program.JsonItem item)
		{
			if (!string.IsNullOrWhiteSpace(item.url))
			{
				try
				{
					ProcessStartInfo psi = new ProcessStartInfo
					{
						FileName = item.url,
						UseShellExecute = true,
						Arguments = item.args
					};
					Process.Start(psi);
				}
				catch (Exception ex)
				{
					MessageBox.Show("Error opening the URL: " + ex.Message);
				}
			}

			if (!string.IsNullOrWhiteSpace(item.clipboard))
			{
				Clipboard.SetText(item.clipboard);
			}
		}

		private Icon GetFileIcon(string filePath)
		{
			try
			{
				// Extract the icon associated with the file
				Icon fileIcon = Icon.ExtractAssociatedIcon(filePath);
				return fileIcon;
			}
			catch (Exception)
			{
				// Handle exceptions if the icon extraction fails
				return null;
			}
		}

		private async Task<Image> GetFaviconAsync(string faviconUrl)
		{
			try
			{
				using (HttpClient client = new HttpClient())
				{
					// Send a GET request to the website
					//HttpResponseMessage response = await client.GetAsync(url);

					//Uri uri = new Uri(url);

					//if (response.IsSuccessStatusCode)
					{
						//string faviconUrl = $"{uri.Scheme}://{uri.Host}/favicon.ico";

						// Download the favicon
						byte[] faviconData = await client.GetByteArrayAsync(faviconUrl);

						// Convert the byte array to an Image
						using (MemoryStream ms = new MemoryStream(faviconData))
						{
							return Image.FromStream(ms);
						}
					}
					//else
					//{
					//	throw new Exception($"HTTP error: {response.StatusCode}");
					//}
				}
			}
			catch (Exception ex)
			{
				//throw new Exception($"Failed to get favicon: {ex.Message}");
				return null;
			}
		}

		static Icon GetFolderIcon(string folderPath)
		{
			try
			{
				var path = Path.GetFullPath(folderPath);


				SHFILEINFO shfi = new SHFILEINFO();
				IntPtr hIcon = SHGetFileInfo(path, 0, ref shfi, (uint)Marshal.SizeOf(shfi), SHGFI_ICON | SHGFI_SMALLICON);

				if (hIcon != IntPtr.Zero)
				{
					Icon folderIcon = Icon.FromHandle(shfi.hIcon);
					return folderIcon;
				}

				return null;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				return null;
			}
		}


		[StructLayout(LayoutKind.Sequential)]
		private struct SHFILEINFO
		{
			public IntPtr hIcon;
			public IntPtr iIcon;
			public uint dwAttributes;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szDisplayName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public string szTypeName;
		}

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

		private const uint SHGFI_ICON = 0x100;
		private const uint SHGFI_SMALLICON = 0x1;

		private void ExitMenuItem_Click(object sender, EventArgs e)
		{
			// Handle the "Exit" context menu item click
			Application.ExitThread();
		}
	}
}