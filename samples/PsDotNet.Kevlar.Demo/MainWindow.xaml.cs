using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PsDotNet.Kevlar;
using PsDotNet.Kevlar.Scripting;

namespace PsDotNet.Kevlar.Demo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    // -------------------------------------------------------------------------
    // Fields
    // -------------------------------------------------------------------------

    private PhotoshopConnection? _conn;

    private SolidColorBrush _foregroundColor = new(Colors.Red);
    private SolidColorBrush _backgroundColor = new(Colors.Blue);
    private string _currentTool = "Undefined";
    private BitmapSource? _thumbnailSource;
    private string _statusText = string.Empty;
    private bool _isConnected;

    // -------------------------------------------------------------------------
    // Bindable properties
    // -------------------------------------------------------------------------

    public SolidColorBrush ForegroundColor
    {
        get => _foregroundColor;
        set { _foregroundColor = value; OnPropertyChanged(); }
    }

    public SolidColorBrush BackgroundColor
    {
        get => _backgroundColor;
        set { _backgroundColor = value; OnPropertyChanged(); }
    }

    public string CurrentTool
    {
        get => _currentTool;
        set { _currentTool = value; OnPropertyChanged(); }
    }

    /// <summary>Thumbnail of the active Photoshop document (null when not yet fetched).</summary>
    public BitmapSource? ThumbnailSource
    {
        get => _thumbnailSource;
        set { _thumbnailSource = value; OnPropertyChanged(); }
    }

    public string StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(); }
    }

    public bool IsConnected
    {
        get => _isConnected;
        set { _isConnected = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanConnect)); }
    }

    public bool CanConnect => !_isConnected;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    // -------------------------------------------------------------------------
    // Event handlers — all async void (acceptable for WPF UI events)
    // -------------------------------------------------------------------------

    private async void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        if (_conn is not null)
            return;

        const string host     = "127.0.0.1";
        const int    port     = 49494;
        const string password = "Swordfish";

        AppendStatus($"Connecting to {host}:{port} …");

        try
        {
            _conn = await PhotoshopConnection.ConnectAsync(host, port, password);

            // Wire up the notification event — raised from the background receive
            // loop, so always marshal UI updates back to the dispatcher.
            _conn.NotificationReceived += OnNotificationReceived;

            IsConnected = true;
            AppendStatus("Connected.");

            // Subscribe to the same events as the old demo.
            await _conn.SubscribeEventsAsync(new[]
            {
                PhotoshopNotification.imageChanged,
                PhotoshopNotification.foregroundColorChanged,
                PhotoshopNotification.backgroundColorChanged,
                PhotoshopNotification.toolChanged,
            });

            AppendStatus("Event subscriptions registered.");

            // Fetch initial foreground / background colors.
            System.Drawing.Color fg = await _conn.GetForegroundColorAsync();
            ForegroundColor.Color = Color.FromRgb(fg.R, fg.G, fg.B);

            System.Drawing.Color bg = await _conn.GetBackgroundColorAsync();
            BackgroundColor.Color = Color.FromRgb(bg.R, bg.G, bg.B);

            AppendStatus("Initial colors fetched.");
        }
        catch (Exception ex)
        {
            AppendStatus($"Connect failed: {ex.Message}");
            _conn?.Dispose();
            _conn = null;
            IsConnected = false;
        }
    }

    private async void DisconnectButton_Click(object sender, RoutedEventArgs e)
    {
        if (_conn is null)
            return;

        AppendStatus("Disconnecting …");

        try
        {
            if (_conn is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            else
                _conn.Dispose();
        }
        catch (Exception ex)
        {
            AppendStatus($"Disconnect error: {ex.Message}");
        }
        finally
        {
            _conn = null;
            IsConnected = false;
            AppendStatus("Disconnected.");
        }
    }

    private async void GetThumbnailButton_Click(object sender, RoutedEventArgs e)
    {
        if (_conn is null)
            return;

        AppendStatus("Fetching thumbnail …");

        try
        {
            System.Drawing.Bitmap bitmap = await _conn.GetThumbnailAsync();

            // Convert System.Drawing.Bitmap → WPF BitmapSource.
            ThumbnailSource = BitmapToBitmapSource(bitmap);
            bitmap.Dispose();

            AppendStatus("Thumbnail received.");
        }
        catch (Exception ex)
        {
            AppendStatus($"GetThumbnail failed: {ex.Message}");
        }
    }

    // -------------------------------------------------------------------------
    // NotificationReceived handler
    // Raised from the background receive loop — marshal UI updates to dispatcher.
    // -------------------------------------------------------------------------

    private void OnNotificationReceived(object? sender, PhotoshopNotificationEventArgs e)
    {
        Trace.WriteLine($"{e.Notification} — {e.Payload}");

        switch (e.Notification)
        {
            case PhotoshopNotification.foregroundColorChanged:
            {
                // Payload for color events is a 6-char hex string.
                if (TryParseHexColor(e.Payload, out Color c))
                    Dispatcher.Invoke(() => ForegroundColor.Color = c);
                break;
            }

            case PhotoshopNotification.backgroundColorChanged:
            {
                if (TryParseHexColor(e.Payload, out Color c))
                    Dispatcher.Invoke(() => BackgroundColor.Color = c);
                break;
            }

            case PhotoshopNotification.toolChanged:
            {
                string toolName = PhotoshopTools.GetToolName(e.Payload) ?? e.Payload;
                Dispatcher.Invoke(() => CurrentTool = toolName);
                break;
            }

            case PhotoshopNotification.imageChanged:
                Dispatcher.Invoke(() => AppendStatus("imageChanged notification received."));
                break;
        }
    }

    // -------------------------------------------------------------------------
    // Window closing — dispose the connection cleanly
    // -------------------------------------------------------------------------

    protected override async void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        if (_conn is IAsyncDisposable asyncDisposable)
        {
            try { await asyncDisposable.DisposeAsync(); }
            catch { /* ignore on shutdown */ }
        }
        else
        {
            _conn?.Dispose();
        }
        _conn = null;
    }

    // -------------------------------------------------------------------------
    // INotifyPropertyChanged
    // -------------------------------------------------------------------------

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Appends a timestamped line to the status TextBox.
    /// Must be called on the UI thread (all callers are already dispatched).
    /// </summary>
    private void AppendStatus(string message)
    {
        string line = $"[{DateTime.Now:HH:mm:ss}] {message}";
        StatusText = string.IsNullOrEmpty(StatusText) ? line : StatusText + Environment.NewLine + line;
    }

    /// <summary>
    /// Converts a <see cref="System.Drawing.Bitmap"/> to a WPF <see cref="BitmapSource"/>
    /// using a MemoryStream → PNG round-trip (no P/Invoke required).
    /// </summary>
    private static BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap bitmap)
    {
        using var ms = new MemoryStream();
        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        ms.Seek(0, SeekOrigin.Begin);

        var bi = new BitmapImage();
        bi.BeginInit();
        bi.CacheOption = BitmapCacheOption.OnLoad;
        bi.StreamSource = ms;
        bi.EndInit();
        bi.Freeze(); // safe to share across threads
        return bi;
    }

    /// <summary>
    /// Parses a 6-char hex color payload (e.g. "ff0000") into a WPF <see cref="Color"/>.
    /// Returns <see langword="false"/> when the payload is not exactly 6 valid hex chars.
    /// </summary>
    private static bool TryParseHexColor(string? payload, out Color color)
    {
        color = Colors.Transparent;
        if (payload is null || payload.Length != 6)
            return false;

        if (!int.TryParse(payload[..2], System.Globalization.NumberStyles.HexNumber,
                System.Globalization.CultureInfo.InvariantCulture, out int r)
         || !int.TryParse(payload[2..4], System.Globalization.NumberStyles.HexNumber,
                System.Globalization.CultureInfo.InvariantCulture, out int g)
         || !int.TryParse(payload[4..6], System.Globalization.NumberStyles.HexNumber,
                System.Globalization.CultureInfo.InvariantCulture, out int b))
        {
            return false;
        }

        color = Color.FromRgb((byte)r, (byte)g, (byte)b);
        return true;
    }
}
