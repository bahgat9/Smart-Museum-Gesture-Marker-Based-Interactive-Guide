using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace GestureGUI.Pages
{
    public partial class Page3 : UserControl
    {
        public event Action OnTutorialFinished;

        public Page3()
        {
            InitializeComponent();
            PlayTutorial();
        }

        public void ShowAction(string action)
        {
            
        }

        private void PlayTutorial()
        {
            try
            {
                string videoPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Videos",
                    "tutorial.mp4"
                );

                if (!File.Exists(videoPath))
                {
                    StatusText.Text = "Tutorial video not found.";
                    return;
                }

                TutorialPlayer.Source = new Uri(videoPath, UriKind.Absolute);
                TutorialPlayer.Play();
                StatusText.Text = "Tutorial is playing...";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error playing tutorial: " + ex.Message;
            }
        }

        private void TutorialPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            OnTutorialFinished?.Invoke();
        }

        private void TutorialPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            StatusText.Text = "Failed to play tutorial.";
        }
    }
}