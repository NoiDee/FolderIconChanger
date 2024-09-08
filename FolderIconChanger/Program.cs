using System.Text.RegularExpressions;

namespace FolderIconChanger
{
    internal static class Program
    {

        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                args = InitializeArgs(args);
                string directoryPath = GetDirectoryPath(args);
                args[0] = directoryPath;
                string iconPath = GetIconPath(args);
                args[1] = iconPath;
                ApplyIconToDirectory(directoryPath, iconPath);
            }
            catch (SilentException ex)
            {
                Console.WriteLine($"Application Error. {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application Error. {ex.Message}.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string[] InitializeArgs(string[] args)
        {
            if (args.Length >= 2)
            {
                return args;
            }

            string[] newArgs = new string[2];
            for (int x = 0; x < newArgs.Length; x++)
            {
                newArgs[x] = x < args.Length ? args[x] : "";
            }
            return newArgs;
        }

        private static string GetIconPath(string[] args)
        {
            if (args.Length >= 2 && !String.IsNullOrWhiteSpace(args[1]))
            {
                return args[1];
            }

            string iconPath = GetIconPath(args[0]);
            if (!File.Exists(iconPath))
            {
                throw new FileNotFoundException($"Invalid icon or executable was selected: {iconPath}.");
            }
            return iconPath;
        }

        private static string GetIconPath(string initialDirectory)
        {
            OpenFileDialog openFileDialog = new()
            {
                InitialDirectory = initialDirectory,
                Filter = "Icon or Executable Files (*.ico;*.exe)|*.ico;*.exe",
                Title = "Select an Icon or Executable for the Folder"
            };
            if (openFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrWhiteSpace(openFileDialog.FileName))
            {
                throw new SilentException("No icon or executable was selected.");
            }

            return openFileDialog.FileName;
        }

        private static string GetDirectoryPath(string[] args)
        {
            if (args.Length == 0 || String.IsNullOrWhiteSpace(args[0]))
            {
                return GetDirectoryPath();
            }

            string directoryPath = args[0];
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"Invalid directory was selected: {directoryPath}.");
            }

            return directoryPath;
        }

        private static string GetDirectoryPath()
        {
            FolderBrowserDialog folderBrowserDialog = new();
            DialogResult dialogResult = folderBrowserDialog.ShowDialog();
            if (dialogResult != DialogResult.OK || string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
            {
                throw new SilentException("No directory was selected.");
            }

            return folderBrowserDialog.SelectedPath;
        }

        private static void ApplyIconToDirectory(string directoryPath, string iconPath)
        {
            string iniPath = Path.Combine(directoryPath, "desktop.ini");
            string newIniContent = GetNewIniContent(iniPath, iconPath);
            if (File.Exists(iniPath))
            {
                File.Delete(iniPath);
            }

            File.Create(iniPath).Dispose();
            File.WriteAllText(iniPath, newIniContent);

            DirectoryInfo dirInfo = new(directoryPath);
            dirInfo.Attributes |= FileAttributes.System | FileAttributes.ReadOnly;

            FileInfo iniFileInfo = new(iniPath);
            iniFileInfo.Attributes |= FileAttributes.Hidden | FileAttributes.System;

        }

        private static string GetNewIniContent(string iniPath, string iconPath)
        {
            if (!File.Exists(iniPath))
            {
                return $"[.ShellClassInfo]\nIconResource={iconPath},0\n[ViewState]\nMode=\nVid=\nFolderType=Generic\n";
            }

            string iniContent = File.ReadAllText(iniPath);
            string pattern = @"^IconResource\s*=\s*.*$";
            string replacement = $"IconResource={iconPath},0";
            string updatedIniContent = Regex.Replace(iniContent, pattern, replacement, RegexOptions.Multiline);
            return updatedIniContent != iniContent
                ? updatedIniContent
                : updatedIniContent + $"\n{replacement}";

        }
    }
}