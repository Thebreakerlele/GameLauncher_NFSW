﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using GameLauncher.App;
using GameLauncher.App.Classes;
using GameLauncherReborn;

namespace GameLauncher {
    internal static class Program {
        [STAThread]
        internal static void Main() {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Application.ExecutablePath) ?? throw new InvalidOperationException());


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);

            Form SplashScreen2 = new SplashScreen();
            SplashScreen2.Show();

            if (Self.isTempFolder(Directory.GetCurrentDirectory())) {
                MessageBox.Show(null, "Please, extract me and my DLL files before executing...", "GameLauncher", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Environment.Exit(0);
            }

            try {
                File.Delete(Directory.GetCurrentDirectory() + "\\tempname.zip");
            } catch { /* ignored */ }


            if (!File.Exists("LZMA.dll"))
                File.WriteAllBytes("LZMA.dll", ExtractResource.AsByte("GameLauncher.LZMA.LZMA.dll"));

            if (!DetectLinux.UnixDetected() && !File.Exists("discord-rpc.dll"))
                File.WriteAllBytes("discord-rpc.dll", ExtractResource.AsByte("GameLauncher.Discord.discord-rpc.dll"));

            if (DetectLinux.LinuxDetected() && !File.Exists("libdiscord-rpc.so"))
                File.WriteAllBytes("libdiscord-rpc.so", ExtractResource.AsByte("GameLauncher.Discord.libdiscord-rpc.so"));

            if (DetectLinux.MacOSDetected() && !File.Exists("libdiscord-rpc.dylib"))
                File.WriteAllBytes("libdiscord-rpc.dylib", ExtractResource.AsByte("GameLauncher.Discord.libdiscord-rpc.dylib"));

            if (File.Exists("GL_Update.exe")) 
				File.Delete("GL_Update.exe");

			if(!File.Exists("GameLauncherUpdater.exe")) {
				try {
					File.WriteAllBytes("GameLauncherUpdater.exe", new WebClientWithTimeout().DownloadData("http://launcher.soapboxrace.world/GameLauncherUpdater.exe"));
                } catch { /* ignored */ }
            }

            if (!File.Exists("servers.json")) {
                try {
                    File.WriteAllText("servers.json", "[]");
                } catch { /* ignored */ }
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            if (Debugger.IsAttached) {
                ServerProxy.Instance.Start();
                Application.Run(new MainScreen(SplashScreen2));
            } else {
                if (NFSW.isNFSWRunning()) {
                    MessageBox.Show(null, "An instance of Need for Speed: World is already running", "GameLauncher", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    Process.GetProcessById(Process.GetCurrentProcess().Id).Kill();
                }

                var mutex = new Mutex(false, "GameLauncherNFSW-MeTonaTOR");
                try {
                    if (mutex.WaitOne(0, false)) {
                        string[] files = {
                            "Newtonsoft.Json.dll",
                            "INIFileParser.dll",
                            "Microsoft.WindowsAPICodePack.dll",
                            "Microsoft.WindowsAPICodePack.Shell.dll",
                            "Flurl.dll",
                            "Flurl.Http.dll",
                        };

                        var missingfiles = new List<string>();

                        foreach (var file in files) {
                            if (!File.Exists(file)) {
                                missingfiles.Add(file);
                            }
                        }

                        if (missingfiles.Count != 0) {
                            var message = "Cannot launch GameLauncher. The following files are missing:\n\n";

                            foreach (var file in missingfiles) {
                                message += "• " + file + "\n";
                            }

                            MessageBox.Show(null, message, "GameLauncher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        } else { 
                            ServerProxy.Instance.Start();
                            Application.Run(new MainScreen(SplashScreen2));
                        }
                    } else {
                        MessageBox.Show(null, "An instance of the application is already running.", "GameLauncher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                } finally {
                    mutex.Close();
                    mutex = null;
                }
            }
        }
    }
}
