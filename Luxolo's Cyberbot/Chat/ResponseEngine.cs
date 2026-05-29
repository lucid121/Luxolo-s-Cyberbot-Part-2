using System;
using System.Collections.Generic;

namespace Luxolo_s_Cyberbot.Chat
{
    internal class ResponseEngine
    {
        // Dictionary maps keywords to cybersecurity answers.
        private readonly Dictionary<string, string> _responses =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Small talk replies.
                ["how are you"] =
                "I'm fully patched and running smoothly, thanks for asking! " +
                "How can I help you stay safe online today?",

                ["your name"] =
                "I'm CyberBot — your Cybersecurity Awareness Assistant, " +
                "built to help South African citizens stay safe online.",

                ["purpose"] =
                "My purpose is to educate you about cyber threats like phishing, " +
                "weak passwords, and malware so you can protect yourself online.",

                ["what can"] =
                "You can ask me about:\n" +
                "  - Phishing emails\n" +
                "  - Password safety\n" +
                "  - Safe browsing\n" +
                "  - Malware and ransomware\n" +
                "  - Social engineering\n" +
                "  - VPNs and 2FA",

                // Cybersecurity topic replies.
                ["phishing"] =
                "WARNING: Phishing is when criminals send fake emails pretending " +
                "to be a trusted source like your bank.\n\n" +
                "  - Always check the sender's email address carefully.\n" +
                "  - Never click links in unexpected emails.\n" +
                "  - Look for spelling mistakes and urgent language — red flags!",

                ["password"] =
                "Strong passwords are your first line of defence.\n\n" +
                "  - Use at least 12 characters with letters, numbers, and symbols.\n" +
                "  - Never reuse the same password across sites.\n" +
                "  - Use a password manager like Bitwarden.\n" +
                "  - Enable two-factor authentication (2FA) everywhere.",

                ["browsing"] =
                "Safe browsing tips:\n\n" +
                "  - Look for 'https://' before entering personal info.\n" +
                "  - Avoid public Wi-Fi for banking — use a VPN if you must.\n" +
                "  - Keep your browser and plugins up to date.\n" +
                "  - Be cautious of pop-ups offering prizes or urgent warnings.",

                ["malware"] =
                "Malware is malicious software designed to harm your device.\n\n" +
                "  - Install reputable antivirus software and keep it updated.\n" +
                "  - Never download files from untrusted sources.\n" +
                "  - Avoid pirated software — it often hides malware.",

                ["ransomware"] =
                "Ransomware encrypts your files and demands payment.\n\n" +
                "  - Back up your data regularly to an offline drive or cloud.\n" +
                "  - Do NOT pay the ransom — it funds criminals.\n" +
                "  - Report attacks to the SAPS Cybercrime Unit.",

                ["social engineering"] =
                "Social engineering is manipulating people into revealing information.\n\n" +
                "  - Be sceptical of anyone asking for passwords — even 'IT support'.\n" +
                "  - Verify callers by phoning back on an official number.\n" +
                "  - Trust your instincts — if something feels wrong, it probably is.",

                ["2fa"] =
                "Two-Factor Authentication (2FA) adds a second login step " +
                "so even a stolen password won't grant access.\n\n" +
                "  - Enable 2FA on email, banking, and social media now.",

                ["vpn"] =
                "A VPN (Virtual Private Network) encrypts your internet traffic, " +
                "hiding it from eavesdroppers on public networks.\n\n" +
                "  - Use a trusted paid VPN — free ones may sell your data.",
            };

        // Searches keywords and returns a matching reply.
        public string GetResponse(string userInput)
        {
            // Returns early if user typed nothing useful.
            if (string.IsNullOrWhiteSpace(userInput))
                return "It looks like you didn't type anything. Please ask me a question!";

            // Checks if input contains any known keyword.
            foreach (var entry in _responses)
            {
                if (userInput.IndexOf(entry.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                    return entry.Value;
            }

            // No match found, suggests topics to try.
            return "I didn't quite understand that. Try asking about:\n" +
                   "  phishing | passwords | safe browsing | malware | 2FA | VPN";
        }

        // Returns true if the user wants to quit.
        public static bool IsExitCommand(string input)
        {
            return input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                   input.Equals("quit", StringComparison.OrdinalIgnoreCase) ||
                   input.Equals("bye", StringComparison.OrdinalIgnoreCase);
        }
    }
}