using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace GestureGUI.Pages
{
    public partial class Page1 : UserControl
    {
        private static Process _tuioProcess;
        private static bool _tuioStarted = false;

        private readonly MediaPlayer _player = new MediaPlayer();

        private int _currentMarkerId = -1;
        private int _lastRotation = 0;
        private double _currentZoom = 1.0;
        private int _currentVolume = 50;

        public Page1()
        {
            InitializeComponent();

            _player.Volume = 0.5;
            VolumeText.Text = "Volume: 50";

            RestoreSelectedUser();
            StartTuioProjectIfNeeded();
        }

        public void UpdateMarker(string artifact, int markerId, int rotation, string status)
        {
            ArtifactText.Text = $"Artifact: {artifact}";
            MarkerIdText.Text = $"Marker ID: {markerId}";
            RotationText.Text = $"Rotation: {rotation}°";
            StatusValueText.Text = $"Status: {status}";

            RestoreSelectedUser();

            if (status == "added")
            {
                HandleMarkerAdded(markerId, artifact, rotation);
            }
            else if (status == "updated")
            {
                HandleMarkerUpdated(markerId, artifact, rotation);
            }
            else if (status == "removed")
            {
                HandleMarkerRemoved(markerId, artifact);
            }

            _currentMarkerId = markerId;
            _lastRotation = rotation;
        }

        public void ShowAction(string action)
        {
            ActionText.Text = action;
            HandleActionMedia(action);
        }

        private void RestoreSelectedUser()
        {
            if (!string.IsNullOrWhiteSpace(AppState.SelectedUserName))
            {
                SelectedUserText.Text = $"User: {AppState.SelectedUserName}";
                SelectedTypeText.Text = $"Type: {AppState.SelectedUserType}";
                WelcomeText.Text = GetWelcomeMessage(AppState.SelectedUserType, AppState.SelectedUserName);
            }
            else
            {
                SelectedUserText.Text = "User: No user selected";
                SelectedTypeText.Text = "Type: -";
                WelcomeText.Text = "";
            }
        }

        private string GetWelcomeMessage(string userType, string userName)
        {
            switch (userType)
            {
                case "VIP":
                    return $"Welcome {userName}! You can access exclusive VIP museum content.";
                case "Premium":
                    return $"Welcome {userName}! You can access premium guided museum content.";
                default:
                    return "";
            }
        }

        private void HandleMarkerAdded(int markerId, string artifact, int rotation)
        {
            _currentZoom = 1.0;
            ArtifactScaleTransform.ScaleX = 1.0;
            ArtifactScaleTransform.ScaleY = 1.0;
            ArtifactRotateTransform.Angle = 0;

            UpdateArtifactImage(artifact);

            if (markerId == 3)
            {
                _currentVolume = 50;
                VolumeText.Text = "Volume: 50";
            }
        }

        private void HandleMarkerUpdated(int markerId, string artifact, int rotation)
        {
            int delta = rotation - _lastRotation;

            if (delta > 180) delta -= 360;
            if (delta < -180) delta += 360;

            if (markerId == 1)
            {
                _currentZoom += delta * 0.02;
                if (_currentZoom < 0.5) _currentZoom = 0.5;
                if (_currentZoom > 3.0) _currentZoom = 3.0;

                ArtifactScaleTransform.ScaleX = _currentZoom;
                ArtifactScaleTransform.ScaleY = _currentZoom;

                ArtifactRotateTransform.Angle = 0;
            }
            else if (markerId == 2)
            {
                ArtifactRotateTransform.Angle = 0;

                if (rotation < 180)
                {
                    ArtifactText.Text = "Artifact: Ancient Egyptian Bow";
                    UpdateArtifactImageByFileNames("weapon.png", "weapon.jpg");
                }
                else
                {
                    ArtifactText.Text = "Artifact: Khopesh Sword";
                    UpdateArtifactImageByFileNames("weapon2.png", "weapon2.jpg");
                }
            }
            else if (markerId == 3)
            {
                ArtifactRotateTransform.Angle = 0;

                _currentVolume += (int)(delta * 1.2);

                if (_currentVolume < 0) _currentVolume = 0;
                if (_currentVolume > 100) _currentVolume = 100;

                _player.Volume = _currentVolume / 100.0;
                VolumeText.Text = $"Volume: {_currentVolume}";
            }
            else if (markerId == 4)
            {
                _currentZoom += delta * 0.02;
                if (_currentZoom < 0.5) _currentZoom = 0.5;
                if (_currentZoom > 3.0) _currentZoom = 3.0;

                ArtifactScaleTransform.ScaleX = _currentZoom;
                ArtifactScaleTransform.ScaleY = _currentZoom;

                ArtifactRotateTransform.Angle = 0; 
            }
        }

        private void HandleMarkerRemoved(int markerId, string artifact)
        {
            if (markerId == 3)
            {
                _player.Stop();
                VolumeText.Text = $"Volume: {_currentVolume}";
            }

            ArtifactScaleTransform.ScaleX = 1.0;
            ArtifactScaleTransform.ScaleY = 1.0;
            ArtifactRotateTransform.Angle = 0;

            _currentZoom = 1.0;
            _currentMarkerId = -1;
        }

        private void UpdateArtifactImage(string artifact)
        {
            try
            {
                string imagePath = FindArtifactImagePath(artifact);

                if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
                {
                    ArtifactImage.Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
                }
                else
                {
                    ArtifactImage.Source = null;
                    ActionText.Text = $"Image not found for artifact: {artifact}";
                }
            }
            catch (Exception ex)
            {
                ArtifactImage.Source = null;
                ActionText.Text = "Error loading image: " + ex.Message;
            }
        }

        private void UpdateArtifactImageByFileNames(params string[] fileNames)
        {
            try
            {
                string imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                string imagePath = FindFirstExisting(imagesFolder, fileNames);

                if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
                {
                    ArtifactImage.Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
                }
            }
            catch
            {
            }
        }

        private string FindArtifactImagePath(string artifact)
        {
            string imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

            switch (artifact)
            {
                case "Pharaoh Statue":
                    return FindFirstExisting(imagesFolder,
                        "pharaoh.jpg",
                        "statue.jpg",
                        "pharaoh.png",
                        "statue.png");

                case "Ancient Egyptian Weapons":
                    return FindFirstExisting(imagesFolder,
                        "weapon.png",
                        "weapon.jpg",
                        "weapon2.png",
                        "weapon2.jpg");

                case "Audio Guide":
                    return FindFirstExisting(imagesFolder,
                        "audio.png",
                        "audio.jpg");

                case "Museum Map":
                    return FindFirstExisting(imagesFolder,
                        "background.jpeg",
                        "background.jpg",
                        "map.jpeg",
                        "map.jpg",
                        "museum_map.png");

                default:
                    return null;
            }
        }

        private string FindFirstExisting(string folder, params string[] fileNames)
        {
            foreach (string fileName in fileNames)
            {
                string path = Path.Combine(folder, fileName);
                if (File.Exists(path))
                    return path;
            }

            return null;
        }

        private void HandleActionMedia(string action)
        {
            try
            {
                string audioFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audio");
                string audioPath = null;

                switch (action)
                {
                    case "Start Audio Guide":
                    case "Play Audio Guide":
                        audioPath = FindFirstExisting(audioFolder,
                            "audio-guide.mp3",
                            "audio.mp3");
                        break;

                    case "Play Pharaoh Video":
                    case "Play Pharaoh Audio":
                        audioPath = FindFirstExisting(audioFolder,
                            "pharaoh.mp3",
                            "statue.mp3");
                        break;

                    case "Play Weapons Video":
                    case "Play Weapons Audio":
                        audioPath = FindFirstExisting(audioFolder,
                            "weapons.mp3",
                            "weapon.mp3");
                        break;

                    case "Open Museum Map":
                    case "Stop Audio":
                        _player.Stop();
                        return;

                    default:
                        return;
                }

                if (!string.IsNullOrWhiteSpace(audioPath) && File.Exists(audioPath))
                {
                    _player.Stop();
                    _player.Open(new Uri(audioPath, UriKind.Absolute));
                    _player.Play();
                }
                else
                {
                    ActionText.Text = $"Audio not found for action: {action}";
                }
            }
            catch (Exception ex)
            {
                ActionText.Text = "Error playing audio: " + ex.Message;
            }
        }

        private void StartTuioProjectIfNeeded()
        {
            try
            {
                if (_tuioStarted && _tuioProcess != null && !_tuioProcess.HasExited)
                    return;

                string exePath = @"E:\Nour\Nour\TUIO11_NET-master\bin\Debug\TuioDemo.exe";

                if (!File.Exists(exePath))
                {
                    ActionText.Text = "TUIO app executable not found.";
                    return;
                }

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Minimized
                };

                _tuioProcess = Process.Start(psi);
                _tuioStarted = true;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Window mainWindow = Window.GetWindow(this);
                    if (mainWindow != null)
                    {
                        mainWindow.Activate();
                        mainWindow.Topmost = true;
                        mainWindow.Topmost = false;
                        mainWindow.Focus();
                    }
                }), DispatcherPriority.ApplicationIdle);
            }
            catch (Exception ex)
            {
                ActionText.Text = "Error starting TUIO app: " + ex.Message;
            }
        }
    }
}