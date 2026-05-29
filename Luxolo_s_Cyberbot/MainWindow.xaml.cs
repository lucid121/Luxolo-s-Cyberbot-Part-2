using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Luxolo_s_Cyberbot_GUI
{
    // Delegate used to handle bot messages and emotions (for sound + reactions)
    public delegate void BotActionDelegate(string message, string emotionState);

    public partial class MainWindow : Window
    {
        // Event that triggers bot response actions (sound, emotion feedback, etc.)
        public event BotActionDelegate BotResponded;



        // Stores user name
        private string _userName = "";

        // Checks if user name was already collected
        private bool _nameCollected = false;

        // Stores last topic user asked about
        private string _lastTopic = "";

        // Stores what user is interested in
        private string _userInterest = "";

        // Stores current detected emotion of user
        private string _userEmotion = "Neutral";

        // Counts total messages typed by user
        private int _totalInteractionCount = 0;

        // Placeholder text inside input box
        private readonly string _placeholder = "Ask about safe browsing, malware, encryption, scams...";

        // ── Main response dictionary (keywords → answers) ──
        // This is the core knowledge base of the bot
        private readonly Dictionary<string, string> _responses = new(StringComparer.OrdinalIgnoreCase)
        {
            ["phishing"] =
                "⚠️ Phishing is when criminals send fake emails/messages pretending to be your bank, cellular network, or a trusted agency.\n\n" +
                "🛡️ QUICK TIPS:\n" +
                "• Inspect the sender's full email address (look for spelling differences like @paypa1.com).\n" +
                "• Never click unexpected login links. Navigate directly to your service's official app or site.\n" +
                "• Legit institutions will never demand your PIN or password in an email link.",

            ["password"] =
                "🔐 Passwords are the keys to your entire digital identity.\n\n" +
                "🛡️ STRENGTHEN YOUR KEYS:\n" +
                "• Aim for 12+ characters combining uppercase, lowercase, numbers, and special symbols.\n" +
                "• Use passphrases (e.g., 'BlueRugGrass77!') — they're easier to remember but very strong.\n" +
                "• Never reuse passwords. One leak can expose all accounts.",

            ["privacy"] =
                "🛡️ Privacy protects your personal data from being collected or misused.\n\n" +
                "🛡️ ACTIONS TO TAKE:\n" +
                "• Set social media to private mode.\n" +
                "• Turn off location tracking when not needed.\n" +
                "• Avoid online quizzes that ask personal questions.",

            ["scam"] =
                "🚨 Scams trick users into giving money or personal info.\n\n" +
                "🛡️ DEFENSE PLAN:\n" +
                "• Always call your bank using official numbers.\n" +
                "• Never pay to claim prizes or winnings.",

            ["malware"] =
                "🦠 Malware is harmful software that damages or steals data.\n\n" +
                "🛡️ PREVENTION:\n" +
                "• Keep Windows updated.\n" +
                "• Use antivirus software.",

            ["ransomware"] =
                "💀 Ransomware locks your files and demands payment.\n\n" +
                "🛡️ PROTECTION:\n" +
                "• Backup files regularly.\n" +
                "• Use offline storage or cloud backup.",

            ["2fa"] =
                "🔑 Two-Factor Authentication adds an extra login step.\n\n" +
                "🛡️ BENEFIT:\n" +
                "• Even if password is stolen, account stays protected.",

            ["vpn"] =
                "🌐 VPN encrypts your internet connection.\n\n" +
                "🛡️ USE CASES:\n" +
                "• Safe browsing on public Wi-Fi.\n" +
                "• Avoid unknown free VPNs.",

            ["browsing"] =
                "🌍 Safe browsing helps protect your device.\n\n" +
                "🛡️ TIPS:\n" +
                "• Use HTTPS websites.\n" +
                "• Block unsafe ads.",

            ["social engineering"] =
                "🎭 Social engineering tricks humans instead of systems.\n\n" +
                "🛡️ PROTECTION:\n" +
                "• Do not rush decisions.\n" +
                "• Verify requests independently.",

            ["encryption"] =
                "🔏 Encryption protects data by making it unreadable without a key.\n\n" +
                "🛡️ EXAMPLE:\n" +
                "• WhatsApp messages are encrypted end-to-end.",

            ["cyberbullying"] =
                "🤝 Cyberbullying is online harassment.\n\n" +
                "🛡️ RESPONSE:\n" +
                "• Block and report users.\n" +
                "• Save evidence.",

            ["identity theft"] =
                "👤 Identity theft is when someone uses your personal details illegally.\n\n" +
                "🛡️ PROTECTION:\n" +
                "• Shred documents.\n" +
                "• Monitor bank activity.",

            ["firewall"] =
                "🧱 A firewall blocks unwanted network access.\n\n" +
                "🛡️ ACTION:\n" +
                "• Keep firewall turned ON.",

            ["cookies"] =
                "🍪 Cookies store small data about your browsing.\n\n" +
                "🛡️ TIP:\n" +
                "• Clear cookies regularly."
        };

        // Tips shown when user asks about phishing
        private readonly List<string> _phishingTips = new()
        {
            "📧 Tip: Be careful of emails asking for urgent action.",
            "🔍 Tip: Hover over links before clicking them.",
            "📌 Tip: Banks never ask for passwords via email.",
            "🛑 Tip: Scammers use urgency to trick users.",
            "⚠️ Tip: SMS scams are very common."
        };

        // Bot welcome messages shown on startup
        private readonly List<string> _botIntroductions = new()
        {
            "Welcome! What is your name?",
            "Hello! Let's start with your name.",
            "System ready. Tell me your name."
        };

        // Responses based on user emotions
        private readonly List<string> _acknowledgements = new()
        {
            "Good question. Here is the explanation: ",
            "Let's break it down: ",
            "Here is what you need to know: ",
            "Important point: ",
            "Let’s look at this carefully: "
        };

        // Emotion-based replies
        private readonly Dictionary<string, string> _sentimentResponses = new(StringComparer.OrdinalIgnoreCase)
        {
            ["worried"] = "Feeling worried is normal. Let's improve your security together.",
            ["scared"] = "Don't worry, we will make your system safer.",
            ["confused"] = "I will explain everything in simple steps.",
            ["frustrated"] = "Let's simplify things step by step.",
            ["curious"] = "Great! Curiosity helps you learn cybersecurity."
        };

        // Random generator for messages
        private readonly Random _random = new();

        public MainWindow()
        {
            InitializeComponent();

            // Connect event to sound handler
            BotResponded += HandleVoiceAndSounds;

            // Run when window opens
            Loaded += OnWindowLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Start bot greeting
            BotResponded?.Invoke("Start Greeting", "Welcome");

            string initialGreeting = _botIntroductions[_random.Next(_botIntroductions.Count)];

            AddBotMessage("CyberBot started successfully!");
            AddBotMessage(initialGreeting);

            UpdateMemoryUI();
            UserInput.Focus();
        }

        // Handles sound or system feedback based on emotion
        private void HandleVoiceAndSounds(string message, string emotionState)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;

                // Possible locations of sound file
                string[] possiblePaths = new[]
                {
                    Path.Combine(baseDir, "Assets", "greeting.wav"),
                    Path.Combine(baseDir, "..", "..", "..", "Assets", "greeting.wav"),
                    Path.Combine(baseDir, "..", "..", "..", "..", "Luxolo's Cyberbot", "Assets", "greeting.wav")
                };

                string path = possiblePaths.FirstOrDefault(File.Exists);

                if (path != null)
                {
                    using SoundPlayer player = new(path);
                    player.Play();
                }
                else
                {
                    // Default system sounds if file not found
                    if (emotionState == "worried" || emotionState == "scared")
                        SystemSounds.Exclamation.Play();
                    else
                        SystemSounds.Asterisk.Play();
                }
            }
            catch { /* prevent crashes */ }
        }

        private void ProcessUserInput()
        {
            string input = UserInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(input) || input == _placeholder)
                return;

            UserInput.Text = "";

            _totalInteractionCount++;
            AddUserMessage(input);

            // Step 1: Ask for name first
            if (!_nameCollected)
            {
                if (input.Length < 2 || input.Any(char.IsDigit))
                {
                    AddBotMessage("Please enter a valid name.");
                    return;
                }

                _userName = char.ToUpper(input[0]) + input.Substring(1).ToLower();
                _nameCollected = true;

                AddBotMessage($"Nice to meet you {_userName}!");
                UpdateMemoryUI();
                return;
            }

            // Step 2: Exit commands
            if (IsExitCommand(input))
            {
                AddBotMessage($"Goodbye {_userName}!");
                SendButton.IsEnabled = false;
                UserInput.IsEnabled = false;
                return;
            }

            // Step 3: Detect emotion in text
            string detectedEmotion = null;
            foreach (var emotion in _sentimentResponses)
            {
                if (input.Contains(emotion.Key, StringComparison.OrdinalIgnoreCase))
                {
                    detectedEmotion = emotion.Key;
                    _userEmotion = emotion.Key;
                    AddBotMessage(emotion.Value);
                    BotResponded?.Invoke(emotion.Value, detectedEmotion);
                    break;
                }
            }

            // Step 4: Follow-up check
            if (IsFollowUp(input))
            {
                if (!string.IsNullOrEmpty(_lastTopic) && _responses.ContainsKey(_lastTopic))
                {
                    AddBotMessage(_responses[_lastTopic]);
                }
                return;
            }

            // Step 5: Phishing tips request
            if (input.Contains("tip", StringComparison.OrdinalIgnoreCase) &&
                input.Contains("phish", StringComparison.OrdinalIgnoreCase))
            {
                AddBotMessage(_phishingTips[_random.Next(_phishingTips.Count)]);
                _lastTopic = "phishing";
                return;
            }

            // Step 6: Detect keyword
            string topicMatched = FindKeyword(input);
            if (topicMatched != null)
            {
                _lastTopic = topicMatched;
                AddBotMessage(_responses[topicMatched]);
                UpdateMemoryUI();
                return;
            }

            // Step 7: Default response
            AddBotMessage("Try asking about phishing, malware, VPN, or passwords.");
            UpdateMemoryUI();
        }

        // Find keyword in user input
        private string FindKeyword(string input)
        {
            foreach (var key in _responses.Keys)
            {
                if (input.Contains(key, StringComparison.OrdinalIgnoreCase))
                    return key;
            }
            return null;
        }

        // Check if user is continuing conversation
        private static bool IsFollowUp(string input)
        {
            string[] followUps = { "tell me more", "explain more", "continue", "expand" };
            return followUps.Any(phrase => input.Contains(phrase, StringComparison.OrdinalIgnoreCase));
        }

        // Check if user wants to exit
        private static bool IsExitCommand(string input)
        {
            string[] exits = { "exit", "quit", "bye" };
            return exits.Any(phrase => input.Equals(phrase, StringComparison.OrdinalIgnoreCase));
        }

        // Update UI memory display
        private void UpdateMemoryUI()
        {
            TxtMemName.Text = string.IsNullOrEmpty(_userName) ? "Unknown" : _userName;
            TxtMemTopic.Text = string.IsNullOrEmpty(_lastTopic) ? "None" : _lastTopic;
            TxtMemInterest.Text = string.IsNullOrEmpty(_userInterest) ? "None" : _userInterest;
            TxtMemFeeling.Text = _userEmotion;
        }

        // Add user chat bubble
        private void AddUserMessage(string text)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(29, 78, 216)),
                CornerRadius = new CornerRadius(16, 16, 2, 16),
                Padding = new Thickness(16),
                Margin = new Thickness(120, 6, 10, 6),
                HorizontalAlignment = HorizontalAlignment.Right,
                MaxWidth = 550,
                Style = (Style)FindResource("AnimatedBubbleStyle")
            };

            border.Child = new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                FontSize = 14.5,
                TextWrapping = TextWrapping.Wrap
            };

            ChatPanel.Children.Add(border);
            ScrollToBottom();
        }

        // Add bot chat bubble
        private void AddBotMessage(string text)
        {
            var tag = new TextBlock
            {
                Text = "CYBERBOT",
                Foreground = Brushes.Green,
                FontSize = 10
            };

            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(22, 28, 41)),
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(16),
                Margin = new Thickness(10, 6, 120, 6),
                HorizontalAlignment = HorizontalAlignment.Left,
                MaxWidth = 580,
                Style = (Style)FindResource("AnimatedBubbleStyle")
            };

            border.Child = new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap
            };

            var blockContainer = new StackPanel();
            blockContainer.Children.Add(tag);
            blockContainer.Children.Add(border);

            ChatPanel.Children.Add(blockContainer);
            ScrollToBottom();
        }

        // Scroll chat to bottom
        private void ScrollToBottom()
        {
            Dispatcher.InvokeAsync(() =>
            {
                ChatScroller.ScrollToEnd();
            });
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessUserInput();
        }

        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessUserInput();
                e.Handled = true;
            }
        }

        // Remove placeholder when typing starts
        private void UserInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UserInput.Text == _placeholder)
            {
                UserInput.Text = "";
            }
        }

        // Restore placeholder when empty
        private void UserInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UserInput.Text))
            {
                UserInput.Text = _placeholder;
            }
        }

        // Clear chat window
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ChatPanel.Children.Clear();
            AddBotMessage("Chat cleared. Starting fresh.");
        }
    }
}