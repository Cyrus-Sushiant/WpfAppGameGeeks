using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace WpfAppGameGeeks
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int score;
        private Nationality nationalityKey;
        private Dictionary<string, bool> images;
        private Point startPosition;
        private bool selectNationality;
        private bool isDragging;
        private Point clickPosition;
        private TranslateTransform originTT;
        private Storyboard storyboard;

        public MainWindow()
        {
            InitializeComponent();
            selectNationality = true;
            nationalityKey = Nationality.NotSet;
            LoadImages();

            // fade out item
            var animation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                FillBehavior = FillBehavior.Stop,
                BeginTime = TimeSpan.FromSeconds(0),
                Duration = new Duration(TimeSpan.FromSeconds(3))
            };

            storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, img);
            Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));
            storyboard.Completed += delegate { NextImage(); };
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            ClearScore();
            btnStart.Visibility = Visibility.Hidden;
            Uri resourceUri = new Uri(GetImageUri(), UriKind.Absolute);
            img.Source = new BitmapImage(resourceUri);
            img.Visibility = Visibility.Visible;
            Task.Delay(500);
            selectNationality = false;
            MoveToBottom();
        }

        private void img_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!selectNationality)
            {
                var draggableControl = sender as Image;
                originTT = draggableControl.RenderTransform as TranslateTransform ?? new TranslateTransform();
                isDragging = true;
                clickPosition = e.GetPosition(this);
                draggableControl.CaptureMouse();
                startPosition = btnStart.TransformToAncestor(MainGrid).Transform(new Point(0, 0));
            }
        }

        private void img_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!selectNationality)
            {
                isDragging = false;
                var draggable = sender as Image;
                draggable.ReleaseMouseCapture();
                MoveDestination();
            }
        }

        private void img_MouseMove(object sender, MouseEventArgs e)
        {
            if (!selectNationality)
            {
                var draggableControl = sender as Image;
                if (isDragging && draggableControl != null)
                {
                    Point currentPosition = e.GetPosition(this);
                    var transform = draggableControl.RenderTransform as TranslateTransform ?? new TranslateTransform();
                    transform.X = originTT.X + (currentPosition.X - clickPosition.X);
                    transform.Y = originTT.Y + (currentPosition.Y - clickPosition.Y);
                    draggableControl.RenderTransform = new TranslateTransform(transform.X, transform.Y);
                }
            }
        }

        private void MoveDestination()
        {
            Point currentPosition = img.TransformToAncestor(MainGrid).Transform(new Point(0, 0));
            var x = startPosition.X - currentPosition.X;
            var y = startPosition.Y - currentPosition.Y;
            if (Math.Abs(x) >= 20 && Math.Abs(y) >= 20)
            {
                var maxX = MainGrid.RenderSize.Width;
                var maxY = MainGrid.RenderSize.Height;

                selectNationality = true;
                if (x > 0 && y > 0)
                {
                    //Japanese
                    ScoreCalculation(Nationality.Japanese);
                    MoveTo(0, 0, fadeOut: true);
                }
                else if (x < 0 && y > 0)
                {
                    //Chinese
                    ScoreCalculation(Nationality.Chinese);
                    MoveTo(maxX, 0, fadeOut: true);
                }
                else if (x > 0 && y < 0)
                {
                    //Thai
                    ScoreCalculation(Nationality.Thai);
                    MoveTo(0, maxY, fadeOut: true);
                }
                else if (x < 0 && y < 0)
                {
                    //Korean
                    ScoreCalculation(Nationality.Korean);
                    MoveTo(maxX, maxY, fadeOut: true);
                }
            }
            else
            {
                img.RenderTransform = new TranslateTransform(startPosition.X, startPosition.Y);
                selectNationality = false;
                MoveTo(newY: 600);
            }
        }

        private void ScoreCalculation(Nationality nationality)
        {
            if (nationality == nationalityKey)
            {
                score += 20;
            }
            else
            {
                score -= 5;
            }
        }

        private void MoveTo(double? newX = null, double? newY = null, double animationDuration = 3, bool fadeOut = false, bool goToNextImage = false)
        {
            Vector offset = VisualTreeHelper.GetOffset(img);
            var top = offset.Y;
            var left = offset.X;

            TranslateTransform trans = new TranslateTransform();
            img.RenderTransform = trans;
            if (newX.HasValue)
            {
                DoubleAnimation animationX = new DoubleAnimation(newX.Value - left, TimeSpan.FromSeconds(animationDuration), FillBehavior.Stop);
                trans.BeginAnimation(TranslateTransform.XProperty, animationX);
            }

            if (newY.HasValue)
            {
                DoubleAnimation animationY = new DoubleAnimation(newY.Value - top, TimeSpan.FromSeconds(animationDuration), FillBehavior.Stop);
                if (!fadeOut && goToNextImage)
                {
                    animationY.Completed += delegate { NextImage(); };
                }
                trans.BeginAnimation(TranslateTransform.YProperty, animationY);
            }

            if (fadeOut)
            {
                storyboard.Begin();
            }
        }

        private void NextImage()
        {
            img.Visibility = Visibility.Hidden;
            storyboard.Stop();
            Task.Delay(1000);
            //get next image
            var imageUri = GetImageUri();

            if (string.IsNullOrEmpty(imageUri))
            {
                Point position = btnStart.TransformToAncestor(MainGrid).Transform(new Point(0, 0));
                MoveTo(position.X, position.Y, 0);

                lblScore.Content = $"Score: {score}";
                lblScore.Visibility = Visibility.Visible;
                btnStart.Visibility = Visibility.Visible;
            }
            else
            {
                // Show next image
                Uri resourceUri = new Uri(imageUri, UriKind.Absolute);
                img.Source = new BitmapImage(resourceUri);
                img.Visibility = Visibility.Visible;

                Point position = btnStart.TransformToAncestor(MainGrid).Transform(new Point(0, 0));
                MoveTo(position.X, position.Y, 0);

                img.Visibility = Visibility.Visible;
                Task.Delay(500);
                selectNationality = false;
                MoveToBottom();
            }

        }

        private void MoveToBottom()
        {
            Point position = lblScore.TransformToAncestor(MainGrid).Transform(new Point(0, 0));
            MoveTo(newY: position.Y + 100, goToNextImage: true);
        }

        private string GetImageUri()
        {
            var image = images.Where(i => !i.Value)
                .Select(i => i.Key)
                .OrderBy(emp => Guid.NewGuid())
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(image))
            {
                nationalityKey = Nationality.NotSet;
                var imgName = new System.IO.FileInfo(image).Name;

                foreach (var item in Enum.GetNames<Nationality>())
                {
                    if (imgName.Contains(item, StringComparison.OrdinalIgnoreCase))
                    {
                        nationalityKey = Enum.Parse<Nationality>(item);
                        break;
                    }
                }

                // set use flag
                images[image] = true;
            }

            return image;
        }

        private void ClearScore()
        {
            score = 0;
            foreach (var key in images.Keys)
            {
                images[key] = false;
            }

            lblScore.Visibility = Visibility.Hidden;
        }

        private void LoadImages()
        {
            var path = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Images");
            images = System.IO.Directory.GetFiles(path).ToDictionary(f => f, f => false);
        }
    }
}
