using System;
using System.IO;
using System.Media;
using System.Runtime.Versioning;

namespace Luxolo_s_Cyberbot.Media
{
    internal class AudioPlayer
    {
        
        public static void PlayGreeting(string filePath)
        {
            if (!OperatingSystem.IsWindows())
                return;

            if (!File.Exists(filePath))
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"  [Audio] greeting.wav not found at: {filePath}");
                Console.ResetColor();
                return;
            }

            try
            {
                using (SoundPlayer player = new SoundPlayer(filePath))
                {
                    player.PlaySync(); // blocks until audio finishes
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"  [Audio] Could not play greeting: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}