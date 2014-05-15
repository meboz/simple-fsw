using System;
using System.IO;

namespace FileWatcher
{
	class Program
	{
		static void Main(string[] args)
		{
			Watch("c:\\temp\\images");
			Console.ReadKey();
		}

		private static void Watch(string images)
		{
			Log("Watching: " + images);

			var fsw = new FileSystemWatcher {Path = images};
			fsw.Created += SkuDirectoryCreated;
			fsw.EnableRaisingEvents = true;
			var skuDirectories = new DirectoryInfo(images);

			Log(string.Format("{0} sku directories found", skuDirectories.GetDirectories().Length));
			
			foreach (var skuDirectory in skuDirectories.GetDirectories())
			{ 
				//Log(string.Format("Watching {0}", string.Format(skuDirectory.Name)));
				WatchSkuDirectory(skuDirectory);
			}
		}

		private static void SkuDirectoryCreated(object sender, FileSystemEventArgs e)
		{
			Log(string.Format("[{0}: {1}] Sku directory created: ", e.ChangeType, e.Name) + e.FullPath);
			WatchSkuDirectory(new DirectoryInfo(e.FullPath));
		}

		private static void WatchSkuDirectory(DirectoryInfo skuDir)
		{
			Log("Watching " + skuDir.Name);
			WatchSkuForNewOriginals(skuDir);
			WatchSkuForUpdatedOriginals(skuDir);
		}

		private static void WatchSkuForUpdatedOriginals(DirectoryInfo skuDir)
		{
			Log(string.Format("Watching '{0}' for updated originals", skuDir));

			var updatedOriginalWatcher = BuildOriginalWatcher(skuDir);
			updatedOriginalWatcher.Changed += GenerateVariantsForUpdatedOriginal;
		}

		private static void GenerateVariantsForUpdatedOriginal(object sender, FileSystemEventArgs e)
		{
			Log("Updating variant for " + e.FullPath);
		}

		private static void WatchSkuForNewOriginals(DirectoryInfo skuDir)
		{
			Log(string.Format("Watching '{0}' for new originals", skuDir));
			
			var newOriginalWatcher = BuildOriginalWatcher(skuDir);

			newOriginalWatcher.Created += GenerateSkuImageVariants;
		}

		private static FileSystemWatcher BuildOriginalWatcher(DirectoryInfo skuDir)
		{
			return new FileSystemWatcher(skuDir.FullName)
				{
					EnableRaisingEvents = true,
					IncludeSubdirectories = false,
					//Filter = "*.jpg"
				};
		}

		private static void GenerateSkuImageVariants(object sender, FileSystemEventArgs e)
		{
			LogActivity("Generating variant for " + e.FullPath, e.FullPath);
		}

		private static void Log(string msg)
		{
			Console.WriteLine(msg);
		}
		
		private static void LogActivity(string msg, string skuOriginalFile)
		{
			Log(msg);
			var skuDirectory = new FileInfo(skuOriginalFile).Directory;

			var activityLog = Path.Combine(skuDirectory.FullName, "activity.log");

			File.AppendAllText(activityLog,msg + "\n");
		}
	}
}
