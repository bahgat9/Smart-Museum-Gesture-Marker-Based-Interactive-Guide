using System;
using System.IO;
using WMPLib;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

public class Central_Function_Manager
{
    private TuioDemo demo;
    private WindowsMediaPlayer player;
    private int currentMarker = -1;

    ///socket///
    private const string SOCKET_HOST = "127.0.0.1";
    private const int SOCKET_PORT = 8000;


    private void SendMarkerToIntegration(string json)
    {
        try
        {
            TcpClient client = new TcpClient(SOCKET_HOST, SOCKET_PORT);
            NetworkStream stream = client.GetStream();

            byte[] bytes = Encoding.UTF8.GetBytes(json + "\n");
            stream.Write(bytes, 0, bytes.Length);

            stream.Close();
            client.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Send to integration server error: " + ex.Message);
        }
    }
    ///socket///
    private Dictionary<int, float> markerAngles = new Dictionary<int, float>();

    private Dictionary<int, string> artifactMap = new Dictionary<int, string>()
    {
        { 1, "Pharaoh Statue" },
        { 2, "Ancient Egyptian Weapons" },
        { 3, "Audio Guide" },
        { 4, "Museum Map" }
    };

    public Central_Function_Manager(TuioDemo demoForm)
    {
        demo = demoForm;

        player = new WindowsMediaPlayer();
        player.settings.volume = 50;
        player.settings.autoStart = false;
    }

    public void ExecuteMarkerAction(int markerId, float angle)
    {
        currentMarker = markerId;
        markerAngles[markerId] = angle;

        MarkerData data = GetMarkerData(markerId, angle);
        demo.SetMarkerData(data);
        PrintMarkerJson("added", data);

        if (markerId == 1)
        {
            demo.Zoom = 1.0f;
            demo.SetMarkerContent(
                1,
                "Pharaoh Statue",
                "Rotate marker to zoom statue.",
                "statue.jpg"
            );
        }
        else if (markerId == 2)
        {
            demo.SetMarkerContent(
                2,
                "Ancient Egyptian Weapons",
                "Rotate marker to switch weapon.",
                "weapon.png"
            );
        }
        else if (markerId == 3)
        {
            demo.SetMarkerContent(
                3,
                "Audio Guide",
                "Rotate marker to control volume.",
                "audio.png"
            );
            PlayAudio();
        }
        else if (markerId == 4)
        {
            demo.Zoom = 1.0f;
            demo.SetMarkerContent(
                4,
                "Museum Map",
                "Rotate marker to zoom map.",
                "background.jpeg"
            );
        }
        else
        {
            demo.SetMarkerContent(
                markerId,
                "Unknown Marker",
                "No mapped artifact found.",
                ""
            );
        }

        demo.SafeRefresh();
    }

    public void HandleMarkerUpdated(int markerId, float angle)
    {
        float oldAngle = angle;

        if (markerAngles.ContainsKey(markerId))
            oldAngle = markerAngles[markerId];

        float delta = angle - oldAngle;

        if (delta > Math.PI) delta -= (float)(2 * Math.PI);
        if (delta < -Math.PI) delta += (float)(2 * Math.PI);

        markerAngles[markerId] = angle;

        MarkerData data = GetMarkerData(markerId, angle);
        demo.SetMarkerData(data);
        PrintMarkerJson("updated", data);

        if (markerId == 1)
        {
            demo.Zoom += delta * 2.5f;

            if (demo.Zoom < 0.5f) demo.Zoom = 0.5f;
            if (demo.Zoom > 3.0f) demo.Zoom = 3.0f;
        }
        else if (markerId == 2)
        {
            if (angle < Math.PI)
            {
                demo.SetMarkerContent(
                    2,
                    "Ancient Egyptian Bow",
                    "A bow used by ancient Egyptian soldiers.",
                    "weapon.png"
                );
            }
            else
            {
                demo.SetMarkerContent(
                    2,
                    "Khopesh Sword",
                    "A curved sword used in ancient Egypt.",
                    "weapon2.png"
                );
            }
        }
        else if (markerId == 3)
        {
            int vol = player.settings.volume + (int)(delta * 120);

            if (vol < 0) vol = 0;
            if (vol > 100) vol = 100;

            player.settings.volume = vol;
            demo.Volume = vol;
        }
        else if (markerId == 4)
        {
            demo.Zoom += delta * 2.5f;

            if (demo.Zoom < 0.5f) demo.Zoom = 0.5f;
            if (demo.Zoom > 3.0f) demo.Zoom = 3.0f;
        }

        demo.SafeRefresh();
    }

    public void HandleMarkerRemoved(int markerId)
    {
        if (markerId == 3)
            player.controls.stop();

        MarkerData data = new MarkerData
        {
            marker_id = markerId,
            rotation = 0,
            artifact = artifactMap.ContainsKey(markerId) ? artifactMap[markerId] : "Unknown"
        };

        PrintMarkerJson("removed", data);

        markerAngles.Remove(markerId);

        if (currentMarker == markerId)
            currentMarker = -1;

        demo.ClearMarkerContent();
        demo.SafeRefresh();
    }

    public MarkerData GetMarkerData(int markerId, float angle)
    {
        float degrees = angle * 180f / (float)Math.PI;

        if (degrees < 0)
            degrees += 360f;

        string artifact = "Unknown";

        if (artifactMap.ContainsKey(markerId))
            artifact = artifactMap[markerId];

        return new MarkerData
        {
            marker_id = markerId,
            rotation = degrees,
            artifact = artifact
        };
    }

    private void PrintMarkerJson(string status, MarkerData data)
    {
        string json =
            "{"
            + "\"source\":\"marker\","
            + "\"status\":\"" + status + "\","
            + "\"marker_id\":" + data.marker_id + ","
            + "\"rotation\":" + ((int)data.rotation) + ","
            + "\"artifact\":\"" + data.artifact + "\""
            + "}";

        Console.WriteLine(json);
        SendMarkerToIntegration(json);///socket///
    }

    private void PlayAudio()
    {
        string path = Path.Combine(Environment.CurrentDirectory, "audio-guide.mp3");

        if (File.Exists(path))
        {
            if (player.URL != path)
                player.URL = path;

            player.controls.play();
        }
        else
        {
            Console.WriteLine("Audio file not found: " + path);
        }
    }
}