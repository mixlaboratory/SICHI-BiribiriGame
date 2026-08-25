using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

// ============================================================================
//  ExBrain UDP 受信スクリプト (Unity 側 / 方法A)
//
//  使い方:
//   1. このファイルを Unity プロジェクトの Assets フォルダに入れる
//   2. 空の GameObject を作り、このスクリプトをアタッチする
//   3. 送信側(UnityBridge)を dotnet run で起動して計測を開始する
//   4. 最新値は Latest... プロパティから、または OnFrame イベントで受け取れる
//
//  送信フォーマット: elapsedSec,brainLeft,dTHbL3cm,pulse,valid
// ============================================================================

public class ExBrainReceiver : MonoBehaviour
{
    [Header("送信側(UnityBridge)と合わせるポート")]
    public int port = 5005;

    [Header("最新値(インスペクタで確認用)")]
    public float latestElapsedSec;
    public float latestBrainLeft;
    public float latestDThbL3cm;   // 前頭前野の血流変化(主役)
    public float latestPulse;      // 心拍数(bpm)
    public bool latestValid;       // false のフレームはノイズ

    // 有効(valid)なフレームだけを保持した最新値。演出にはこちらを使うと安定します。
    public float lastValidDThbL3cm;
    public float lastValidPulse;

    // 1フレーム受信ごとに呼ばれるイベント(任意で購読)
    public event System.Action<ExBrainFrame> OnFrame;

    private UdpClient _udp;
    private Thread _thread;
    private volatile bool _running;

    // 受信スレッド -> メインスレッドへの受け渡し用
    private readonly object _lock = new object();
    private ExBrainFrame? _pending;

    void Start()
    {
        _udp = new UdpClient(port);
        _running = true;
        _thread = new Thread(ReceiveLoop) { IsBackground = true };
        _thread.Start();
        Debug.Log($"[ExBrain] UDP 受信開始 port={port}");
    }

    private void ReceiveLoop()
    {
        var remote = new IPEndPoint(IPAddress.Any, 0);
        while (_running)
        {
            try
            {
                byte[] data = _udp.Receive(ref remote);
                string text = System.Text.Encoding.ASCII.GetString(data);
                if (TryParse(text, out ExBrainFrame frame))
                {
                    lock (_lock) { _pending = frame; }
                }
            }
            catch (SocketException) { /* 停止時など */ }
        }
    }

    private static bool TryParse(string text, out ExBrainFrame frame)
    {
        frame = default;
        string[] p = text.Split(',');
        if (p.Length < 5) return false;
        var ci = CultureInfo.InvariantCulture;
        if (!float.TryParse(p[0], NumberStyles.Float, ci, out float t)) return false;
        if (!float.TryParse(p[1], NumberStyles.Float, ci, out float b)) return false;
        if (!float.TryParse(p[2], NumberStyles.Float, ci, out float d)) return false;
        if (!float.TryParse(p[3], NumberStyles.Float, ci, out float pr)) return false;
        frame = new ExBrainFrame
        {
            elapsedSec = t,
            brainLeft = b,
            dThbL3cm = d,
            pulse = pr,
            valid = p[4].Trim() == "1"
        };
        return true;
    }

    void Update()
    {
        ExBrainFrame? frame = null;
        lock (_lock)
        {
            if (_pending.HasValue) { frame = _pending; _pending = null; }
        }
        if (frame.HasValue)
        {
            var f = frame.Value;
            latestElapsedSec = f.elapsedSec;
            latestBrainLeft = f.brainLeft;
            latestDThbL3cm = f.dThbL3cm;
            latestPulse = f.pulse;
            latestValid = f.valid;
            if (f.valid)
            {
                lastValidDThbL3cm = f.dThbL3cm;
                lastValidPulse = f.pulse;
            }
            OnFrame?.Invoke(f);
        }
    }

    void OnDestroy()
    {
        _running = false;
        try { _udp?.Close(); } catch { }
        try { _thread?.Join(200); } catch { }
    }
}

public struct ExBrainFrame
{
    public float elapsedSec;
    public float brainLeft;
    public float dThbL3cm;
    public float pulse;
    public bool valid;
}
