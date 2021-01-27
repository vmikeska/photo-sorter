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

namespace photo_sorter
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string dir = "";
        public List<string> files = new List<string>();
        public int currentImagePos = 0;

        protected TouchPoint TouchStart;
        private Boolean AlreadySwiped = false;

        public MainWindow()
        {
            InitializeComponent();
            this.TouchDown += new EventHandler<TouchEventArgs>(BasePage_TouchDown);
            this.TouchUp += new EventHandler<TouchEventArgs>(BasePage_TouchUp);
            this.TouchMove += new EventHandler<TouchEventArgs>(BasePage_TouchMove);
        }

        void BasePage_TouchDown(object sender, TouchEventArgs e)
        {
            TouchStart = e.GetTouchPoint(this);
        }

        void BasePage_TouchUp(object sender, TouchEventArgs e)
        {
            AlreadySwiped = false;
        }

        private int sensitivity = 50;

        void BasePage_TouchMove(object sender, TouchEventArgs e)
        {
            if (!AlreadySwiped)
            {
                var Touch = e.GetTouchPoint(this);

                if (TouchStart != null && Touch.Position.X > (TouchStart.Position.X + sensitivity))
                {
                    MessageBox.Show("Hello, world!", "right");
                    AlreadySwiped = true;
                }

                if (TouchStart != null && Touch.Position.X < (TouchStart.Position.X - sensitivity))
                {
                    MessageBox.Show("Hello, world!", "left");
                    AlreadySwiped = true;
                }
            }

            e.Handled = true;
        }

        public void openDialogClick(object sender, RoutedEventArgs e)
        {
            openDirectoryDialog();
        }

        public void nextClick(object sender, RoutedEventArgs e)
        {
            var isLastPos = currentImagePos >= files.Count() - 1;
            if (isLastPos)
            {
                return;
            }

            var nextPos = currentImagePos + 1;
            showImageByPos(nextPos);
            currentImagePos = nextPos;
        }

        public void backClick(object sender, RoutedEventArgs e)
        {
            if (currentImagePos == 0)
            {
                return;
            }

            var prevPos = currentImagePos - 1;
            showImageByPos(prevPos);
            currentImagePos = prevPos;
        }

        public void yesClick(object sender, RoutedEventArgs e)
        {
            var currentFile = files[currentImagePos];
            moveFile(currentFile, "yes");
            files.Remove(currentFile);
            showImageByPos(currentImagePos);
        }

        public void noClick(object sender, RoutedEventArgs e)
        {
            var currentFile = files[currentImagePos];
            moveFile(currentFile, "no");
            files.Remove(currentFile);
            showImageByPos(currentImagePos);
        }




        private void showImageByPos(int pos)
        {
            var fileName = files[pos];
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

        public void openDirectoryDialog()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result.ToString() != string.Empty)
                {
                    dir = dialog.SelectedPath;
                    var filesResult = Directory.GetFiles(dir);
                    files.Clear();
                    files.AddRange(filesResult);

                    var hasFiles = files.Count() > 0;

                    if (hasFiles)
                    {
                        showImageByPos(0);
                    }
                  
                }
            }
        }

        private void moveFile(string fullFile, string newDir)
        {
            var fileName = System.IO.Path.GetFileName(fullFile);
            var basePath = System.IO.Path.GetDirectoryName(fullFile);

            var newPath = System.IO.Path.Combine(basePath, newDir);
            var newFilePath = System.IO.Path.Combine(newPath, fileName);

            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }
           
            //MainImage.Source = null;

            File.Move(fullFile, newFilePath);

        }

        




    }
}
