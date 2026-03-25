using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using TUIO;


public class TuioDemo : Form, TuioListener
{
    private TuioClient client;
    private Dictionary<long, TuioObject> objectList;
    private Dictionary<long, TuioCursor> cursorList;
    private Dictionary<long, TuioBlob> blobList;
    private Central_Function_Manager manager;

    public static int width, height;
    private int window_width = 1000;
    private int window_height = 650;
    private int window_left = 0;
    private int window_top = 0;
    private int screen_width = Screen.PrimaryScreen.Bounds.Width;
    private int screen_height = Screen.PrimaryScreen.Bounds.Height;

    private bool fullscreen;
    private bool verbose;

    private Font font = new Font("Arial", 10.0f);
    private Font titleFont = new Font("Arial", 16.0f, FontStyle.Bold);
    private Font infoFont = new Font("Arial", 11.0f);

    private SolidBrush fntBrush = new SolidBrush(Color.White);
    private SolidBrush bgrBrush = new SolidBrush(Color.FromArgb(0, 0, 64));
    private SolidBrush curBrush = new SolidBrush(Color.FromArgb(192, 0, 192));
    private SolidBrush blbBrush = new SolidBrush(Color.FromArgb(64, 64, 64));
    private Pen curPen = new Pen(new SolidBrush(Color.Blue), 1);

    public float Zoom { get; set; } = 1.0f;
    public int Volume { get; set; } = 50;

    private MarkerData currentMarkerData;
    private string currentTitle = "";
    private string currentDescription = "";
    private string currentImagePath = "";

    public TuioDemo(int port)
    {
        verbose = false;
        fullscreen = false;
        width = window_width;
        height = window_height;

        this.ClientSize = new Size(width, height);
        this.Name = "TuioDemo";
        this.Text = "TuioDemo";

        this.Closing += new CancelEventHandler(Form_Closing);
        this.KeyDown += new KeyEventHandler(Form_KeyDown);

        this.SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.DoubleBuffer, true
        );

        objectList = new Dictionary<long, TuioObject>(128);
        cursorList = new Dictionary<long, TuioCursor>(128);
        blobList = new Dictionary<long, TuioBlob>(128);

        manager = new Central_Function_Manager(this);

