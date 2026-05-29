using System;
using System.Diagnostics;
using System.IO;

namespace Luxolo_s_Cyberbot
{
    class Program
    {
        static void Main(string[] args)
        {
            // So its starts with Gui
            string StartupName = "Luxolo_s_Cyberbot.exe";
            string WhereIam = AppDomain.CurrentDomain.BaseDirectory;

            // Search for the GUI 
            string FindStart = FindGuiExecutable(WhereIam, StartupName);

            if (FindStart != null)
            {
                try
                {
                    // Launch GUI
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = FindStart,
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(FindStart)
                    });
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[-] Error starting CyberBot GUI: {ex.Message}");
                    Console.ReadKey();
                }
            }
            else
            {
                // Message if the GUI hasn't been compiled yet
                Console.Title = "CyberBot Launcher Console ";
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("=========================================================");
                Console.WriteLine("            🔒 CYBERBOT AUTO-LAUNCH ENGINE               ");
                Console.WriteLine("=========================================================");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[-] Could not locate: {StartupName}");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("\n[!] Please make sure to Build the entire Solution first:");
                Console.WriteLine("    1. Right-click the Solution 'Luxolo's Cyberbot' on the right panel.");
                Console.WriteLine("    2. Click 'Build Solution'.");
                Console.WriteLine("    3. Run this again.");
                Console.WriteLine("\nPress any key to close...");
                Console.ReadKey();
            }
        }


        private static string FindGuiExecutable(string startDir, string fileName)
        {
            // 1. Direct check in current folder
            string directPath = Path.Combine(startDir, fileName);
            if (File.Exists(directPath)) return directPath;

            // 2. Traversal check up to sibling solution folders (covers Debug/Release build folder structures)
            DirectoryInfo dir = new DirectoryInfo(startDir);
            for (int i = 0; i < 5 && dir != null; i++)
            {
                try
                {
                    // Check current level for the executable
                    var files = dir.GetFiles(fileName, SearchOption.AllDirectories);
                    if (files.Length > 0)
                    {
                        return files[0].FullName;
                    }
                }
                catch
                {
                    // Skip unauthorized folders or file access locks
                }
                dir = dir.Parent;
            }

            return null;
        }
    }
}