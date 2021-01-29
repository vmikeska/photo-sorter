using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using photo_sorter.ints;
using System.Windows.Input;

namespace photo_sorter
{

    public partial class MainWindow : Window
    {
        public string Dir = "";
        public List<string> Files = new List<string>();
        public int CurrentImagePos = 0;

        private ConfigInt Config = null;

        private string LastMovedFileOrigin = null;
        private string LastMovedFileDestination = null;

        private TouchEvents Touch;

        private List<string> ImageExts = new List<string>()
        {
            "png", "jpg", "jpeg", "bmp", "gif", "tif", "tiff"
        };

        public MainWindow()
        {
            InitializeComponent();

            this.Drop += MainWindow_Drop;

            LoadConfig();

            Touch = new TouchEvents(this, Config);
            Touch.DirectionSwipedEvent += Touch_DirectionSwipedEvent;

            InitButtons();
            InitLabels();

            MainVideo.UnloadedBehavior = MediaState.Manual;
        }

        private void Touch_DirectionSwipedEvent(object sender, string direction)
        {
            var action = Config.actions.First(i => i.direction == direction);
            if (action != null)
            {
                MoveCurrentFile(action.directory);
            }
        }
    
        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("FileName"))
            {
                var dirs = (string[])e.Data.GetData("FileName");
                var dir = dirs[0];
                LoadDir(dir);
            }
        }
        
        private void MoveBack_Click(object sender, RoutedEventArgs e)
        {
            MoveFileBack();
        }

        private void ActionBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var action = btn.Tag as ButtonsInt;

            MoveCurrentFile(action.directory);
        }

        private void OpenDialogClick(object sender, RoutedEventArgs e)
        {
            OpenDirectoryDialog();
        }

        private void NextClick(object sender, RoutedEventArgs e)
        {
            var isLastPos = CurrentImagePos >= Files.Count() - 1;
            if (isLastPos)
            {
                return;
            }

            var nextPos = CurrentImagePos + 1;
            ShowMediaByPos(nextPos);
            CurrentImagePos = nextPos;
        }

        private void BackClick(object sender, RoutedEventArgs e)
        {
            if (CurrentImagePos == 0)
            {
                return;
            }

            var prevPos = CurrentImagePos - 1;
            ShowMediaByPos(prevPos);
            CurrentImagePos = prevPos;
        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            DeleteFile();
        }

        private void MainVideo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainVideo.Position = new TimeSpan(0);
            MainVideo.Play();
        }

        private void LoadConfig()
        {
            using (StreamReader file = File.OpenText("config.json"))
            {
                var serializer = new JsonSerializer();
                Config = (ConfigInt)serializer.Deserialize(file, typeof(ConfigInt));
            }
        }

        private void MoveFileBack()
        {
            File.Move(LastMovedFileDestination, LastMovedFileOrigin);

            Files.Insert(CurrentImagePos, LastMovedFileOrigin);
            LastMovedFileDestination = null;
            LastMovedFileOrigin = null;
            BtnMoveBack.Visibility = Visibility.Hidden;
            ShowMediaByPos(CurrentImagePos);
        }

        private void InitButtons()
        {
            foreach (var action in Config.actions)
            {
                var btn = new Button();
                btn.Content = action.name;
                btn.Tag = action;
                btn.FontSize = 25;
                btn.Margin = new Thickness(5, 5, 5, 5);
                btn.Click += ActionBtn_Click;
                btn.Style = App.Current.Resources["btnBlue"] as Style;
                ActionsPanel.Children.Add(btn);
            }
        }

        private void InitLabels()
        {
            foreach (var action in Config.actions)
            {
                var control = getLabelByDirection(action.direction);
                if (control != null)
                {
                    control.Text = action.name;
                }
            }
        }

        private TextBlock getLabelByDirection(string direction)
        {
            if (direction == "TOP")
            {
                return LblTop;
            }
            if (direction == "BOTTOM")
            {
                return LblBottom;
            }
            if (direction == "LEFT")
            {
                return LbLeft;
            }
            if (direction == "RIGHT")
            {
                return LblRight;
            }

            return null;
        }

        private void MoveCurrentFile(string newDir)
        {
            
            if (Files.Count() == 0)
            {
                return;
            }

            MainVideo.Visibility = Visibility.Hidden;
            MainImage.Visibility = Visibility.Hidden;
            //Preloader.Visibility = Visibility.Visible;

            MainVideo.Stop();
            MainVideo.Close();

            var currentFile = Files[CurrentImagePos];
            MoveFile(currentFile, newDir);
            Files.Remove(currentFile);

            if (Files.Count() > 0)
            {
                ShowMediaByPos(CurrentImagePos);
            }
            else
            {
                CloseMedia();
            }
            //Preloader.Visibility = Visibility.Hidden;
        }

        private void DeleteFile()
        {
            if (Files.Count() == 0)
            {
                return;
            }

            MainVideo.Close();

            var currentFile = Files[CurrentImagePos];
            File.Delete(currentFile);
            Files.Remove(currentFile);

            if (Files.Count() > 0)
            {
                ShowMediaByPos(CurrentImagePos);
            }
            else
            {
                CloseMedia();
            }            
        }

        private void ShowMediaByPos(int pos)
        {
            var fileName = Files[pos];
            var uri = new Uri(fileName);

            MainImage.Visibility = Visibility.Hidden;
            MainVideo.Visibility = Visibility.Hidden;

            var isImage = IsImage(fileName);
            if (isImage)
            {
                MainImage.Visibility = Visibility.Visible;
                var freeBmp = BitmapFromUri(uri);
                MainImage.Source = freeBmp;                
            }
            else
            {
                MainVideo.Visibility = Visibility.Visible;
                MainVideo.Source = uri;
            }
        }

        private void CloseMedia()
        {
            MainImage.Source = null;
            MainVideo.Close();
        }

        private Boolean IsImage(string path)
        {
            var ext = System.IO.Path.GetExtension(path).Replace(".", "");
            var isImg = ImageExts.Contains(ext);
            return isImg;
        }

        public ImageSource BitmapFromUri(Uri source)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = source;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }

        public void OpenDirectoryDialog()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result.ToString() != string.Empty)
                {
                    var dir = dialog.SelectedPath;
                    LoadDir(dir);                  
                }
            }
        }

        private void LoadDir(string dir)
        {
            Dir = dir;
            var filesResult = Directory.GetFiles(dir);
            Files.Clear();
            Files.AddRange(filesResult);

            var hasFiles = Files.Count() > 0;

            if (hasFiles)
            {
                ShowMediaByPos(0);
                CurrentImagePos = 0;
            }
        }

        
        private void MoveFile(string fullFile, string newDir)
        {
            var fileName = System.IO.Path.GetFileName(fullFile);
            var basePath = System.IO.Path.GetDirectoryName(fullFile);

            var newPath = System.IO.Path.Combine(basePath, newDir);
            var newFilePath = System.IO.Path.Combine(newPath, fileName);

            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }
           
            File.Move(fullFile, newFilePath);
            
            LastMovedFileDestination = newFilePath;
            LastMovedFileOrigin = fullFile;
            BtnMoveBack.Visibility = Visibility.Visible;
        }

        
    }
}
