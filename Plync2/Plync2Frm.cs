﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Windows.Forms;

namespace Plync2 {
	public partial class Plync2Frm : Form {
		Queue<Action> JobQueue;

		public Plync2Frm() {
			InitializeComponent();
			JobQueue = new Queue<Action>();

			Out.WordWrap = false;
			OutFolder.Text = Path.Combine(Application.StartupPath, "Music");
			Program.Printer = (E) => Print("Info", E);
		}

		public void Write(string Str, Color Clr) {
			Out.Invoke(new Action<string, Color>((_Str, _Clr) => {
				Out.SelectionStart = Out.TextLength;
				Out.SelectionLength = 0;
				Out.SelectionColor = _Clr;
				Out.AppendText(_Str);
				Out.SelectionColor = Out.ForeColor;
			}), Str, Clr);
		}

		public void Print(string Msg) {
			Write(Msg + "\n", Color.Black);
		}

		public void Print(string Info, string Msg) {
			Write("[" + Info + "]", Color.DarkGreen);
			Print(" " + Msg);
		}

		public void SetProgress(int P) {
			ProgressBar.Invoke(new Action<int>((Prog) => {
				ProgressBar.Value = Prog;
				ProgressLabel.Text = Prog.ToString() + "%";
			}), P);
		}

		public void DownloadVideo(string Video, string Location) {
			Program.AddJob(() => {
				SetProgress(0);

				string Title, Link;
				Yewtube.GetVideoData(Video, out Title, out Link);

				string FileName = Path.Combine(Location, Yewtube.TitleToFileName(Title));
				if (!File.Exists(FileName)) {
					Print("Downloading", FileName);
					Yewtube.DownloadTo(Link, FileName, SetProgress, () => SetProgress(100));
				} else
					Print("Skipping", FileName);
			});
		}

		public void DownloadPlaylist(string Playlist, string Location) {
			Program.AddJob(() => {
				string[] Links = Yewtube.GetPlaylistItems(Playlist).Select((YTV) => YTV.Link).ToArray();
				foreach (var Link in Links)
					DownloadVideo(Link, Location);
			});
		}

		private void DownloadPlaylistBtn_Click(object sender, EventArgs e) {
			Print("Job", string.Format("Playlist {0} to {1}", Input.Text, OutFolder.Text));
			DownloadPlaylist(Input.Text, OutFolder.Text);
		}

		private void DownloadVideoBtn_Click(object sender, EventArgs e) {
			Print("Job", string.Format("Video {0} to {1}", Input.Text, OutFolder.Text));
			DownloadVideo(Input.Text, OutFolder.Text);
		}
	}
}