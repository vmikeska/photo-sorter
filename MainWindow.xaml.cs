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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using photo_sorter.ints;

namespace photo_sorter
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string Dir = "";
        public List<string> Files = new List<string>();
        public int CurrentImagePos = 0;

        protected TouchPoint TouchStart;
        private Boolean AlreadySwiped = false;

        private ConfigInt Config = null;

        public MainWindow()
        {
            InitializeComponent();
            this.TouchDown += new EventHandler<TouchEventArgs>(BasePage_TouchDown);
            this.TouchUp += new EventHandler<TouchEventArgs>(BasePage_TouchUp);
            this.TouchMove += new EventHandler<TouchEventArgs>(BasePage_TouchMove);

            this.Drop += MainWindow_Drop;

            LoadConfig();

            InitButtons();
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

             
        private void LoadConfig() {            
            using (StreamReader file = File.OpenText("config.json"))
            {
                var serializer = new JsonSerializer();
                Config = (ConfigInt)serializer.Deserialize(file, typeof(ConfigInt));
            }
        }

        private void InitButtons()
        {            
            foreach(var action in Config.actions)
            {
                var btn = new Button();
                btn.Content = action.name;
                btn.Tag = action;
                btn.FontSize = 25;
                btn.Margin = new Thickness(5,5,5,5);
                btn.Click += ActionBtn_Click;
                ActionsPanel.Children.Add(btn);
            }
        }

        private void ActionBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var action = btn.Tag as ButtonsInt;

            MoveCurrentFile(action.directory);
        }

        private void MoveCurrentFile(string newDir)
        {
            if (Files.Count() == 0)
            {
                return;
            }

            var currentFile = Files[CurrentImagePos];
            MoveFile(currentFile, newDir);
            Files.Remove(currentFile);

            if (Files.Count() > 0)
            {
                ShowImageByPos(CurrentImagePos);
            }       
            else
            {
                MainImage.Source = null;
            }
        }

        void BasePage_TouchDown(object sender, TouchEventArgs e)
        {
            TouchStart = e.GetTouchPoint(this);
        }

        void BasePage_TouchUp(object sender, TouchEventArgs e)
        {
            AlreadySwiped = false;
        }

        private int Sensitivity = 200;

        void BasePage_TouchMove(object sender, TouchEventArgs e)
        {

            if (TouchStart == null)
            {
                return;
            }

            if (!AlreadySwiped)
            {
                var Touch = e.GetTouchPoint(this);

                string direction = null;

                var xStart = TouchStart.Position.X;
                var yStart = TouchStart.Position.Y;

                var xCurrent = Touch.Position.X;
                var yCurrent = Touch.Position.Y;

                var xDiff = xCurrent - xStart;
                var yDiff = yCurrent - yStart;

                PosInfo.Text = $"x: {xDiff}, y: {yDiff}";

                //var anyDirectionThresholdReached = (Math.Abs(xDiff) > Sensitivity) | (Math.Abs(yDiff) > Sensitivity);

                var xLeftReached = xDiff < -Sensitivity;
                if (xLeftReached)
                {
                    direction = "LEFT";
                    AlreadySwiped = true;
                }

                var xRightReached = xDiff > Sensitivity;
                if (xRightReached)
                {
                    direction = "RIGHT";
                    AlreadySwiped = true;
                }

                var yTopReached = yDiff < -Sensitivity;
                if (yTopReached)
                {
                    direction = "TOP";
                    AlreadySwiped = true;
                }

                var yBottomReached = yDiff > Sensitivity;
                if (yBottomReached)
                {
                    direction = "BOTTOM";
                    AlreadySwiped = true;
                }


                //if (Touch.Position.X > (TouchStart.Position.X + Sensitivity))
                //{
                //    direction = "RIGHT";
                //    AlreadySwiped = true;
                //}

                //if (Touch.Position.X < (TouchStart.Position.X - Sensitivity))
                //{
                //    direction = "LEFT";
                //    AlreadySwiped = true;
                //}

                if (direction != null)
                {
                    MessageBox.Show(direction);
                    //var action = Config.actions.First(i => i.direction == direction);
                    //if (action != null)
                    //{
                    //    MoveCurrentFile(action.directory);
                    //}
                }
            }

            e.Handled = true;
        }

        public void openDialogClick(object sender, RoutedEventArgs e)
        {
            OpenDirectoryDialog();
        }

        public void nextClick(object sender, RoutedEventArgs e)
        {
            var isLastPos = CurrentImagePos >= Files.Count() - 1;
            if (isLastPos)
            {
                return;
            }

            var nextPos = CurrentImagePos + 1;
            ShowImageByPos(nextPos);
            CurrentImagePos = nextPos;
        }

        public void backClick(object sender, RoutedEventArgs e)
        {
            if (CurrentImagePos == 0)
            {
                return;
            }

            var prevPos = CurrentImagePos - 1;
            ShowImageByPos(prevPos);
            CurrentImagePos = prevPos;
        }

        private void ShowImageByPos(int pos)
        {
            var fileName = Files[pos];
            var uri = new Uri(fileName);

            var freeBmp = BitmapFromUri(uri);
            MainImage.Source = freeBmp;
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
                ShowImageByPos(0);
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
        }
    }
}
