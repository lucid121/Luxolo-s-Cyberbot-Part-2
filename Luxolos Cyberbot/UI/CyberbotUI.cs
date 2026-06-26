using System;
using System.Threading;

namespace Luxolo_s_Cyberbot.UI
{
    internal class CyberbotUI
    {
        // Prints coloured text without moving to next line .
        public static void Write(string text, ConsoleColor colour)
        {
            Console.ForegroundColor = colour;
            Console.Write(text);
            Console.ResetColor();
        }

        // Prints coloured text then moves to next line.
        public static void WriteLine(string text, ConsoleColor colour)
        {
            Console.ForegroundColor = colour;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        // Prints text one character at a time, like typing.
        public static void TypeAnimator(string text, ConsoleColor colour = ConsoleColor.White, int delayMs = 18)
        {
            Console.ForegroundColor = colour;

            // Each character appears with a small delay.
            foreach (char c in text)
            {
                Console.Write(c);
                Thread.Sleep(delayMs);
            }

            Console.WriteLine();
            Console.ResetColor();
        }

        // Prints a full-width line of repeated characters.
        public static void PrintDivider(char symbol = '-', int width = 60)
        {
            WriteLine(new string(symbol, width), ConsoleColor.DarkCyan);
        }

        // Prints an empty line for spacing.
        public static void PrintBlankLine() => Console.WriteLine();

        // Prints the green "CyberBot |" label.
        public static void PrintBotLabel()
        {
            Write("  CyberBot  ", ConsoleColor.Green);
            Write("| ", ConsoleColor.DarkGray);
        }

        // Prints the user's name as their chat label.
        public static void PrintUserPrompt(string name)
        {
            PrintBlankLine();
            Write($"  {name,-12}", ConsoleColor.Yellow);
            Write("| ", ConsoleColor.DarkGray);
        }

        // Bot label + animated message printed together.
        public static void BotSay(string message, ConsoleColor colour = ConsoleColor.White)
        {
            PrintBotLabel();
            TypeAnimator(message, colour);
        }

        // Prints a titled section with dividers above and below.
        public static void PrintSectionHeader(string title)
        {
            PrintBlankLine();
            PrintDivider('-');
            WriteLine($"  >> {title}", ConsoleColor.Magenta);
            PrintDivider('-');
        }

        // Clears screen then draws the ASCII art logo.
        public static void PrintLogo()
        {
            Console.Clear();
            PrintBlankLine();

            // Each string is one row of the logo.
            string[] logo =
            {
                @"  ██████╗██╗   ██╗██████╗ ███████╗██████╗  ",
                @" ██╔════╝╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗ ",
                @" ██║      ╚████╔╝ ██████╔╝█████╗  ██████╔╝ ",
                @" ██║       ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗ ",
                @" ╚██████╗   ██║   ██████╔╝███████╗██║  ██║ ",
                @"  ╚═════╝   ╚═╝   ╚═════╝ ╚══════╝╚═╝  ╚═╝ ",
                @"                                             ",
                @"  Your Cybersecurity Awareness Assistant      ",
            };

            // Prints each logo line in cyan colour.
            foreach (string line in logo)
                WriteLine(line, ConsoleColor.Cyan);

            PrintBlankLine();
            PrintDivider('=');
            WriteLine("  Protecting South African citizens — one conversation at a time.",
                      ConsoleColor.DarkCyan);
            PrintDivider('=');
            PrintBlankLine();
        }
    }
}