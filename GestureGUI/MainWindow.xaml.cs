using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
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
using GestureGUI.Pages;

namespace GestureGUI
{
    public partial class MainWindow : Window
    {
        private Page1 _page1;
        private Page2 _page2;
        private Page3 _page3;

        private TcpListener _actionServer;
        private Thread _actionServerThread;

        private TcpListener _bluetoothServer;
        private Thread _bluetoothServerThread;

        private TcpListener _tuioServer;
        private Thread _tuioServerThread;

        private TcpListener _server;
        private Thread _serverThread;

        private double lastX = -1;
        private double lastY = -1;

        private double rawX = 400;
        private double rawY = 300;

        private double smoothX = 400;
        private double smoothY = 300;

        private const double sensitivity = 1.5;
        private const double deadZone = 1.2;
        private const double smoothing = 0.15;

        private DateTime pinchStart;
        private bool isPinching = false;
        private const int CLICK_THRESHOLD_MS = 300;

        public MainWindow()
        {
            InitializeComponent();

            var landing = new LandingPage();
            landing.OnPageSelected += HandlePageSelection;
            MainContent.Content = landing;

            StartServer();         
            StartActionServer();   
            StartBluetoothServer();
            StartTuioServer();     
        }


        private void StartActionServer()
        {
            _actionServer = new TcpListener(IPAddress.Any, 6001);
            _actionServer.Start();

            _actionServerThread = new Thread(() => ListenLoop(_actionServer, HandleActionMessage));
            _actionServerThread.IsBackground = true;
            _actionServerThread.Start();
        }

        private void StartBluetoothServer()
        {
            _bluetoothServer = new TcpListener(IPAddress.Any, 6002);
            _bluetoothServer.Start();

            _bluetoothServerThread = new Thread(() => ListenLoop(_bluetoothServer, HandleBluetoothMessage));
            _bluetoothServerThread.IsBackground = true;
            _bluetoothServerThread.Start();
        }

        private void StartTuioServer()
        {
            _tuioServer = new TcpListener(IPAddress.Any, 6003);
            _tuioServer.Start();

            _tuioServerThread = new Thread(() => ListenLoop(_tuioServer, HandleTuioMessage));
            _tuioServerThread.IsBackground = true;
            _tuioServerThread.Start();
        }

        private void ListenLoop(TcpListener listener, Action<string> handler)
        {
            while (true)
            {
                var client = listener.AcceptTcpClient();
                var reader = new StreamReader(client.GetStream());

                while (!reader.EndOfStream)
                {
                    string message = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(message))
                        handler(message);
                }

                client.Close();
            }
        }

        private void HandleBluetoothMessage(string msg)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = "Bluetooth: " + msg;

