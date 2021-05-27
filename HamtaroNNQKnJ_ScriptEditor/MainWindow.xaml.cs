using FolderBrowserEx;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace HamtaroNNQKnJ_ScriptEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string BaseWindowTitle = "Hamtaro Q Script Editor";

        private ScriptFile _scriptFile { get; set; }
        private DirectoryFile _directoryFile { get; set; }
        private int _openDirectoryFileIndex { get; set; } = -1;

        public MainWindow()
        {
            InitializeComponent();
            Title = BaseWindowTitle;
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mainTabControl.SelectedIndex == 0 && _scriptFile != null) // Messages
            {
                Title = $"{BaseWindowTitle} - {_scriptFile.FileName}";
            }
            else if (mainTabControl.SelectedIndex == 1 && _directoryFile != null) // Directory File
            {
                Title = $"{BaseWindowTitle} - {_directoryFile.FileName}";
            }
            else
            {
                Title = BaseWindowTitle;
            }
        }

        private void OpenMessagesButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "DAT file|*.dat"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                _scriptFile = ScriptFile.ParseFromFile(openFileDialog.FileName);
                messageListBox.ItemsSource = _scriptFile.Messages;
                _openDirectoryFileIndex = -1;
                reinsertMessageButton.IsEnabled = false;

                Title = $"{BaseWindowTitle} - {_scriptFile.FileName}";
            }
        }

        private void SaveMessagesButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "DAT file|*.dat"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                _scriptFile.WriteToFile(saveFileDialog.FileName);
            }
        }

        private void ExtractMessagesButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "TXT file|*.txt"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllLines(saveFileDialog.FileName, _scriptFile.Messages.Select(m => $"{m.Text}\n"));
            }
        }

        private void ReinsertMessageButton_Click(object sender, RoutedEventArgs e)
        {
            _directoryFile.ReinsertFile(_openDirectoryFileIndex, _scriptFile);
            mainTabControl.SelectedIndex = 1;
            directoryListBox.Items.Refresh();
        }

        private void MessageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            editStackPanel.Children.Clear();

            if (messageListBox.SelectedIndex == -1)
            {
                return;
            }

            var message = (Message)messageListBox.SelectedItem;

            if (showIntroBytesCheckBox.IsChecked == true)
            {
                var introBytesTextBox = new MessageTextBox
                {
                    Text = message.IntroBytes,
                    Message = message,
                    AcceptsReturn = false,
                    MaxLength = 2,
                };
                introBytesTextBox.TextChanged += IntroBytesTextBox_TextChanged;
                
                editStackPanel.Children.Add(introBytesTextBox);
            }    

            var messageTextBox = new MessageTextBox
            {
                Text = message.Text,
                Message = message,
                AcceptsReturn = true,
                MaxLines = 7,
            };
            messageTextBox.TextChanged += MessageTextBox_TextChanged;

            editStackPanel.Children.Add(messageTextBox);
        }

        private void ShowIntroBytesCheckBox_Click(object sender, RoutedEventArgs e)
        {
            MessageListBox_SelectionChanged(null, null);
        }

        private void IntroBytesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var introBytesTextBox = (MessageTextBox)sender;

            introBytesTextBox.Message.IntroBytes = introBytesTextBox.Text;
            messageListBox.Items.Refresh();
        }

        private void MessageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var messageTextBox = (MessageTextBox)sender;

            messageTextBox.Message.Text = messageTextBox.Text.Replace("\r", "");
            messageListBox.Items.Refresh();
        }

        private void HexEditorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            hexPreviewTextBlock.Text = "";

            var hexText = hexEditorTextBox.Text.Replace(" ", "");
            for (int i = 0; i < hexEditorTextBox.Text.Length; i += 2)
            {
                try
                {
                    if (byte.TryParse($"{hexText[i]}{hexText[i + 1]}", NumberStyles.HexNumber, null, out byte currentByte))
                    {
                        if (currentByte == 0xFF)
                        {
                            byte[] nextTwoBytes = new byte[]
                            {
                                byte.Parse($"{hexText[i + 2]}{hexText[i + 3]}", NumberStyles.HexNumber),
                                byte.Parse($"{hexText[i + 4]}{hexText[i + 5]}", NumberStyles.HexNumber),
                            };

                            (string op, int bytes) = ScriptFile.GetFFOp(nextTwoBytes, new ArgumentException($"Encountered unknown opcode 0x{hexText[i + 2]}{hexText[i + 3]}"));
                            hexPreviewTextBlock.Text += op;
                            i += (bytes - 1) * 2;
                        }
                        else
                        {
                            ScriptFile.ByteToCharMap.TryGetValue(currentByte, out string mappedChar);
                            hexPreviewTextBlock.Text += mappedChar;
                        }
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    // do nothing
                }
            }
        }

        private void OpenDirectoryFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "DAT file|*.dat"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                _directoryFile = DirectoryFile.ParseFromFile(openFileDialog.FileName);
                directoryListBox.ItemsSource = _directoryFile.FilesInDirectory;
                _openDirectoryFileIndex = -1;
                reinsertMessageButton.IsEnabled = false;

                Title = $"{BaseWindowTitle} - {_directoryFile.FileName}";
            }
        }

        private void SaveDirectoryFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "DAT file|*.dat"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                _directoryFile.WriteToFile(saveFileDialog.FileName);
            }
        }

        private void ExtractDirectoryFileButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
            {
                AllowMultiSelect = false,
            };
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                for (int i = 0; i < _directoryFile.FilesInDirectory.Count; i++)
                {
                    File.WriteAllBytes(Path.Combine(folderBrowserDialog.SelectedFolder, $"{i:d4}.dat"), _directoryFile.FilesInDirectory[i].Content);
                }
            }
        }

        private void ExtractAllTextFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "TXT file|*.txt"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                List<string> text = new List<string>();

                IEnumerable<(int offset, ScriptFile script)> files = _directoryFile.FilesInDirectory
                    .Where(f => f.FileType == "Script File")
                    .Select(f => (f.Offset, ScriptFile.ParseFromData(f.Content)));
                foreach (var file in files)
                {
                    text.Add("---------------------------------------------------------------------------------");
                    text.Add($"0x{file.offset:X8}\n");
                    text.Add(string.Join("\n\n", file.script.Messages.Select(m => m.Text)));
                }

                File.WriteAllLines(saveFileDialog.FileName, text.ToArray());
            }
        }

        private void DirectoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                openInMessageButton.IsEnabled = true;
            }
        }

        private void OpenInMessageButton_Click(object sender, RoutedEventArgs e)
        {
            var file = (FileInDirectory)directoryListBox.SelectedItem;
            _scriptFile = ScriptFile.ParseFromData(file.Content);
            _scriptFile.FileName = $"{_directoryFile.FileName} at 0x{file.Offset:X8}";
            messageListBox.ItemsSource = _scriptFile.Messages;

            _openDirectoryFileIndex = directoryListBox.SelectedIndex;
            reinsertMessageButton.IsEnabled = true;

            mainTabControl.SelectedIndex = 0;
        }
    }
}
