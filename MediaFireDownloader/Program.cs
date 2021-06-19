using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace MediaFireDownloader
{
    class Program
    {
        static int idx = 0, errors = 0, downloaded = 0, skipped = 0;
        static List<FileEntry> files;
        static SHA256 SHA256 = SHA256.Create();

        static void Main(string[] args)
        {
            FolderEntry root;

            switch (args.Length)
            {
                default:
                    Console.WriteLine("Use MediaFireDownloader <folder key> [destination path]".Normalize());
                    Console.ReadLine();
                    return;
                case 1:
                    root = new FolderEntry(args[0]);
                    break;
                case 2:
                    root = new FolderEntry(args[0]);
                    root.Destination = Path.GetFullPath(args[1]);
                    break;
            }

            WriteLine("inf", "File info:");
            Console.WriteLine("  Name: " + root.Name);
            Console.WriteLine("  Destination: " + root.Destination);
            WriteLine("inf", "Press Enter for exit");
            WriteLine("inf", "Indexing...");
            files = new List<FileEntry>();
            GetFiles(root, in files);

            WriteLine("inf", "Downloading...");
            DownloadFile(files[idx], DownloadLoop);

            Console.ReadLine();
        }

        public static void DownloadLoop()
        {
            if (++idx >= files.Count)
            {
                WriteLine("inf", "Done");
                Console.WriteLine("  Downloaded files: " + downloaded);
                Console.WriteLine("  Skipped files: " + skipped);
                Console.WriteLine("  Errors: " + errors);
                return;
            }
            DownloadFile(files[idx], DownloadLoop);
        }

        public static void GetFiles(FolderEntry entry, in List<FileEntry> collection)
        {
            foreach (var file in entry.GetFiles())
            {
                WriteLine("add", file.Destination + "/" + file.Name);
                collection.Add(file);
            }

            foreach (var folder in entry.GetFolders())
            {
                GetFiles(folder, collection);
            }
        }

        public static void DownloadFile(FileEntry entry, Action onCompleated)
        {
            var wc = new WebClient();
            if (!Directory.Exists(entry.Destination))
                Directory.CreateDirectory(entry.Destination);
            var fileName = entry.Destination + "/" + entry.Name;

            if (File.Exists(fileName))
            {
                string fileHash;
                using (var stream = File.OpenRead(fileName))
                    fileHash = string.Join("", SHA256.ComputeHash(stream).Select(b => b.ToString("x2")));

                if (entry.Hash == fileHash)
                {
                    skipped++;
                    WriteLine("skp", fileName);
                    if (onCompleated != null)
                        onCompleated();
                    return;
                }
            }

            try
            {
                var downloadLink = entry.GetDownloadLink();

                WriteLine("dwnload", fileName);
                Console.WriteLine("  Source: " + downloadLink);

                var cursorTop = Console.CursorTop;
                var isConsoleLocked = false;

                void DownloadProgressChangedHandler(int ProgressPercentage, ulong BytesReceived)
                {
                    if (isConsoleLocked)
                        return;
                    isConsoleLocked = true;
                    var loadedWidth = (int)Math.Round(ProgressPercentage / 10d);
                    Console.SetCursorPosition(0, cursorTop);
                    var str = $"[{new string('#', loadedWidth)}{new string(' ', 10 - loadedWidth)}] {ProgressPercentage}% ({BytesReceived} B / {entry.Size} B)";
                    Console.Write(str);
                    isConsoleLocked = false;
                };

                DownloadProgressChangedHandler(0, 0);
                wc.DownloadProgressChanged += (s, e) => DownloadProgressChangedHandler(e.ProgressPercentage, (ulong)e.BytesReceived);
                wc.DownloadFileCompleted += (s, e) =>
                {
                    downloaded++;
                    if (!e.Cancelled)
                        DownloadProgressChangedHandler(100, entry.Size);
                    else
                    {
                        Console.Write(" Error!");
                        errors++;
                    }

                    Console.WriteLine();
                    if (onCompleated != null)
                        onCompleated();
                    wc.Dispose();
                };
                wc.DownloadFileAsync(new Uri(downloadLink), fileName);
            } catch
            {
                WriteLine("error", fileName);
                errors++;
                if (onCompleated != null)
                    onCompleated();
            }
        }

        public static void WriteLine(string prefix, string content)
        {
            if (prefix.Length > 3)
                prefix = prefix.Substring(0, 3);
            Console.WriteLine($"[{prefix.ToUpper()}] {content}");
        }
    }
}
