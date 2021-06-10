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
        private static string BaseWindowTitle = "Hamtaro Nazo Nazo Q Script Editor";
        private static string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HamtaroScriptEditor");

        private MessageFile _messageFile { get; set; }
        private DirectoryFile _directoryFile { get; set; }
        private int _openDirectoryFileIndex { get; set; } = -1;
        private uint _globalOffset { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Title = BaseWindowTitle;
            if (!Directory.Exists(AppDataPath))
            {
                Directory.CreateDirectory(AppDataPath);
            }
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mainTabControl.SelectedIndex == 0 && _messageFile != null) // Messages
            {
                Title = $"{BaseWindowTitle} - {_messageFile.FileName}";
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
                _messageFile = MessageFile.ParseFromFile(openFileDialog.FileName);
                messageListBox.ItemsSource = _messageFile.Messages;
                _openDirectoryFileIndex = -1;
                reinsertMessageButton.IsEnabled = false;

                Title = $"{BaseWindowTitle} - {_messageFile.FileName}";
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
                _messageFile.WriteToFile(saveFileDialog.FileName);
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
                File.WriteAllLines(saveFileDialog.FileName, _messageFile.Messages.Select(m => $"{m.Text}\n"));
            }
        }

        private void ReinsertMessageButton_Click(object sender, RoutedEventArgs e)
        {
            _directoryFile.ReinsertMessageFile(_openDirectoryFileIndex, _messageFile);
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

                            (string op, int bytes) = MessageFile.GetFFOp(nextTwoBytes, new ArgumentException($"Encountered unknown opcode 0x{hexText[i + 2]}{hexText[i + 3]}"));
                            hexPreviewTextBlock.Text += op;
                            i += (bytes - 1) * 2;
                        }
                        else
                        {
                            MessageFile.ByteToCharMap.TryGetValue(currentByte, out string mappedChar);
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
                string directoryFileNotesCache = Path.Combine(AppDataPath, $"{_directoryFile.FileName}.info");
                if (!File.Exists(directoryFileNotesCache))
                {
                    using (var notesFile = File.CreateText(directoryFileNotesCache))
                    {
                        foreach (FileInDirectory file in _directoryFile.FilesInDirectory)
                        {
                            notesFile.WriteLine();
                        }
                    }
                }
                string[] notes = File.ReadAllLines(directoryFileNotesCache);
                for (int i = 0; i < notes.Length; i++)
                {
                    _directoryFile.FilesInDirectory[i].Notes = notes[i];
                }

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

                IEnumerable<(int offset, MessageFile messageFile)> files = _directoryFile.FilesInDirectory
                    .Where(f => f.FileType == "Message File")
                    .Select(f => (f.Offset, MessageFile.ParseFromData(f.Content)));
                foreach (var file in files)
                {
                    text.Add("---------------------------------------------------------------------------------");
                    text.Add($"0x{file.offset:X8}\n");
                    text.Add(string.Join("\n\n", file.messageFile.Messages.Select(m => m.Text)));
                }

                File.WriteAllLines(saveFileDialog.FileName, text.ToArray());
            }
        }

        private void DirectoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                openInMessageButton.IsEnabled = true;
                decompressBgTilesButton.IsEnabled = true;
                decompressSpriteTilesButton.IsEnabled = true;
                exportPaletteButton.IsEnabled = true;
                directoryFileDetailsStackPanel.Children.Clear();

                var file = (FileInDirectory)directoryListBox.SelectedItem;
                directoryFileDetailsStackPanel.Children.Add(new TextBlock
                {
                    Text = $"{_directoryFile.FilesInDirectory.IndexOf(file):d4}"
                });
                directoryFileDetailsStackPanel.Children.Add(new TextBlock
                {
                    Text = $"Start Offset:\t0x{file.Offset + _globalOffset:X8}"
                });
                directoryFileDetailsStackPanel.Children.Add(new TextBlock
                {
                    Text = $"End Offset:\t0x{file.Offset + file.Content.Length - 1 + _globalOffset:X8}"
                });

                var fileTextBox = new FileTextBox
                {
                    File = file,
                    Text = file.Notes,
                };
                fileTextBox.TextChanged += FileTextBox_TextChanged;
                directoryFileDetailsStackPanel.Children.Add(fileTextBox);

                if (file.GetType() == typeof(SpriteMapFile))
                {
                    var spriteMapFile = (SpriteMapFile)file;
                    directoryFileDetailsStackPanel.Children.Add(new Separator());
                    directoryFileDetailsStackPanel.Children.Add(new TextBlock
                    {
                        Text = $"Associated Palette: {spriteMapFile.AssociatedPaletteIndex} (file {spriteMapFile.AssociatedPalette?.Index ?? -1})"
                    });
                    if (spriteMapFile.AssociatedTiles is not null && spriteMapFile.AssociatedPalette is not null)
                    {
                        directoryFileDetailsStackPanel.Children.Add(new Separator());
                        for (int i = 0; i < spriteMapFile.NumClips; i++)
                        {
                            directoryFileDetailsStackPanel.Children.Add(new Image { Source = Helpers.GetBitmapImageFromBitmap(spriteMapFile.GetAnimationPreview(i)) });
                        }
                    }
                }
                else if (file.GetType() == typeof(TileFile))
                {
                    var tileFile = (TileFile)file;
                    if (tileFile.Palette is not null)
                    {
                        directoryFileDetailsStackPanel.Children.Add(new Separator());
                        directoryFileDetailsStackPanel.Children.Add(new TextBlock
                        {
                            Text = $"Associated Palette: {tileFile.SpriteMapFile?.AssociatedPaletteIndex ?? -1} (file {tileFile.Palette?.Index ?? -1})"
                        });
                        if (tileFile.PixelData is not null)
                        {
                            directoryFileDetailsStackPanel.Children.Add(new Image { Source = Helpers.GetBitmapImageFromBitmap(tileFile.Get16ColorImage()), MaxWidth = 256 });
                            directoryFileDetailsStackPanel.Children.Add(new Separator());
                            directoryFileDetailsStackPanel.Children.Add(new Image { Source = Helpers.GetBitmapImageFromBitmap(tileFile.Get256ColorImage()), MaxWidth = 256 });
                        }
                    }
                }
                else if (file.GetType() == typeof(PaletteFile))
                {
                    var paletteFile = (PaletteFile)file;
                    directoryFileDetailsStackPanel.Children.Add(new Separator());
                    directoryFileDetailsStackPanel.Children.Add(new Image { Source = Helpers.GetBitmapImageFromBitmap(paletteFile.GetPaletteDisplay()) });
                }
            }
            if (e.RemovedItems.Count > 0)
            {
                string directoryFileNotesCache = Path.Combine(AppDataPath, $"{_directoryFile.FileName}.info");
                using (var cacheFile = File.CreateText(directoryFileNotesCache))
                {
                    foreach (var f in _directoryFile.FilesInDirectory)
                    {
                        cacheFile.WriteLine(f.Notes);
                    }
                }
            }
        }

        private void FileTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (FileTextBox)sender;
            textBox.File.Notes = textBox.Text;
            directoryListBox.Items.Refresh();
        }

        private void OpenInMessageButton_Click(object sender, RoutedEventArgs e)
        {
            var file = (FileInDirectory)directoryListBox.SelectedItem;
            _messageFile = MessageFile.ParseFromData(file.Content);
            _messageFile.FileName = $"{_directoryFile.FileName} at 0x{file.Offset:X8}";
            messageListBox.ItemsSource = _messageFile.Messages;

            _openDirectoryFileIndex = directoryListBox.SelectedIndex;
            reinsertMessageButton.IsEnabled = true;

            mainTabControl.SelectedIndex = 0;
        }

        private void DecompressBgTilesButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "dstile file|*.dstile"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                var file = (FileInDirectory)directoryListBox.SelectedItem;
                var tileFile = TileFile.ParseBGFromCompressedData(file.Content);
                tileFile.WritePixelsToFile(saveFileDialog.FileName);
                MessageBox.Show("Extracted successfully!");
            }
        }

        private void DecompressSpriteTilesButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "dstile file|*.dstile"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                var file = (FileInDirectory)directoryListBox.SelectedItem;
                try
                {
                    var tileFile = TileFile.ParseSpriteFromCompressedData(file.Content);
                    tileFile.WritePixelsToFile(saveFileDialog.FileName);
                    MessageBox.Show("Extracted successfully!");
                }
                catch (Exception exc)
                {
                    MessageBox.Show($"Error extracting sprite file: ${exc.Message}");
                }
            }
        }

        private void GlobalOffsetTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (uint.TryParse(globalOffsetTextBox.Text, NumberStyles.HexNumber, null, out uint globalOffset))
            {
                _globalOffset = globalOffset;
            }
            else
            {
                _globalOffset = 0;
            }
        }

        private void ExportPaletteButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "RIFF Palette file|*.pal"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                var file = (FileInDirectory)directoryListBox.SelectedItem;
                var paletteFile = PaletteFile.ParseFromData(file.Content);
                paletteFile.WriteRiffPaletteFile(saveFileDialog.FileName);
                MessageBox.Show("Extracted successfully!");
            }
        }

        private void ParseSpriteIndexFileButton_Click(object sender, RoutedEventArgs e)
        {
            _directoryFile.ParseSpriteIndexFile();
            directoryListBox.Items.Refresh();
        }
    }
}
