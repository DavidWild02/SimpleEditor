using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using WinForms = System.Windows.Forms;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace SimpleEditor
{
    public class FileItem
    {
        public FileItem()
        {
            ChangesSaved = true;
        }

        public string FileName { get; set; }
        public string Path { get; set; }
        private string _content = "";
        public string Content {
            get { return _content;  }
            set
            {
                ChangesSaved = false;
                _content = value;
            }
        }
        public bool ChangesSaved { get; set; }
    }

    public class FolderItem
    {
        public FolderItem()
        {
            Children = new ObservableCollection<object>();
        }

        public ObservableCollection<object> Children { get; set; }
        public string FolderName { get; set; }
        public string Path { get; set; }
    }


    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged 
    {
        private WinForms.FolderBrowserDialog folderBrowserDialog = new WinForms.FolderBrowserDialog();
        private string folderName;
        private double WidthExplorer = 125;
        private int unnamedFiles = 0;

        private ObservableCollection<object> _openTabs = new ObservableCollection<object>();
        public ObservableCollection<object> openTabs
        {
            get { return _openTabs; }
            set
            {
                _openTabs = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            navBar.ItemsSource = openTabs;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        private FolderItem get_folder_structure(string dirPath)
        {
            ObservableCollection<object> children = new ObservableCollection<object>();
            foreach (string subdir in Directory.GetDirectories(dirPath))
            {
                children.Add(get_folder_structure(subdir));
            }
            foreach (string file in Directory.GetFiles(dirPath))
            {
                children.Add(new FileItem { Path=file, FileName=System.IO.Path.GetFileName(file) });
            }
            return new FolderItem
            {
                Path = dirPath,
                FolderName = dirPath.Substring(System.IO.Path.GetDirectoryName(dirPath).Length),
                Children = children
            };
        }

        private void open_file_in_editor(object sender, RoutedEventArgs e)
        {
            if (folderView.SelectedItem is FileItem) {
                FileItem selectedFile = folderView.SelectedItem as FileItem;

                if (openTabs.Any<object>(item => item is FileItem ? (item as FileItem).Path == selectedFile.Path : false)) return;

                using (StreamReader sr = new StreamReader(selectedFile.Path))
                {
                    selectedFile.Content = sr.ReadToEnd();
                    selectedFile.ChangesSaved = true;
                }
                openTabs.Add(selectedFile);
                navBar.SelectedIndex = openTabs.Count - 1;
            }
        }

        private void collapsed_expander(object sender, RoutedEventArgs e)
        {
            WidthExplorer = explorerColumn.Width.Value;
            openTabsColumn.Width = new GridLength(1, GridUnitType.Star);
            explorerColumn.Width = new GridLength(1, GridUnitType.Auto);
        }

        private void expanded_expander(object sender, RoutedEventArgs e)
        {
            openTabsColumn.Width = new GridLength(1, GridUnitType.Star);
            explorerColumn.Width = new GridLength(WidthExplorer);
        }

        private void close_tab(object sender, RoutedEventArgs e)
        {
            this.CloseCommand.Command.Execute(null);
        }

        private void click_new_file(object sender, RoutedEventArgs e)
        {
            openTabs.Add(new FileItem
            {
                FileName = "unbenannt" + unnamedFiles++,
                ChangesSaved = false
            });
            navBar.SelectedIndex = openTabs.Count - 1;
        }


        private void execute_open_file(object sender, ExecutedRoutedEventArgs e)
        {
            WinForms.OpenFileDialog dialog = new WinForms.OpenFileDialog();
            dialog.Filter = "txt files (*.txt)|*.txt";
            if (dialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                if (!dialog.CheckFileExists)
                {
                    MessageBox.Show("The given file does not exist");
                    return;
                }
                string content = "";
                using (StreamReader sr = new StreamReader(dialog.FileName))
                {
                    content = sr.ReadToEnd();
                }
                openTabs.Add(new FileItem
                {
                    Path = dialog.FileName,
                    FileName = System.IO.Path.GetFileName(dialog.FileName),
                    Content = content
                });
            }     
        }

        private void execute_open_directory(object sender, ExecutedRoutedEventArgs e)
        {
            WinForms.DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == WinForms.DialogResult.OK)
            {
                folderName = folderBrowserDialog.SelectedPath;

                folderView.ItemsSource = new List<object>() { get_folder_structure(folderName) };
            }
        }

        private void execute_save(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                using (StreamWriter sw = File.CreateText((navBar.SelectedItem as FileItem).Path))
                {
                    sw.Write((navBar.SelectedItem as FileItem).Content);
                }
                (navBar.SelectedItem as FileItem).ChangesSaved = true;
            }
            catch
            {
                MessageBox.Show(String.Format("An error occured. Was not able to save {0}", (navBar.SelectedItem as FileItem).FileName));
            }
        }

        private void execute_save_as(object sender, ExecutedRoutedEventArgs e)
        {
            WinForms.SaveFileDialog dialog = new WinForms.SaveFileDialog();
            dialog.Filter = "txt files (*.txt)|*.txt";
            if (dialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                (navBar.SelectedItem as FileItem).Path = dialog.FileName;
                (navBar.SelectedItem as FileItem).FileName = System.IO.Path.GetFileName(dialog.FileName);
                this.SaveCommand.Command.Execute(null);
            }
        }

        private void execute_save_all(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (object item in openTabs)
            {
                if (!(item is FileItem) || String.IsNullOrEmpty((item as FileItem).Path)) continue;
                if (!(item as FileItem).ChangesSaved)
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter((item as FileItem).Path))
                        {
                            sw.Write((item as FileItem).Content);
                        }
                        (item as FileItem).ChangesSaved = true;
                    }
                    catch
                    {
                        MessageBox.Show(String.Format("Ein Fehler ist aufgetreten. Es war nicht möglich {0} zu speichern", (item as FileItem).FileName));
                    }
                }
            }
        }

        private void execute_close(object sender, ExecutedRoutedEventArgs e)
        {
            FileItem item = navBar.SelectedItem as FileItem;
            if (!item.ChangesSaved)
            {
                MessageBoxResult result = MessageBox.Show("Die Datei wurde nicht gespeichert! Möchten sie die Datei speichern?", "Haaalt Stoooop, Jetzt Rede Ich", MessageBoxButton.YesNoCancel);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        if (String.IsNullOrEmpty((navBar.SelectedItem as FileItem).Path))
                            this.SaveAsCommand.Command.Execute(null);
                        else
                            this.SaveCommand.Command.Execute(null);
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.None:
                    case MessageBoxResult.Cancel:
                        return;
                }
            }
            openTabs.RemoveAt(navBar.SelectedIndex);
        }

        private void execute_close_all(object sender, ExecutedRoutedEventArgs e)
        {
            bool notSavedOnes = openTabs.Any<object>(item =>
            {
                if (item is FileItem)
                {
                    return !(item as FileItem).ChangesSaved;
                }
                return false;
            }
            );
            if (notSavedOnes)
            {
                MessageBoxResult result = MessageBox.Show("Einige Änderungen wurden nicht gespeichert? Jetzt speichern?", "Es Ist Obst Im Haus!!!", MessageBoxButton.YesNoCancel);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                            this.SaveAllCommand.Command.Execute(null);
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.None:
                    case MessageBoxResult.Cancel:
                        return;
                }
            }
            openTabs.Clear();
        }


        private void canexecute_tab_selected(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = navBar.SelectedItem is FileItem;
        }

        private void canexecute_save(object sender, CanExecuteRoutedEventArgs e)
        {
            if (navBar.SelectedItem is FileItem)
                e.CanExecute = !String.IsNullOrEmpty((navBar.SelectedItem as FileItem).Path);
            else
                e.CanExecute = false;
        }
    }
}
