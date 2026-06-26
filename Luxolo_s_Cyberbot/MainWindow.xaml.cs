using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient; 
using System.IO;
using System.Linq;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Luxolo_s_Cyberbot_GUI
{
    public partial class MainWindow : Window
    {
        // =========================================================
        // 1. CLASS PROPERTIES, CONFIGURATION, & LOCAL DATA BACKUP
        // =========================================================

        // This is your SQL Express connection path. It targets the local SQLEXPRESS server.
        private readonly string ConnectionString = @"Server=.\SQLEXPRESS;Database=CyberbotDB;Trusted_Connection=True;TrustServerCertificate=True;";

        private string _userName = "";
        private bool _nameCollected = false;
        private string _lastTopic = "";
        private string _userEmotion = "Neutral";
        private readonly string _placeholder = "Ask about safe browsing, malware, encryption, scams...";

        // If your SQL Server is offline, this memory backup keeps the app running flawlessly.
        private List<CyberTask> _offlineTaskList = new List<CyberTask>();
        private List<ActivityLog> _activityLogs = new List<ActivityLog>();
        private bool _isShowingAllLogs = false; // Monitors "Show More" sidebar log toggle state.

        // General Q&A Bot Knowledge dictionary.
        private readonly Dictionary<string, string> _responses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["phishing"] = "⚠️ Phishing is when criminals send fake messages mimicking legitimate entities.\n\n🛡️ PREVENTION:\n- Inspect sender addresses closely.\n- Never click immediate log-in requests.",
            ["password"] = "🔐 Passwords protect vital credentials.\n\n🛡️ RULES:\n- Minimum 12 characters.\n- Use passphrase combos.\n- Never reuse on other accounts.",
            ["2fa"] = "🔑 Two-Factor Authentication secures user accounts even if passwords leak.",
            ["vpn"] = "🌐 VPN encrypts web traffic over public unsecure networks.",
            ["malware"] = "🦠 Malware covers trojans, viruses, and ransom tools.",
            ["scams"] = "🚨 Scams request immediate actions with artificial deadlines or fake rewards."
        };

        // Sentiment vocabulary mappings to change the bot's dynamic responses.
        private readonly Dictionary<string, string> _sentimentTriggers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["worried"] = "It's normal to feel concerned about safety. Let's configure defenses step-by-step.",
            ["scared"] = "Don't panic! Taking defensive steps makes you incredibly safe online.",
            ["confused"] = "No problem! Ask me to 'Start Quiz' or clarify complex topics anytime.",
            ["excited"] = "Outstanding! Cyber training leads to secure digital presence."
        };

        // =========================================================
        // 2. CYBER QUIZ VARIABLES & INNER CLASS
        // =========================================================
        private class QuizQuestion
        {
            public string QuestionText { get; set; }
            public string[] Options { get; set; }
            public char CorrectAnswerChar { get; set; }
            public string ExplanationText { get; set; }
        }

        private List<QuizQuestion> _quizQuestions = new List<QuizQuestion>();
        private int _currentQuizIndex = 0;
        private int _userQuizScore = 0;
        private bool _quizOptionSelected = false;

        // =========================================================
        // 3. STORAGE MODELS
        // =========================================================
        public class CyberTask
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime ReminderDate { get; set; }
            public bool IsCompleted { get; set; }
        }

        public class ActivityLog
        {
            public DateTime Timestamp { get; set; }
            public string ActionSummary { get; set; }
        }

        // =========================================================
        // 4. MAIN SYSTEM CONSTRUCTOR
        // =========================================================
        public MainWindow()
        {
            InitializeComponent();
            SetupQuizData();
            UserInput.Text = _placeholder;
            UserInput.Foreground = Brushes.Gray;

            // Connect SQL database tables on window launch
            InitializeLocalDatabase();
            RefreshTaskDisplay();
            LogActivity("Chatbot application initialized and running.");
        }

        // Sets up the 10 educational multiple-choice and true/false quiz questions.
        private void SetupQuizData()
        {
            _quizQuestions.Add(new QuizQuestion
            {
                QuestionText = "1. If you receive an urgent security update email with a login link, what should you do?",
                Options = new[] { "A) Click link immediately to prevent lock", "B) Delete and check through official site", "C) Forward to family members", "D) Send bank credentials directly" },
                CorrectAnswerChar = 'B',
                ExplanationText = "Institutions never send critical security warnings requiring instant link access. Always verify externally!"
            });
            _quizQuestions.Add(new QuizQuestion
            {
                QuestionText = "2. What makes a passphrase extremely safe but easy to recall?",
                Options = new[] { "A) Short sequences like '12345'", "B) Combination of 4 random words with symbols", "C) Your birth date", "D) Name of your pet" },
                CorrectAnswerChar = 'B',
                ExplanationText = "Four random words (e.g., 'CloudAppleStoneHorse!') create extreme complexity while remaining highly memorable."
            });
            _quizQuestions.Add(new QuizQuestion
            {
                QuestionText = "3. What is the fundamental goal of ransomware programs?",
                Options = new[] { "A) Give free storage space", "B) Scan files for speedups", "C) Lock user files and demand ransom", "D) Play media sound effects" },
                CorrectAnswerChar = 'C',
                ExplanationText = "Ransomware locks down directories using AES/RSA and forces victims to pay decryption keys to scammers."
            });
            _quizQuestions.Add(new QuizQuestion
            {
                QuestionText = "4. Public Wi-Fi connections are perfectly safe without any security layers.",
                Options = new[] { "A) True", "B) False" },
                CorrectAnswerChar = 'B',
                ExplanationText = "Public hot-spots are highly vulnerable to man-in-the-middle exploits. Always secure connections with a VPN."
            });
            _quizQuestions.Add(new QuizQuestion
            {
                QuestionText = "5. Which mechanism provides protective defense on active open connections?",
                Options = new[] { "A) Simple desktop wallpapers", "B) A robust network firewall", "C) Audio cards", "D) RAM cleanups" },
                CorrectAnswerChar = 'B',
                ExplanationText = "Firewalls monitor and filter incoming/outgoing traffic, rejecting unrecognized requests."
            });
            _quizQuestions.Add(new QuizQuestion
            {
                QuestionText = "6. Two-Factor Authentication (2FA) guarantees account security even when passwords are leaked.",
                Options = new[] { "A) True", "B) False" },
                CorrectAnswerChar = 'A',
                ExplanationText = "Even with credentials compromised, malicious access fails without the temporary hardware authorization token."
            });
            _quizQuestions.Add(new QuizQuestion
            {
                QuestionText = "7. What does the secure padlock or 'HTTPS' in the web address indicate?",
                Options = new[] { "A) Site cannot be copied", "B) Web traffic is securely encrypted", "C) The site has no ads", "D) Speed is unlimited" },
                CorrectAnswerChar = 'B',
                ExplanationText = "HTTPS ensures that intercepting actors cannot read input payloads passed between your browser and servers."
            });
            _quizQuestions.Add(new QuizQuestion
            {
                QuestionText = "8. What is the practice of manipulating people into revealing private keys?",
                Options = new[] { "A) Social Engineering", "B) SQL Injection", "C) Buffer Overflows", "D) Ransom attacks" },
                CorrectAnswerChar = 'A',
                ExplanationText = "Social engineering bypasses technologies by tricking human operators into leaking key system privileges."
            });
            _quizQuestions.Add(new QuizQuestion
            {
                QuestionText = "9. How often should crucial files be backed up?",
                Options = new[] { "A) Once every few years", "B) Never, computers are bulletproof", "C) Regularly on distinct disconnected units", "D) Only when file crash alerts prompt" },
                CorrectAnswerChar = 'C',
                ExplanationText = "Regular disconnected backups guarantee recovery from ransomware attacks without having to deal with cyber blackmailers."
            });
            _quizQuestions.Add(new QuizQuestion
            {
                QuestionText = "10. What are internet browser cookies mainly used for?",
                Options = new[] { "A) Charging data costs", "B) Storing user preference tokens and session states", "C) Mining crypto currencies", "D) Cleaning hardware fans" },
                CorrectAnswerChar = 'B',
                ExplanationText = "Cookies track preference selections and keep users authenticated safely within the individual browser sandbox."
            });
        }

        // =========================================================
        // 5. DATABASE OPERATIONS & SYNC (SQL EXPRESS)
        // =========================================================
        private void InitializeLocalDatabase()
        {
            try
            {
                // Connect to master database first to create 'CyberbotDB' database if it does not exist
                string masterConnection = @"Server=.\SQLEXPRESS;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";
                using (SqlConnection masterConn = new SqlConnection(masterConnection))
                {
                    masterConn.Open();
                    string dbCheck = "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'CyberbotDB') CREATE DATABASE CyberbotDB;";
                    using (SqlCommand dbCmd = new SqlCommand(dbCheck, masterConn))
                    {
                        dbCmd.ExecuteNonQuery();
                    }
                }

                // Connect to CyberbotDB database and configure tables
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string tableQuery = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CyberTasks')
                        BEGIN
                            CREATE TABLE CyberTasks (
                                Id INT IDENTITY(1,1) PRIMARY KEY,
                                Title NVARCHAR(150) NOT NULL,
                                Description NVARCHAR(MAX) NULL,
                                ReminderDate DATETIME NOT NULL,
                                IsCompleted BIT DEFAULT 0 NOT NULL
                            );
                        END;";

                    using (SqlCommand tblCmd = new SqlCommand(tableQuery, conn))
                    {
                        tblCmd.ExecuteNonQuery();
                    }
                }

                TxtDBStatus.Text = "Status: SQL Express Online ✔";
                TxtDBStatus.Foreground = Brushes.LightGreen;
                LogActivity("SQL Express Database successfully synced.");
            }
            catch (Exception ex)
            {
                // Soft local fallback: If database is offline, user is alerted cleanly and local mode triggers
                TxtDBStatus.Text = "Status: Local File-Engine Ready ⚙";
                TxtDBStatus.Foreground = Brushes.Yellow;
                LogActivity("SQL Connection failed. Switched to in-memory local fallback storage. Error details: " + ex.Message);
            }
        }

        private void LogActivity(string summary)
        {
            _activityLogs.Add(new ActivityLog
            {
                Timestamp = DateTime.Now,
                ActionSummary = summary
            });
            UpdateActivityUI();
        }

        private void UpdateActivityUI()
        {
            // Limit shown actions (5-10 logs by default to keep screen neat, shows all if toggled)
            if (_isShowingAllLogs)
            {
                LstActivityLog.ItemsSource = null;
                LstActivityLog.ItemsSource = _activityLogs.OrderByDescending(x => x.Timestamp).ToList();
                BtnToggleLogs.Content = "Show Less Log";
            }
            else
            {
                LstActivityLog.ItemsSource = null;
                LstActivityLog.ItemsSource = _activityLogs.OrderByDescending(x => x.Timestamp).Take(7).ToList();
                BtnToggleLogs.Content = "Show Full Log";
            }
        }

        // Add a Task to Database
        private void AddTaskToDB(string title, string desc, int daysOffset)
        {
            DateTime reminder = DateTime.Now.AddDays(daysOffset);

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string insertSql = "INSERT INTO CyberTasks (Title, Description, ReminderDate, IsCompleted) VALUES (@t, @d, @r, 0);";
                    using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@t", title);
                        cmd.Parameters.AddWithValue("@d", desc);
                        cmd.Parameters.AddWithValue("@r", reminder);
                        cmd.ExecuteNonQuery();
                    }
                }
                LogActivity($"Added DB Task: '{title}' with a reminder set for {daysOffset} days.");
            }
            catch
            {
                // Local program storage fallback
                var localTask = new CyberTask
                {
                    Id = _offlineTaskList.Count + 1,
                    Title = title,
                    Description = desc,
                    ReminderDate = reminder,
                    IsCompleted = false
                };
                _offlineTaskList.Add(localTask);
                LogActivity($"Saved Local Fallback Task: '{title}' for {daysOffset} days.");
            }

            RefreshTaskDisplay();
        }

        // Fetch all registered tasks from DB or backup memory
        private List<CyberTask> GetAllTasks()
        {
            var tasks = new List<CyberTask>();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string selectSql = "SELECT Id, Title, Description, ReminderDate, IsCompleted FROM CyberTasks ORDER BY IsCompleted ASC, ReminderDate ASC;";
                    using (SqlCommand cmd = new SqlCommand(selectSql, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tasks.Add(new CyberTask
                                {
                                    Id = reader.GetInt32(0),
                                    Title = reader.GetString(1),
                                    Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                    ReminderDate = reader.GetDateTime(3),
                                    IsCompleted = reader.GetBoolean(4)
                                });
                            }
                        }
                    }
                }
                return tasks;
            }
            catch
            {
                return _offlineTaskList.OrderBy(x => x.IsCompleted).ThenBy(x => x.ReminderDate).ToList();
            }
        }

        // Complete database tasks safely
        private void CompleteTask(int taskId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string updateSql = "UPDATE CyberTasks SET IsCompleted = 1 WHERE Id = @id;";
                    using (SqlCommand cmd = new SqlCommand(updateSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", taskId);
                        cmd.ExecuteNonQuery();
                    }
                }
                LogActivity($"Completed Database Task ID: {taskId}");
            }
            catch
            {
                var task = _offlineTaskList.FirstOrDefault(x => x.Id == taskId);
                if (task != null) task.IsCompleted = true;
                LogActivity($"Completed Local Task ID: {taskId}");
            }
            RefreshTaskDisplay();
        }

        // Drop/Delete a task
        private void DeleteTask(int taskId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string deleteSql = "DELETE FROM CyberTasks WHERE Id = @id;";
                    using (SqlCommand cmd = new SqlCommand(deleteSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", taskId);
                        cmd.ExecuteNonQuery();
                    }
                }
                LogActivity($"Deleted DB Task ID: {taskId}");
            }
            catch
            {
                var task = _offlineTaskList.FirstOrDefault(x => x.Id == taskId);
                if (task != null) _offlineTaskList.Remove(task);
                LogActivity($"Deleted Local Task ID: {taskId}");
            }
            RefreshTaskDisplay();
        }

        // Rebuilds UI list cards dynamically
        private void RefreshTaskDisplay()
        {
            PnlTaskList.Children.Clear();
            var list = GetAllTasks();

            if (list.Count == 0)
            {
                var lblEmpty = new TextBlock
                {
                    Text = "No active reminders stored.",
                    Foreground = Brushes.Gray,
                    FontStyle = FontStyles.Italic,
                    FontSize = 11,
                    Margin = new Thickness(0, 10, 0, 0)
                };
                PnlTaskList.Children.Add(lblEmpty);
                return;
            }

            foreach (var item in list)
            {
                var card = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(10),
                    Margin = new Thickness(0, 0, 0, 8),
                    BorderBrush = item.IsCompleted ? Brushes.DimGray : new SolidColorBrush(Color.FromRgb(99, 102, 241)),
                    BorderThickness = new Thickness(1)
                };

                var layoutStack = new StackPanel();

                // Header
                var titleText = new TextBlock
                {
                    Text = item.Title,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold, // FIXED: Changed from FontWeight.Bold to plural FontWeights.Bold
                    FontSize = 12,
                    TextDecorations = item.IsCompleted ? TextDecorations.Strikethrough : null
                };
                layoutStack.Children.Add(titleText);

                // Description
                if (!string.IsNullOrEmpty(item.Description))
                {
                    var descText = new TextBlock
                    {
                        Text = item.Description,
                        Foreground = Brushes.LightGray,
                        FontSize = 11,
                        Margin = new Thickness(0, 4, 0, 4),
                        TextWrapping = TextWrapping.Wrap
                    };
                    layoutStack.Children.Add(descText);
                }

                // Date Time Remaining Badge
                var timeText = new TextBlock
                {
                    Text = $"🔔 Remind on: {item.ReminderDate:yyyy-MM-dd}",
                    Foreground = new SolidColorBrush(Color.FromRgb(245, 158, 11)),
                    FontSize = 10,
                    FontWeight = FontWeights.SemiBold, // FIXED: Changed from FontWeight.SemiBold to FontWeights.SemiBold
                    Margin = new Thickness(0, 2, 0, 6)
                };
                layoutStack.Children.Add(timeText);

                // Completion Grid Controls
                var buttonsGrid = new Grid();
                buttonsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                buttonsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                if (!item.IsCompleted)
                {
                    var btnDone = new Button
                    {
                        Content = "✓ Complete",
                        Height = 22,
                        Background = new SolidColorBrush(Color.FromRgb(34, 197, 94)),
                        Foreground = Brushes.White,
                        BorderThickness = new Thickness(0), // FIXED: Changed from int '0' to standard 'new Thickness(0)'
                        Margin = new Thickness(0, 0, 4, 0),
                        FontSize = 10,
                        FontWeight = FontWeights.Bold, // FIXED: Changed to FontWeights.Bold
                        Tag = item.Id,
                        Cursor = Cursors.Hand
                    };
                    btnDone.Click += (s, e) => {
                        int id = (int)((Button)s).Tag;
                        CompleteTask(id);
                    };
                    Grid.SetColumn(btnDone, 0);
                    buttonsGrid.Children.Add(btnDone);
                }

                var btnDel = new Button
                {
                    Content = "🗑 Delete",
                    Height = 22,
                    Background = new SolidColorBrush(Color.FromRgb(239, 68, 68)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0), // FIXED: Changed from int '0' to standard 'new Thickness(0)'
                    Margin = new Thickness(4, 0, 0, 0),
                    FontSize = 10,
                    FontWeight = FontWeights.Bold, // FIXED: Changed to FontWeights.Bold
                    Tag = item.Id,
                    Cursor = Cursors.Hand
                };
                btnDel.Click += (s, e) => {
                    int id = (int)((Button)s).Tag;
                    DeleteTask(id);
                };
                Grid.SetColumn(btnDel, 1);
                buttonsGrid.Children.Add(btnDel);

                layoutStack.Children.Add(buttonsGrid);
                card.Child = layoutStack;
                PnlTaskList.Children.Add(card);
            }
        }

        // =========================================================
        // 6. NATURAL LANGUAGE PROCESSING (NLP) SIMULATION
        // =========================================================
        private void AnalyzeAndRespond(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText)) return;

            // Instantly clear startup instructions placeholder
            if (PnlChatHint.Visibility == Visibility.Visible)
                PnlChatHint.Visibility = Visibility.Collapsed;

            _userName = TxtMemName.Text;

            // 6.1 GREET USER AND REQUIRE USERNAME FIRST (Task 1 Context Requirement)
            if (!_nameCollected)
            {
                if (rawText.Length < 2 || Regex.IsMatch(rawText, @"\d"))
                {
                    AddBotMessage("To deliver customized security reports, could you please provide your name first?");
                    return;
                }
                _userName = Regex.Replace(rawText, @"\b\w", m => m.Value.ToUpper());
                _nameCollected = true;
                TxtMemName.Text = _userName;
                AddBotMessage($"Welcome, Officer {_userName}! I have loaded your security database workspace. Ask me any cyber questions, set reminders, or say 'Start Quiz' to begin.");
                LogActivity($"User registered name as: '{_userName}'");
                return;
            }

            // 6.2 TRIGGER REMOVALS / EXIT COMMANDS
            if (rawText.Equals("exit", StringComparison.OrdinalIgnoreCase) || rawText.Equals("bye", StringComparison.OrdinalIgnoreCase))
            {
                AddBotMessage($"Signing off. Stay secure online, {_userName}!");
                UserInput.IsEnabled = false;
                SendButton.IsEnabled = false;
                LogActivity("Chat session closed by user request.");
                return;
            }

            // 6.3 NATURAL LANGUAGE ACTIVITY LOG TRIGGERS ('show activity log' / 'what have you done')
            if (Regex.IsMatch(rawText, @"\b(activity|log|what have you done|history|summary)\b", RegexOptions.IgnoreCase))
            {
                var recentLogs = _activityLogs.OrderByDescending(x => x.Timestamp).Take(5).ToList();
                string outputStr = "Here is a summary of recent bot actions:\n";
                int counter = 1;
                foreach (var log in recentLogs)
                {
                    outputStr += $"\n{counter}. [{log.Timestamp:HH:mm}] {log.ActionSummary}";
                    counter++;
                }
                AddBotMessage(outputStr);
                LogActivity("Activity Log displayed inside conversation.");
                return;
            }

            // 6.4 NLP REMINDER MATCHING (Regex and String Contains Matching)
            // Evaluates phrase parameters automatically (e.g. 'Remind me to update password tomorrow')
            Match remindMatch = Regex.Match(rawText, @"\b(remind|task|add task|schedule|todo)\b\s+(?:me\s+)?(?:to\s+)?(.+?)(?:\s+(?:in|on|tomorrow|next week|day[s]?))?$", RegexOptions.IgnoreCase);
            if (remindMatch.Success)
            {
                string parsedTask = remindMatch.Groups[2].Value.Trim();

                // Days until trigger
                int daysOffset = 3;

                if (rawText.Contains("tomorrow", StringComparison.OrdinalIgnoreCase))
                {
                    daysOffset = 1;
                }
                else if (rawText.Contains("today", StringComparison.OrdinalIgnoreCase))
                {
                    daysOffset = 0;
                }
                else if (rawText.Contains("week", StringComparison.OrdinalIgnoreCase))
                {
                    daysOffset = 7;
                }
                else
                {
                    Match digitMatch = Regex.Match(rawText, @"\b(\d+)\s+day", RegexOptions.IgnoreCase);
                    if (digitMatch.Success)
                    {
                        int.TryParse(digitMatch.Value, out daysOffset);
                    }
                }

                if (parsedTask.Length > 60)
                {
                    parsedTask = parsedTask.Substring(0, 57) + "...";
                }

                if (string.IsNullOrEmpty(parsedTask))
                {
                    parsedTask = "Default Cybersecurity Health Check";
                }

                AddTaskToDB(parsedTask, "Task generated automatically via NLP chatbot prompt.", daysOffset);
                AddBotMessage($"Roger that! Task registered: '{parsedTask}'. Set reminder trigger in {daysOffset} days. Your database is synchronized.");
                return;
            }

            // 6.5 QUIZ TRIGGER PHRASES ('start quiz', 'play game')
            if (Regex.IsMatch(rawText, @"\b(quiz|game|play|trivia|question)\b", RegexOptions.IgnoreCase))
            {
                LaunchQuizSession();
                return;
            }

            // 6.6 EMOTION SENTIMENT SCANNER
            string sentimentMatch = "Neutral";
            foreach (var key in _sentimentTriggers.Keys)
            {
                if (rawText.Contains(key, StringComparison.OrdinalIgnoreCase))
                {
                    sentimentMatch = key;
                    _userEmotion = key;
                    TxtMemFeeling.Text = key;
                    AddBotMessage(_sentimentTriggers[key]);
                    LogActivity($"NLP Sentiment Classified: {key.ToUpper()}");
                    TriggerSystemAlertSound();
                    break;
                }
            }

            // 6.7 KNOWLEDGE DICTIONARY DETECTION
            bool foundResponse = false;
            foreach (var key in _responses.Keys)
            {
                if (rawText.Contains(key, StringComparison.OrdinalIgnoreCase))
                {
                    _lastTopic = key;
                    TxtMemTopic.Text = key;
                    AddBotMessage(_responses[key]);
                    LogActivity($"NLP Keywords matched: {key.ToUpper()} query answered.");
                    foundResponse = true;
                    break;
                }
            }

            // Suggest helper topics when query does not hit key markers
            if (!foundResponse && sentimentMatch == "Neutral")
            {
                AddBotMessage("I registered your query, but I want to make sure I give you correct security info. Try asking specifically about: 'phishing', 'passwords', '2FA', or say 'Start Quiz'!");
                LogActivity($"NLP Unmatched Query registered: '{rawText}'");
            }
        }

        // =========================================================
        // 7. TRIVIA QUIZ INTERACTION FUNCTIONS
        // =========================================================
        private void LaunchQuizSession()
        {
            _currentQuizIndex = 0;
            _userQuizScore = 0;
            ModalQuiz.Visibility = Visibility.Visible;
            LoadQuizQuestionToUI();
            LogActivity("User initialized Cybersecurity Trivia Challenge.");
        }

        private void LoadQuizQuestionToUI()
        {
            if (_currentQuizIndex >= _quizQuestions.Count)
            {
                // End of quiz game
                ModalQuiz.Visibility = Visibility.Collapsed;

                string endReport = "";
                if (_userQuizScore >= 8)
                {
                    endReport = $"🏆 Exceptional! You scored {_userQuizScore}/10. You're a certified Cybersecurity Pro!";
                }
                else if (_userQuizScore >= 5)
                {
                    endReport = $"👍 Good effort! You scored {_userQuizScore}/10. Keep studying to build up your digital shield defenses.";
                }
                else
                {
                    endReport = $"⚠️ Score: {_userQuizScore}/10. I recommend looking over my 'Suggested Keywords' resources to improve.";
                }

                AddBotMessage($"🎮 Cybersecurity Quiz Complete!\n\n{endReport}");
                LogActivity($"Completed quiz with total score: {_userQuizScore}/{_quizQuestions.Count}");
                return;
            }

            _quizOptionSelected = false;
            BdrQuizFeedback.Visibility = Visibility.Collapsed;
            BtnQuizNext.IsEnabled = false;

            var activeQuestion = _quizQuestions[_currentQuizIndex];
            TxtQuizProgress.Text = $"Question {_currentQuizIndex + 1}/{_quizQuestions.Count}";
            TxtQuizQuestion.Text = activeQuestion.QuestionText;

            ResetOptionButtons();

            BtnOptA.Content = activeQuestion.Options[0];
            BtnOptB.Content = activeQuestion.Options[1];

            if (activeQuestion.Options.Length > 2)
            {
                BtnOptC.Visibility = Visibility.Visible;
                BtnOptD.Visibility = Visibility.Visible;
                BtnOptC.Content = activeQuestion.Options[2];
                BtnOptD.Content = activeQuestion.Options[3];
            }
            else
            {
                BtnOptC.Visibility = Visibility.Collapsed;
                BtnOptD.Visibility = Visibility.Collapsed;
            }
        }

        private void ResetOptionButtons()
        {
            var darkBlue = new SolidColorBrush(Color.FromRgb(26, 34, 52));
            BtnOptA.Background = darkBlue;
            BtnOptB.Background = darkBlue;
            BtnOptC.Background = darkBlue;
            BtnOptD.Background = darkBlue;
            BtnOptA.IsEnabled = true;
            BtnOptB.IsEnabled = true;
            BtnOptC.IsEnabled = true;
            BtnOptD.IsEnabled = true;
        }

        private void BtnQuizOption_Click(object sender, RoutedEventArgs e)
        {
            if (_quizOptionSelected) return;

            _quizOptionSelected = true;
            Button clicked = (Button)sender;
            string content = clicked.Content.ToString();
            char choice = content[0];

            var question = _quizQuestions[_currentQuizIndex];

            if (choice == question.CorrectAnswerChar)
            {
                clicked.Background = Brushes.Green;
                _userQuizScore++;
                TxtFeedbackHeader.Text = "Correct answer! ✔";
                TxtFeedbackHeader.Foreground = Brushes.LightGreen;
            }
            else
            {
                clicked.Background = Brushes.Red;
                TxtFeedbackHeader.Text = $"Incorrect Choice. The correct answer was {question.CorrectAnswerChar}.";
                TxtFeedbackHeader.Foreground = Brushes.OrangeRed;

                // Mark the correct choice as green so they learn
                if (BtnOptA.Content.ToString().StartsWith(question.CorrectAnswerChar.ToString())) BtnOptA.Background = Brushes.Green;
                if (BtnOptB.Content.ToString().StartsWith(question.CorrectAnswerChar.ToString())) BtnOptB.Background = Brushes.Green;
                if (BtnOptC.Content.ToString().StartsWith(question.CorrectAnswerChar.ToString())) BtnOptC.Background = Brushes.Green;
                if (BtnOptD.Content.ToString().StartsWith(question.CorrectAnswerChar.ToString())) BtnOptD.Background = Brushes.Green;
            }

            TxtFeedbackExplain.Text = question.ExplanationText;
            BdrQuizFeedback.Visibility = Visibility.Visible;
            BtnQuizNext.IsEnabled = true;

            BtnOptA.IsEnabled = false;
            BtnOptB.IsEnabled = false;
            BtnOptC.IsEnabled = false;
            BtnOptD.IsEnabled = false;
        }

        private void BtnQuizNext_Click(object sender, RoutedEventArgs e)
        {
            _currentQuizIndex++;
            LoadQuizQuestionToUI();
        }

        private void BtnQuizClose_Click(object sender, RoutedEventArgs e)
        {
            ModalQuiz.Visibility = Visibility.Collapsed;
            LogActivity("Quiz window terminated early by user action.");
        }

        // =========================================================
        // 8. INTERACTIVE UI UTILITIES
        // =========================================================
        private void ProcessInputPayload()
        {
            string raw = UserInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(raw) || raw == _placeholder) return;

            UserInput.Text = "";
            AddUserMessage(raw);
            AnalyzeAndRespond(raw);
        }

        private void AddUserMessage(string text)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(29, 78, 216)),
                CornerRadius = new CornerRadius(14, 14, 2, 14),
                Padding = new Thickness(14),
                Margin = new Thickness(100, 5, 8, 5),
                HorizontalAlignment = HorizontalAlignment.Right,
                MaxWidth = 480,
                Style = (Style)FindResource("AnimatedBubbleStyle")
            };

            border.Child = new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                FontSize = 13.5,
                TextWrapping = TextWrapping.Wrap
            };

            ChatPanel.Children.Add(border);
            ScrollToBottom();
        }

        private void AddBotMessage(string text)
        {
            var holder = new StackPanel();

            var tagLabel = new TextBlock
            {
                Text = "🛡️ CYBERBOT AGENT",
                Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 153)),
                FontSize = 9,
                FontWeight = FontWeights.Bold, // FIXED: Changed from FontWeight.Bold to FontWeights.Bold
                Margin = new Thickness(12, 4, 0, 2)
            };
            holder.Children.Add(tagLabel);

            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(22, 28, 41)),
                CornerRadius = new CornerRadius(14),
                Padding = new Thickness(14),
                Margin = new Thickness(8, 2, 100, 8),
                HorizontalAlignment = HorizontalAlignment.Left,
                MaxWidth = 500,
                Style = (Style)FindResource("AnimatedBubbleStyle")
            };

            border.Child = new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 18
            };

            holder.Children.Add(border);
            ChatPanel.Children.Add(holder);
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            Dispatcher.InvokeAsync(() => {
                ChatScroller.ScrollToEnd();
            });
        }

        private void TriggerSystemAlertSound()
        {
            try
            {
                SystemSounds.Asterisk.Play();
            }
            catch { }
        }

        // =========================================================
        // 9. EVENT HANDLERS
        // =========================================================
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessInputPayload();
        }

        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessInputPayload();
                e.Handled = true;
            }
        }

        private void UserInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UserInput.Text == _placeholder)
            {
                UserInput.Text = "";
                UserInput.Foreground = Brushes.White;
            }
        }

        private void UserInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UserInput.Text))
            {
                UserInput.Text = _placeholder;
                UserInput.Foreground = Brushes.Gray;
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ChatPanel.Children.Clear();
            PnlChatHint.Visibility = Visibility.Visible;
            LogActivity("Conversation console history cleared.");
        }

        private void BtnStartQuiz_Click(object sender, RoutedEventArgs e)
        {
            LaunchQuizSession();
        }

        private void BtnToggleLogs_Click(object sender, RoutedEventArgs e)
        {
            _isShowingAllLogs = !_isShowingAllLogs;
            UpdateActivityUI();
        }

        private void BtnInitDB_Click(object sender, RoutedEventArgs e)
        {
            InitializeLocalDatabase();
            RefreshTaskDisplay();
        }

        private void BtnSaveTask_Click(object sender, RoutedEventArgs e)
        {
            string title = TxtTaskTitle.Text.Trim();
            string desc = TxtTaskDesc.Text.Trim();
            int days = (int)SldDays.Value;

            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Please enter a valid cybersecurity task name first.", "Task Assistant Shield", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AddTaskToDB(title, desc, days);
            TxtTaskTitle.Text = "";
            TxtTaskDesc.Text = "";
            SldDays.Value = 7;
        }
    }
}