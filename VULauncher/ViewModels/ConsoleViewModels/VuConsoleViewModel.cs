using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using VULauncher.Util;
using VULauncher.ViewModels.Common;
using VULauncher.ViewModels.Enums;

namespace VULauncher.ViewModels.ConsoleViewModels
{
    public class VuConsoleViewModel : DockableDocumentViewModel
    {
        public StartupType StartupType { get; set; }
        public ProcessUtils GameProcess { get; set; }

        private readonly int ConsoleOutputMaxLines = 1000;

        private static readonly Dictionary<Regex, Color> styleSheets = new Dictionary<Regex, Color>
        {
            { new Regex(PathRegex), Color.FromRgb(246, 185, 59) },
            { new Regex(GuidRegex), Color.FromRgb(255, 103, 51) },
            { new Regex(TimeStampRegex), Color.FromRgb(96, 96, 96) },
            { new Regex(InfoRegex), Color.FromRgb(255, 255, 255) },
            { new Regex(ErrorRegex), Color.FromRgb(200, 25, 25) },
            { new Regex(SuccessRegex), Color.FromRgb(25, 200, 25) },
            { new Regex(CompilingRegex), Color.FromRgb(25, 200, 25) },
        };

        private static readonly Dictionary<string, Color> ModColors = new Dictionary<string, Color>();

        public ObservableCollection<Inline> CustomInlines { get; set; } = new ObservableCollection<Inline>();

        private const string TimeStampRegex = @"^\[\d+:\d+:\d+\]";
        private const string InfoRegex = @"^\[info\].(?![Ss]uccess\w+).+";
        private const string ErrorRegex = @"^\[error\].+";
        private const string SuccessRegex = @".+[Ss]uccess\w+.+";
        private const string CompilingRegex = @"Compiling .+\.\.\.";

        private const string GuidRegex = @"[0-9a-f]{32}";
        private const string PathRegex = @"(\w+/)+\w+";

        public VuConsoleViewModel(string consoleName, StartupType startupType)
        {
            Title = consoleName;
            StartupType = startupType;
            GameProcess = new ProcessUtils();
        }

        private void FindRegExPatternColor(string text)
        {
            foreach (var styleSheet in styleSheets)
            {
                foreach (Match match in styleSheet.Key.Matches(text))
                {
                    CustomInlines.Add(
                        new Run
                        {
                            Text = match.Value,
                            Foreground = new SolidColorBrush(styleSheet.Value)
                        }
                    );
                }
            }
        }

        private static Color GenerateColor(string text)
        {
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
            Color color = Color.FromRgb(hash[0], hash[1], hash[2]);
            if (color.R * 0.2126 + color.G * 0.7152 + color.B * 0.0722 > 255 / 2)
            {
                return GenerateColor(text + text);
            } else
            {
                return color;
            }
        }

        private static Color FindModColor(string modName)
        {
            if (ModColors.ContainsKey(modName))
            {
                return ModColors[modName];
            }
            else
            {
                Color color = GenerateColor(modName);
                ModColors.Add(modName, color);
                return color;
            }
        }


        /*
         * TODO: MAKE BETTER
         * NOT FINISHED
         */
        private void FormatAndClean(string input)
        {
            Match messageInfo = Regex.Match(input, @"^\[(\d+-\d+-\d+ (\d+:\d+:\d+).\d+:\d+)\] \[(\w+)\] ");

            if (messageInfo.Groups.Count < 3)
            {
                CustomInlines.Add(
                    new Run
                    {
                        Text = input,
                        Foreground = Brushes.White
                    }
                );
                CustomInlines.Add(new LineBreak());
                return;
            }

            string time = messageInfo.Groups[2].ToString();
            string type = messageInfo.Groups[3].ToString();

            string messageRaw = input.Replace(messageInfo.Groups[0].ToString(), string.Empty);

            FindRegExPatternColor("[" + time + "]");

            if (!messageRaw.Contains("[VeniceEXT]"))
            {
                if (type == "error")
                {
                    CustomInlines.Add(
                        new Run
                        {
                            Text = "[" + type + "] " + messageRaw,
                            Foreground = new SolidColorBrush(Color.FromRgb(200, 25, 25))
                        }
                    );
                }
                else
                {
                    CustomInlines.Add(
                        new Run
                        {
                            Text = "[" + type + "] " + messageRaw,
                            Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255))
                        }
                    );
                }
            }
            else
            {
                string message = messageRaw.Replace("[VeniceEXT] ", string.Empty);
                string modName = Regex.Match(message, @"^\[(\w+)\] ").Groups[1].ToString();

                if (modName == string.Empty)
                {
                    FindRegExPatternColor("[" + type + "] " + message);
                }
                else
                {
                    FindRegExPatternColor("[" + type + "]");
                    CustomInlines.Add(
                        new Run
                        {
                            Text = "[",
                            Foreground = Brushes.AliceBlue
                        }
                    );
                    CustomInlines.Add(
                        new Run
                        {
                            Text = modName,
                            Foreground = new SolidColorBrush(FindModColor(modName))
                        }
                    );
                    CustomInlines.Add(
                        new Run
                        {
                            Text = "] ",
                            Foreground = Brushes.AliceBlue
                        }
                    );

                    message = message.Replace("[" + modName + "] ", "");
                    string moduleName = Regex.Match(message, @"^\[(\w+)\] ").Groups[1].ToString();

                    if (moduleName == string.Empty)
                    {
                        if (message.Contains("Compiling"))
                        {
                            CustomInlines.Add(
                                new Run
                                {
                                    Text = message,
                                    Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100))
                                }
                            );
                        }
                        else if (message.Contains("Error: "))
                        {
                            CustomInlines.Add(
                                new Run
                                {
                                    Text = message,
                                    Foreground = Brushes.Red
                                }
                            );
                        }
                        else
                        {
                            CustomInlines.Add(
                                new Run
                                {
                                    Text = message,
                                    Foreground = Brushes.White
                                }
                            );
                        }
                    }
                    else
                    {
                        message = message.Replace("[" + moduleName + "] ", "");
                        CustomInlines.Add(
                            new Run
                            {
                                Text = "[",
                                Foreground = Brushes.AliceBlue
                            }
                        );
                        CustomInlines.Add(
                            new Run
                            {
                                Text = moduleName,
                                Foreground = new SolidColorBrush(FindModColor(moduleName))
                            }
                        );
                        CustomInlines.Add(
                            new Run
                            {
                                Text = "] ",
                                Foreground = Brushes.AliceBlue
                            }
                        );
                        CustomInlines.Add(
                            new Run
                            {
                                Text = message,
                                Foreground = Brushes.White
                            }
                        );
                    }
                }
            }

            CustomInlines.Add(new LineBreak());
        }

        private void CheckLinesLimits()
        {
            if (CustomInlines.Where(x => x is LineBreak).Count() > ConsoleOutputMaxLines)
            {
                CustomInlines
                    .Where(x => CustomInlines.IndexOf(x) <= CustomInlines.IndexOf(CustomInlines.First(x => x is LineBreak)))
                    .ToList()
                    .All(x => CustomInlines.Remove(x));
            }
        }

        public void WriteLog(string text)
        {
            if (text.Length > 0)
            {
                FormatAndClean(text);
                CheckLinesLimits();
            }
        }
    }
}
