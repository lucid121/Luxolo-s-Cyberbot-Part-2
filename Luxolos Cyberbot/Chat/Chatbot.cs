using System;
using Luxolo_s_Cyberbot.Media;
using Luxolo_s_Cyberbot.UI;

namespace Luxolo_s_Cyberbot.Chat
{
    internal class ChatBot
    {
        // Creates the response brain once at startup.
        private readonly ResponseEngine _engine = new ResponseEngine();

        // Stores the user's name for personalised replies.
        private string _userName = "Friend";

        // Runs the whole chatbot from start to finish.
        public void Run()
        {
            ShowWelcomeScreen();
            CollectUserName();
            RunConversationLoop();
            ShowFarewell();
        }

        // Shows logo, plays sound, prints welcome messages.
        private static void ShowWelcomeScreen()
        {
            CyberbotUI.PrintLogo();

            // Builds the full path to the greeting sound file.
            string greetingPath = Path.Combine(
                AppContext.BaseDirectory, "Assets", "greeting.wav");

            // Plays the greeting audio file.
            AudioPlayer.PlayGreeting(greetingPath);

            CyberbotUI.BotSay("Welcome to the Cybersecurity Awareness Assistant!");
            CyberbotUI.BotSay("I'm here to help you stay safe in the digital world.");
            CyberbotUI.PrintBlankLine();
        }

        // Asks for the user's name, keeps asking until valid.
        private void CollectUserName()
        {
            CyberbotUI.PrintSectionHeader("Getting to know you");

            while (true)
            {
                CyberbotUI.BotSay("Before we begin, what is your name?");
                CyberbotUI.PrintUserPrompt("You");

                // Reads input and removes extra spaces.
                string input = Console.ReadLine()?.Trim() ?? string.Empty;

                // If blank, warn user and ask again.
                if (string.IsNullOrWhiteSpace(input))
                {
                    CyberbotUI.BotSay("I need a name to personalise our chat. Please try again.",
                                      ConsoleColor.Yellow);
                    continue;
                }

                // Saves name with only first letter capitalised.
                _userName = char.ToUpper(input[0]) + input.Substring(1).ToLower();
                break;
            }

            CyberbotUI.PrintBlankLine();
            CyberbotUI.BotSay($"Great to meet you, {_userName}!", ConsoleColor.Green);
            CyberbotUI.BotSay("Type a question or topic below. Type 'exit' to quit.");
            CyberbotUI.PrintBlankLine();
        }

        // Keeps chatting with the user until they exit.
        private void RunConversationLoop()
        {
            CyberbotUI.PrintSectionHeader("Cybersecurity Chat");

            while (true)
            {
                CyberbotUI.PrintUserPrompt(_userName);

                // Reads what the user typed.
                string input = Console.ReadLine()?.Trim() ?? string.Empty;

                // Stops the loop if user wants to leave.
                if (ResponseEngine.IsExitCommand(input))
                    break;

                // Gets the matching reply from the engine.
                string response = _engine.GetResponse(input);
                CyberbotUI.PrintBlankLine();
                CyberbotUI.BotSay(response, ConsoleColor.White);
                CyberbotUI.PrintBlankLine();
                CyberbotUI.PrintDivider();
            }
        }

        // Prints a goodbye message with safety reminders.
        private void ShowFarewell()
        {
            CyberbotUI.PrintBlankLine();
            CyberbotUI.BotSay($"Stay safe online, {_userName}! Remember:", ConsoleColor.Cyan);
            CyberbotUI.BotSay("  Use strong passwords   Enable 2FA   Browse safely",
                              ConsoleColor.Green);
            CyberbotUI.PrintBlankLine();
            CyberbotUI.PrintDivider('=');
            CyberbotUI.WriteLine("  Session ended. Goodbye!", ConsoleColor.DarkCyan);
            CyberbotUI.PrintDivider('=');
            CyberbotUI.PrintBlankLine();
        }
    }
}