                if (_page2 != null)
                {
                    string userName = ExtractJsonValue(msg, "user_name");
                    string userType = ExtractJsonValue(msg, "user_type");
                    string deviceName = ExtractJsonValue(msg, "device_name");

                    _page2.UpdateBluetoothUser(userName, userType, deviceName);
                }
            });
        }

        private void HandleTuioMessage(string msg)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = "TUIO: " + msg;

                if (_page1 != null)
                {
                    string artifact = ExtractJsonValue(msg, "artifact");
                    string markerIdText = ExtractJsonValue(msg, "marker_id");
                    string rotationText = ExtractJsonValue(msg, "rotation");
                    string status = ExtractJsonValue(msg, "status");

                    int markerId = 0;
                    int rotation = 0;

                    int.TryParse(markerIdText, out markerId);
                    int.TryParse(rotationText, out rotation);

                    _page1.UpdateMarker(artifact, markerId, rotation, status);
                }
            });
        }

        private void HandleActionMessage(string msg)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = "Action: " + msg;

                string action = ExtractJsonValue(msg, "action");

                if (!string.IsNullOrWhiteSpace(action))
                {
                    if (_page1 != null)
                        _page1.ShowAction(action);

                    if (_page2 != null)
                        _page2.ShowAction(action);

                    if (_page3 != null)
                        _page3.ShowAction(action);
                }
            });
        }

        private string ExtractJsonValue(string json, string key)
        {
            try
            {
                string pattern = "\"" + key + "\":";

                int start = json.IndexOf(pattern);
                if (start == -1) return "";

                start += pattern.Length;

                while (start < json.Length && json[start] == ' ')
                    start++;

                if (start < json.Length && json[start] == '\"')
                {
                    start++;
                    int end = json.IndexOf("\"", start);
                    if (end == -1) return "";
                    return json.Substring(start, end - start);
                }
                else
                {
                    int end = json.IndexOfAny(new char[] { ',', '}' }, start);
                    if (end == -1) end = json.Length;
                    return json.Substring(start, end - start).Trim();
                }
            }
            catch
            {
                return "";
            }
        }


        private void StartServer()
        {
            _server = new TcpListener(IPAddress.Any, 6000);
            _server.Start();

            _serverThread = new Thread(ServerLoop);
            _serverThread.IsBackground = true;
            _serverThread.Start();
        }

        private void ServerLoop()
        {
            while (true)
            {
                var client = _server.AcceptTcpClient();

                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.IsBackground = true;
                clientThread.Start();
            }
        }
        private void HandleClient(TcpClient client)
        {
            try
            {
                var reader = new StreamReader(client.GetStream());

                while (!reader.EndOfStream)
                {
                    string message = reader.ReadLine();

                    if (!string.IsNullOrEmpty(message))
                    {
                        HandleMessage(message);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Client error: " + e.Message);
            }
            finally
            {
                client.Close();
            }
        }
        private void HandleMessage(string msg)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = msg;

                if (msg.StartsWith("MOVE:"))
                {
                    HandleMove(msg);
                }
                else if (msg.StartsWith("PINCH:"))
                {
                    HandlePinch(msg);
                }
                else
                {
                    HandleGesture(msg);
                }
            });
        }

        private void HandleMove(string msg)
        {
            try
            {
                var parts = msg.Replace("MOVE:", "").Split(',');
                double x = double.Parse(parts[0]);
                double y = double.Parse(parts[1]);

                if (lastX < 0 || lastY < 0)
                {
                    lastX = x;
                    lastY = y;
                    return;
                }

                double dx = x - lastX;
                double dy = y - lastY;

                lastX = x;
                lastY = y;

                if (Math.Abs(dx) < deadZone) dx = 0;
                if (Math.Abs(dy) < deadZone) dy = 0;

                rawX += dx * sensitivity;
                rawY += dy * sensitivity;

                rawX = Math.Max(0, Math.Min(OverlayCanvas.ActualWidth - 20, rawX));
                rawY = Math.Max(0, Math.Min(OverlayCanvas.ActualHeight - 20, rawY));

                smoothX += (rawX - smoothX) * smoothing;
                smoothY += (rawY - smoothY) * smoothing;

                Canvas.SetLeft(Cursor, smoothX);
                Canvas.SetTop(Cursor, smoothY);
            }
            catch { }
        }

        private void HandlePinch(string msg)
        {
            bool pinchNow = msg.Contains("1");

            if (pinchNow && !isPinching)
            {
                isPinching = true;
                pinchStart = DateTime.Now;
            }
            else if (!pinchNow && isPinching)
            {
                isPinching = false;

                var duration = (DateTime.Now - pinchStart).TotalMilliseconds;

                if (duration < CLICK_THRESHOLD_MS)
                {
                    HandleClick();
                }
            }
        }

        private void HandleClick()
        {
            StatusText.Text = "CLICK";

            Point overlayPoint = new Point(smoothX + 10, smoothY + 10);

            GeneralTransform transform = OverlayCanvas.TransformToVisual(MainContent);
            Point contentPoint = transform.Transform(overlayPoint);

            HitTestResult result = VisualTreeHelper.HitTest(MainContent, contentPoint);

            if (result != null)
            {
                FrameworkElement element = result.VisualHit as FrameworkElement;

                while (element != null)
                {
                    if (element is Button btn)
                    {
                        btn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        StatusText.Text = "BUTTON CLICKED ✅";
                        return;
                    }

                    if (element is System.Windows.Shapes.Path path)
                    {
                        string tag = path.Tag?.ToString();

                        if (!string.IsNullOrEmpty(tag))
                        {
                            StatusText.Text = $"SLICE CLICKED: {tag}";
                            HandlePageSelection(tag);
                            return;
                        }
                    }

                    element = VisualTreeHelper.GetParent(element) as FrameworkElement;
                }
            }

            StatusText.Text = "Nothing clicked";
        }

        private void HandleGesture(string gesture)
        {
            StatusText.Text = $"Gesture RAW: [{gesture}]";

            string cleanGesture = gesture.Trim().ToLower();

            if (cleanGesture == "circle")
            {
                StatusText.Text = "Circle detected → Returning to menu";
                ShowLandingPage();
            }
        }

        private void HandlePageSelection(string page)
        {
            StatusText.Text = $"Selected: {page}";

            switch (page)
            {
                case "TUIO":
                    _page1 = new Page1();
                    MainContent.Content = _page1;
                    break;

                case "Bluetooth":
                    _page2 = new Page2();
                    MainContent.Content = _page2;
                    break;

                case "Tutorial":
                    _page3 = new Page3();
                    _page3.OnTutorialFinished += ShowLandingPage;
                    MainContent.Content = _page3;
                    break;
            }
        }

        private void ShowLandingPage()
        {

            _page1 = null;
            _page2 = null;
            _page3 = null;

            var landing = new LandingPage();
            landing.OnPageSelected += HandlePageSelection;

            MainContent.Content = landing;
        }
    }
}
