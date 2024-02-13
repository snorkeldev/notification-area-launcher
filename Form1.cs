using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace LauncherWinFormsCore
{
	public partial class Form1 : Form
	{
		ContextMenuStrip _contextMenu;

		public Form1()
		{
			InitializeComponent();

			var data = Program.Data;

			// window title
			Text = data.title;

			var notifyIcon = this.notifyIcon1;

			// icon hover text
			notifyIcon.Text = data.title;

			_contextMenu = new ContextMenuStrip();
			notifyIcon.ContextMenuStrip = _contextMenu;
			notifyIcon.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
			notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

			//ForeColor = Color.Black;
			//BackColor = Color.Black;

			CreateContextMenu();
			CreateGridLayout();

			//this.AutoSize = true;
			//this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
		}

		private void CreateContextMenu()
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
					if (data.fileIcons)
					{
						var fileIcon = GetFileIcon(item.url);
						if (fileIcon != null)
						{
							image = fileIcon.ToBitmap();
						}
					}

					_contextMenu.Items.Add(item.name, image, OpenMenuItem_Click);
				}

				_contextMenu.Items.Add(new ToolStripSeparator());
			}
			
			//_contextMenu.Items.Add("Open", null, OpenMenuItem_Click);
			//_contextMenu.Items.Add("Exit", null, ExitMenuItem_Click);
		}

		private void CreateGridLayout()
		{
			/*
			TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
			tableLayoutPanel.Dock = DockStyle.Fill;

			// Set the number of rows and columns in the table
			tableLayoutPanel.RowCount = 2;
			tableLayoutPanel.ColumnCount = 3;

			// Add controls to the table
			for (int row = 0; row < tableLayoutPanel.RowCount; row++)
			{
				for (int col = 0; col < tableLayoutPanel.ColumnCount; col++)
				{
					var group = new GroupBox();

					Button button = new Button();
					button.Text = $"Button {row + 1}-{col + 1}";
					button.Dock = DockStyle.Top;

					var url = "C:\\GIT\\Launcher\\LauncherWinFormsCore\\bin\\Release\\net8.0-windows\\publish";
					LinkLabel linkLabel = new LinkLabel();
					linkLabel.Text = "Visit OpenAI's website";
					linkLabel.LinkClicked += LinkLabel_LinkClicked;
					linkLabel.Dock = DockStyle.Top;
					linkLabel.Tag = url;
					
					var toolTip = new ToolTip();
					toolTip.SetToolTip(linkLabel, linkLabel.Tag.ToString());

					group.Controls.Add(button);
					group.Controls.Add(linkLabel);

					// Adjust Row and Column properties for each control
					tableLayoutPanel.Controls.Add(group, col, row);
				}
			}

			this.Controls.Add(tableLayoutPanel);

			this.Size = tableLayoutPanel.PreferredSize;
			*/

			var data = Program.Data;

			var groupsReverse = data.groups.Reverse();
			foreach (var group in groupsReverse)
			{
				var groupBox = new GroupBox();
				foreach (var item in group.items)
				{
					var button = new Button();
					button.Text = item.name;
					button.Dock = DockStyle.Bottom;
					button.Click += (sender, e) => ExecuteTask(item);
					button.ImageAlign = ContentAlignment.MiddleRight;
					button.TextAlign = ContentAlignment.MiddleLeft;
					button.Height = 40;

					if (data.fileIcons)
					{
						var fileIcon = GetFileIcon(item.url);
						if (fileIcon != null)
						{
							button.Image = fileIcon.ToBitmap();
						}
					}

					groupBox.Controls.Add(button);
				}

				groupBox.Text = group.group;
				groupBox.Dock = DockStyle.Top;
				groupBox.AutoSize = true;
				groupBox.AutoSizeMode = AutoSizeMode.GrowAndShrink;
				Controls.Add(groupBox);
			}
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

		/*
		private void LinkLabel_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
		{
			// Handle the link click event
			//string url = "https://www.openai.com";
			string url = (string)((LinkLabel)sender).Tag;

			try
			{
				ProcessStartInfo psi = new ProcessStartInfo
				{
					FileName = url,
					UseShellExecute = true
				};
				Process.Start(psi);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error opening the URL: " + ex.Message);
			}
		}*/

		private void ContextMenuStrip_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
		{
			var strip = notifyIcon1.ContextMenuStrip;
			//Screen screen = Screen.FromPoint(Cursor.Position);
			//int x = screen.WorkingArea.Left - strip.Width;
			//int y = screen.WorkingArea.Bottom - strip.Height;
			int x = Cursor.Position.X;
			int y = Cursor.Position.Y;

			strip.Show(x, y);
		}

		private void NotifyIcon_DoubleClick(object sender, EventArgs e)
		{
			// Handle double-click on the notification icon
			// This can be used to show/hide your main form, for example
			Show();
			WindowState = FormWindowState.Normal;
		}
		private void OpenMenuItem_Click(object sender, EventArgs e)
		{
			// Handle the "Open" context menu item click
			Show();
			WindowState = FormWindowState.Normal;
		}

		private void ExitMenuItem_Click(object sender, EventArgs e)
		{
			// Handle the "Exit" context menu item click
			Close();
		}

		/*protected override void OnFormClosing(FormClosingEventArgs e)
		{
			// Make sure to hide the form instead of closing it when the user clicks the close button
			if (e.CloseReason == CloseReason.UserClosing)
			{
				e.Cancel = true;
				Hide();
			}
			base.OnFormClosing(e);
		}*/

		/*protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				notifyIcon.Dispose();
				contextMenuStrip.Dispose();
			}
			base.Dispose(disposing);
		}*/

	}
}