        client = new TuioClient(port);
        client.addTuioListener(this);
        client.connect();
    }

    public void SetMarkerData(MarkerData data)
    {
        currentMarkerData = data;
    }

    public void SetMarkerContent(int markerId, string title, string description, string imagePath)
    {
        currentTitle = title;
        currentDescription = description;
        currentImagePath = imagePath;
    }

    public void ClearMarkerContent()
    {
        currentMarkerData = null;
        currentTitle = "";
        currentDescription = "";
        currentImagePath = "";
        Zoom = 1.0f;
    }

    public void SafeRefresh()
    {
        if (this.IsHandleCreated && this.InvokeRequired)
            this.BeginInvoke((MethodInvoker)(() => Invalidate()));
        else
            Invalidate();
    }

    private void Form_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyData == Keys.F1)
        {
            if (!fullscreen)
            {
                width = screen_width;
                height = screen_height;

                window_left = this.Left;
                window_top = this.Top;

                this.FormBorderStyle = FormBorderStyle.None;
                this.Left = 0;
                this.Top = 0;
                this.Width = screen_width;
                this.Height = screen_height;

                fullscreen = true;
            }
            else
            {
                width = window_width;
                height = window_height;

                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.Left = window_left;
                this.Top = window_top;
                this.Width = window_width;
                this.Height = window_height;

                fullscreen = false;
            }

            Invalidate();
        }
        else if (e.KeyData == Keys.Escape)
        {
            this.Close();
        }
        else if (e.KeyData == Keys.V)
        {
            verbose = !verbose;
        }
    }

    private void Form_Closing(object sender, CancelEventArgs e)
    {
        client.removeTuioListener(this);
        client.disconnect();
        Environment.Exit(0);
    }

    public void addTuioObject(TuioObject o)
    {
        lock (objectList)
        {
            objectList[o.SessionID] = o;
        }

        manager.ExecuteMarkerAction(o.SymbolID, o.Angle);

        if (verbose)
            Console.WriteLine("add obj " + o.SymbolID + " (" + o.SessionID + ") " + o.X + " " + o.Y + " " + o.Angle);
    }

    public void updateTuioObject(TuioObject o)
    {
        lock (objectList)
        {
            objectList[o.SessionID] = o;
        }

        manager.HandleMarkerUpdated(o.SymbolID, o.Angle);

        if (verbose)
            Console.WriteLine("set obj " + o.SymbolID + " " + o.SessionID + " " + o.X + " " + o.Y + " " + o.Angle);
    }

    public void removeTuioObject(TuioObject o)
    {
        lock (objectList)
        {
            objectList.Remove(o.SessionID);
        }

        manager.HandleMarkerRemoved(o.SymbolID);

        if (verbose)
            Console.WriteLine("del obj " + o.SymbolID + " (" + o.SessionID + ")");
    }

    public void addTuioCursor(TuioCursor c)
    {
        lock (cursorList)
        {
            cursorList[c.SessionID] = c;
        }

        if (verbose)
            Console.WriteLine("add cur " + c.CursorID + " (" + c.SessionID + ") " + c.X + " " + c.Y);
    }

    public void updateTuioCursor(TuioCursor c)
    {
        if (verbose)
            Console.WriteLine("set cur " + c.CursorID + " (" + c.SessionID + ") " + c.X + " " + c.Y);
    }

    public void removeTuioCursor(TuioCursor c)
    {
        lock (cursorList)
        {
            cursorList.Remove(c.SessionID);
        }

        if (verbose)
            Console.WriteLine("del cur " + c.CursorID + " (" + c.SessionID + ")");
    }

    public void addTuioBlob(TuioBlob b)
    {
        lock (blobList)
        {
            blobList[b.SessionID] = b;
        }

        if (verbose)
            Console.WriteLine("add blb " + b.BlobID + " (" + b.SessionID + ")");
    }

    public void updateTuioBlob(TuioBlob b)
    {
        if (verbose)
            Console.WriteLine("set blb " + b.BlobID + " (" + b.SessionID + ")");
    }

    public void removeTuioBlob(TuioBlob b)
    {
        lock (blobList)
        {
            blobList.Remove(b.SessionID);
        }

        if (verbose)
            Console.WriteLine("del blb " + b.BlobID + " (" + b.SessionID + ")");
    }

    public void refresh(TuioTime frameTime)
    {
        SafeRefresh();
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        Graphics g = pevent.Graphics;
        g.FillRectangle(bgrBrush, new Rectangle(0, 0, width, height));

        DrawCursorPath(g);
        DrawObjects(g);
        DrawBlobs(g);

        DrawInfoPanel(g, 0, 0, width, height);

        DrawCursorPath(g);
        DrawObjects(g);
        DrawBlobs(g);
    }

    private void DrawCursorPath(Graphics g)
    {
        if (cursorList.Count == 0) return;

        lock (cursorList)
        {
            foreach (TuioCursor tcur in cursorList.Values)
            {
                List<TuioPoint> path = tcur.Path;
                if (path.Count == 0) continue;

                TuioPoint currentPoint = path[0];

                for (int i = 0; i < path.Count; i++)
                {
                    TuioPoint nextPoint = path[i];
                    g.DrawLine(
                        curPen,
                        currentPoint.getScreenX(width / 2),
                        currentPoint.getScreenY(height),
                        nextPoint.getScreenX(width / 2),
                        nextPoint.getScreenY(height)
                    );
                    currentPoint = nextPoint;
                }

                g.FillEllipse(
                    curBrush,
                    currentPoint.getScreenX(width / 2) - height / 100,
                    currentPoint.getScreenY(height) - height / 100,
                    height / 50,
                    height / 50
                );

                g.DrawString(
                    tcur.CursorID + "",
                    font,
                    fntBrush,
                    new PointF(tcur.getScreenX(width / 2) - 10, tcur.getScreenY(height) - 10)
                );
            }
        }
    }

    private void DrawObjects(Graphics g)
    {
        if (objectList.Count == 0) return;

        lock (objectList)
        {
            foreach (TuioObject tobj in objectList.Values)
            {
                int drawWidth = width / 2;
                int ox = tobj.getScreenX(drawWidth);
                int oy = tobj.getScreenY(height);
                int size = height / 10;

                Brush markerBrush = Brushes.Gray;
                string artifactName = "Unknown";

                if (tobj.SymbolID == 1)
                {
                    markerBrush = Brushes.Goldenrod;
                    artifactName = "Statue";
                }
                else if (tobj.SymbolID == 2)
                {
                    markerBrush = Brushes.DarkGreen;
                    artifactName = "Weapon";
                }
                else if (tobj.SymbolID == 3)
                {
                    markerBrush = Brushes.DarkBlue;
                    artifactName = "Audio";
                }
                else if (tobj.SymbolID == 4)
                {
                    markerBrush = Brushes.Purple;
                    artifactName = "Map";
                }

                g.TranslateTransform(ox, oy);
                g.RotateTransform((float)(tobj.Angle / Math.PI * 180.0f));
                g.TranslateTransform(-ox, -oy);

                g.DrawLine(Pens.White, ox, oy, ox, oy - size / 2);

                g.TranslateTransform(ox, oy);
                g.RotateTransform(-1 * (float)(tobj.Angle / Math.PI * 180.0f));
                g.TranslateTransform(-ox, -oy);

                float angleDeg = (float)(tobj.Angle * 180.0 / Math.PI);
                if (angleDeg < 0) angleDeg += 360f;

                g.DrawString("ID: " + tobj.SymbolID, font, fntBrush, new PointF(ox - 20, oy - 30));
                g.DrawString(artifactName, font, fntBrush, new PointF(ox - 25, oy + 20));
                g.DrawString(((int)angleDeg) + "°", font, fntBrush, new PointF(ox - 15, oy + 35));
            }
        }
    }

    private void DrawBlobs(Graphics g)
    {
        if (blobList.Count == 0) return;

        lock (blobList)
        {
            foreach (TuioBlob tblb in blobList.Values)
            {
                int bx = tblb.getScreenX(width / 2);
                int by = tblb.getScreenY(height);
                float bw = tblb.Width * (width / 2);
                float bh = tblb.Height * height;

                g.TranslateTransform(bx, by);
                g.RotateTransform((float)(tblb.Angle / Math.PI * 180.0f));
                g.TranslateTransform(-bx, -by);

                g.FillEllipse(blbBrush, bx - bw / 2, by - bh / 2, bw, bh);

                g.TranslateTransform(bx, by);
                g.RotateTransform(-1 * (float)(tblb.Angle / Math.PI * 180.0f));
                g.TranslateTransform(-bx, -by);

                g.DrawString(tblb.BlobID + "", font, fntBrush, new PointF(bx, by));
            }
        }
    }

    private void DrawInfoPanel(Graphics g, int x, int y, int w, int h)
    {
        g.FillRectangle(Brushes.Black, new Rectangle(x, y, w, h));
        g.DrawRectangle(Pens.White, new Rectangle(x + 10, y + 10, w - 20, h - 20));

        int textX = x + 25;
        int currentY = y + 25;

        g.DrawString("Smart Museum Guide", titleFont, Brushes.White, new PointF(textX, currentY));
        currentY += 40;

        if (currentMarkerData == null)
        {
            g.DrawString("Place a marker to show artifact data.", infoFont, Brushes.White, new PointF(textX, currentY));
            return;
        }

        g.DrawString("Marker ID: " + currentMarkerData.marker_id, infoFont, Brushes.White, new PointF(textX, currentY));
        currentY += 25;

        g.DrawString("Rotation: " + ((int)currentMarkerData.rotation) + "°", infoFont, Brushes.White, new PointF(textX, currentY));
        currentY += 25;

        g.DrawString("Artifact: " + currentMarkerData.artifact, infoFont, Brushes.White, new PointF(textX, currentY));
        currentY += 35;

        g.DrawString(currentTitle, titleFont, Brushes.LightYellow, new PointF(textX, currentY));
        currentY += 35;

        g.DrawString(currentDescription, infoFont, Brushes.White, new RectangleF(textX, currentY, w - 50, 60));
        currentY += 75;

        if (currentMarkerData.marker_id == 1 || currentMarkerData.marker_id == 4)
        {
            g.DrawString("Zoom: " + Zoom.ToString("F2") + "x", infoFont, Brushes.White, new PointF(textX, currentY));
            currentY += 25;
        }

        if (currentMarkerData.marker_id == 3)
        {
            g.DrawString("Volume: " + Volume + "%", infoFont, Brushes.White, new PointF(textX, currentY));
            currentY += 25;

            g.FillRectangle(Brushes.Gray, new Rectangle(textX, currentY, 200, 15));
            g.FillRectangle(Brushes.LimeGreen, new Rectangle(textX, currentY, Volume * 2, 15));
            currentY += 30;
        }

        DrawContentImage(g, textX, currentY, w - 50, h - currentY - 25);
    }

    private void DrawContentImage(Graphics g, int x, int y, int w, int h)
    {
        if (string.IsNullOrEmpty(currentImagePath))
            return;

        string fullPath = Path.Combine(Environment.CurrentDirectory, currentImagePath);

        if (!File.Exists(fullPath))
        {
            g.DrawString("Image not found: " + currentImagePath, infoFont, Brushes.OrangeRed, new PointF(x, y));
            return;
        }

        try
        {
            using (Image img = Image.FromFile(fullPath))
            {
                float scaledW = w;
                float scaledH = h;

                if (currentMarkerData != null && (currentMarkerData.marker_id == 1 || currentMarkerData.marker_id == 4))
                {
                    scaledW = w * Zoom;
                    scaledH = h * Zoom;
                }

                RectangleF dest = new RectangleF(
                    x + (w - scaledW) / 2,
                    y + (h - scaledH) / 2,
                    scaledW,
                    scaledH
                );

                g.SetClip(new Rectangle(x, y, w, h));
                g.DrawImage(img, dest);
                g.ResetClip();

                g.DrawRectangle(Pens.White, x, y, w, h);
            }
        }
        catch
        {
            g.DrawString("Could not load image: " + currentImagePath, infoFont, Brushes.OrangeRed, new PointF(x, y));
        }
    }

    public static void Main(string[] argv)
    {
        int port = 0;

        switch (argv.Length)
        {
            case 1:
                port = int.Parse(argv[0], null);
                if (port == 0) goto default;
                break;

            case 0:
                port = 3333;
                break;

            default:
                Console.WriteLine("usage: TuioDemo [port]");
                Environment.Exit(0);
                break;
        }

        TuioDemo app = new TuioDemo(port);
        Application.Run(app);
    }
}