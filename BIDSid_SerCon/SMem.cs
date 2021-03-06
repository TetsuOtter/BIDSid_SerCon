﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TR.BIDSid_SerCon
{
  class SMemLib : IDisposable
  {
    private static readonly string SRAMName = "BIDSSharedMem";
    //SECTION_ALL_ACCESS=983071
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr CreateFileMapping(UIntPtr hFile, IntPtr lpAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);
    //[DllImport("kernel32.dll")]
    //static extern void CopyMemory(IntPtr Destination, IntPtr Source, uint Length);//予約
    [DllImport("kernel32.dll")]
    static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);
    [DllImport("kernel32.dll")]
    static extern bool CloseHandle(IntPtr hObject);


    /// <summary>車両のスペック(互換性確保)</summary>
    public struct Spec
    {
      /// <summary>ブレーキ段数</summary>
      public int B;
      /// <summary>ノッチ段数</summary>
      public int P;
      /// <summary>ATS確認段数</summary>
      public int A;
      /// <summary>常用最大段数</summary>
      public int J;
      /// <summary>編成車両数</summary>
      public int C;
    };
    /// <summary>車両の状態(互換性確保)</summary>
    public struct State
    {
      /// <summary>列車位置[m]</summary>
      public double Z;
      /// <summary>列車速度[km/h]</summary>
      public float V;
      /// <summary>0時からの経過時間[ms]</summary>
      public int T;
      /// <summary>BC圧力[kPa]</summary>
      public float BC;
      /// <summary>MR圧力[kPa]</summary>
      public float MR;
      /// <summary>ER圧力[kPa]</summary>
      public float ER;
      /// <summary>BP圧力[kPa]</summary>
      public float BP;
      /// <summary>SAP圧力[kPa]</summary>
      public float SAP;
      /// <summary>電流[A]</summary>
      public float I;
    };
    /// <summary>車両のハンドル位置(互換性確保)</summary>
    public struct Hand
    {
      /// <summary>ブレーキハンドル位置</summary>
      public int B;
      /// <summary>ノッチハンドル位置</summary>
      public int P;
      /// <summary>レバーサーハンドル位置</summary>
      public int R;
      /// <summary>定速制御状態</summary>
      public int C;
    };
    /// <summary>BIDSSharedMemoryのデータ構造体(互換性確保)</summary>
    public struct BIDSSharedMemoryData
    {
      /// <summary>SharedMemoryが有効かどうか</summary>
      public bool IsEnabled;
      /// <summary>SharedRAMの構造バージョン</summary>
      public int VersionNum;
      /// <summary>車両スペック情報</summary>
      public Spec SpecData;
      /// <summary>車両状態情報</summary>
      public State StateData;
      /// <summary>ハンドル位置情報</summary>
      public Hand HandleData;
      /// <summary>ドアが閉まっているかどうか</summary>
      public bool IsDoorClosed;
      /// <summary>Panelの表示番号配列</summary>
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      public int[] Panel;
      /// <summary>Soundの値配列</summary>
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      public int[] Sound;
    };

    static private readonly uint BSMDsize = (uint)Marshal.SizeOf(typeof(BIDSSharedMemoryData));

    /// <summary>毎フレームごとに取得できるデータ(本家/open共通)</summary>
    public struct ElapD
    {
      /// <summary>情報が有効かどうか</summary>
      public bool IsEnabled { get; set; }
      /// <summary>クラスバージョン</summary>
      public readonly int Ver;
      /// <summary>列車位置[m]</summary>
      public double Location { get; set; }
      /// <summary>列車速度[km/h]</summary>
      public double Speed { get; set; }
      /// <summary>BC圧力[kPa]</summary>
      public double BCPres { get; set; }
      /// <summary>MR圧力[kPa]</summary>
      public double MRPres { get; set; }
      /// <summary>ER圧力[kPa]</summary>
      public double ERPres { get; set; }
      /// <summary>BP圧力[kPa]</summary>
      public double BPPres { get; set; }
      /// <summary>SAP圧力[kPa]</summary>
      public double SAPPres { get; set; }
      /// <summary>電流[A]</summary>
      public double Current { get; set; }
      /// <summary>(未実装)架線電圧[V]</summary>
      public double Voltage { get; set; }
      /// <summary>0時からの経過時間[ms]</summary>
      public int Time { get; set; }
      /// <summary>現在時刻[時]</summary>
      public byte TimeHH { get { return (byte)TimeSpan.FromMilliseconds(Time).Hours; } }
      /// <summary>現在時刻[分]</summary>
      public byte TimeMM { get { return (byte)TimeSpan.FromMilliseconds(Time).Minutes; } }
      /// <summary>現在時刻[秒]</summary>
      public byte TimeSS { get { return (byte)TimeSpan.FromMilliseconds(Time).Seconds; } }
      /// <summary>現在時刻[ミリ秒]</summary>
      public short TimeMS { get { return (short)TimeSpan.FromMilliseconds(Time).Milliseconds; } }
    }

    /// <summary>毎フレームごとに取得できるデータ(open専用)</summary>
    public struct OElapD
    {
      /// <summary>情報が有効かどうか</summary>
      public bool IsEnabled { get; set; }
      /// <summary>クラスバージョン</summary>
      public readonly int Ver;
      /// <summary>現在のカーブ半径[m]</summary>
      public double Radius { get; set; }
      /// <summary>現在のカントの大きさ[mm]</summary>
      public double Cant { get; set; }
      /// <summary>現在の軌間[mm]</summary>
      public double Pitch { get; set; }
      /// <summary>1フレーム当たりの時間[ms]</summary>
      public ushort ElapTime { get; set; }
      /// <summary>先行列車に関する情報</summary>
      public PreTrainD PreTrain { get; set; }

      public struct PreTrainD
      {
        /// <summary>情報が有効かどうか</summary>
        public bool IsEnabled { get; set; }
        /// <summary>先行列車の尻尾の位置[m]</summary>
        public double Location { get; set; }
        /// <summary>先行列車の尻尾までの距離[m]</summary>
        public double Distance { get; set; }
        /// <summary>先行列車の速度[km/h]</summary>
        public double Speed { get; set; }
      }
    }

    /// <summary>ハンドルに関する情報</summary>
    public struct HandleD
    {
      /// <summary>クラスバージョン</summary>
      public readonly int Ver;
      /// <summary>情報が有効かどうか</summary>
      public bool IsEnabled { get; set; }
      /// <summary>レバーサー位置</summary>
      public sbyte Reverser { get; set; }
      /// <summary>Pノッチ位置</summary>
      public short PNotch { get; set; }
      /// <summary>Bノッチ位置</summary>
      public ushort BNotch { get; set; }
      /// <summary>単弁位置</summary>
      public ushort LocoBNotch { get; set; }
      /// <summary>定速制御の有効状態</summary>
      public bool ConstSP { get; set; }
    }

    /// <summary>駅に関するデータ</summary>
    public struct StaD
    {
      /// <summary>データサイズ</summary>
      public int Size => Marshal.SizeOf(StaList);
      public List<StationData> StaList { get; set; }
      /// <summary>駅に関するデータ</summary>
      public struct StationData
      {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        /// <summary>駅名(64Byteまで)</summary>
        public char[] Name;
        /// <summary>到着予定時刻[ms]</summary>
        public int ArrTime;
        /// <summary>発車予定時刻[ms]</summary>
        public int DepTime;
        /// <summary>停車時間[ms]</summary>
        public int StopTime;
        /// <summary>停止定位駅かどうか</summary>
        public bool IsSigUsuallyRed;
        /// <summary>左のドアが開くかどうか</summary>
        public bool IsLeftOpen;
        /// <summary>右のドアが開くかどうか</summary>
        public bool IsRightOpen;
        /// <summary>予定された発着番線</summary>
        public float DefaultTrack;
        /// <summary>停止位置[m]</summary>
        public double StopLocation;
        /// <summary>通過駅かどうか</summary>
        public bool IsPass;
        /// <summary>駅の種類</summary>
        public StaType Type;

        /// <summary>駅の種類</summary>
        public enum StaType { Normal, ChangeEnd, Termial, Stop }
      }
    }

    /// <summary>操作情報</summary>
    public struct ControlD
    {
      /// <summary>クラスバージョン</summary>
      public readonly int Ver;
      /// <summary>情報が有効かどうか</summary>
      public bool IsEnabled { get; set; }
      /// <summary>レバーサー位置</summary>
      public sbyte Reverser { get; set; }
      /// <summary>Pノッチ位置</summary>
      public short PNotch { get; set; }
      /// <summary>Bノッチ位置</summary>
      public ushort BNotch { get; set; }
      /*
      /// <summary>キー押下状態</summary>
      public IDictionary<KeyCode,bool> Keys { get; set; }
      /// <summary>キー名称一覧</summary>
      public enum KeyCode {
        S = 78, A1, A2, B1, B2, C1, C2, D, E, F, G, H, I, J, K, L, M, N, O, P,
        WiperSpeedUp, WiperSpeedDown, FillFuel, Headlights, LiveSteamInjector, ExhaustSteamInjector, IncreaseCutoff, DecreaseCutoff, Blowers,
        EngineStart, EngineStop, GearUp, GearDown, RaisePantograph, LowerPantograph, MainBreaker=113,
        LeftDoors = 21, RightDoors, PrimaryHorn, SecondaryHorn, MusicHorn,
        LocoBIncrease = 27, LocoBDecrease
      }*/
    }

    /// <summary>ドア状態情報</summary>
    public enum DoorInfo { Open, Close, RightOpen, LeftOpen }

    /// <summary> BIDSSMemDataが更新された際に呼ばれるイベント </summary>
    public event EventHandler BIDSSMemChanged;
    /// <summary>BIDSSharedMemoryのデータ(互換性確保)</summary>
    public BIDSSharedMemoryData BIDSSMemData { get { return __BIDSSMemData; } private set { __BIDSSMemData = value; BIDSSMemChanged?.Invoke(value, new EventArgs()); } }
    private BIDSSharedMemoryData __BIDSSMemData = new BIDSSharedMemoryData();

    /// <summary> ElapDが更新された際に呼ばれるイベント </summary>
    public event EventHandler ElapDChanged;
    /// <summary>毎フレームごとのデータ(本家/open共通)</summary>
    public ElapD ElapData { get { return __ElapD; } private set { __ElapD = value; ElapDChanged?.Invoke(value, new EventArgs()); } }
    private ElapD __ElapD = new ElapD();

    /// <summary> OElapDが更新された際に呼ばれるイベント </summary>
    public event EventHandler OElapDChanged;
    /// <summary>毎フレームごとのデータ(open専用)</summary>
    public OElapD OElapData { get { return __OElapD; } private set { __OElapD = value; OElapDChanged?.Invoke(value, new EventArgs()); } }
    private OElapD __OElapD = new OElapD();

    /// <summary> HandleDが更新された際に呼ばれるイベント </summary>
    public event EventHandler HandleDChanged;
    /// <summary>ハンドルに関する情報</summary>
    public HandleD HandleInfo { get { return __HandleD; } private set { __HandleD = value; HandleDChanged?.Invoke(value, new EventArgs()); } }
    private HandleD __HandleD = new HandleD();

    /// <summary> DoorInfoが更新された際に呼ばれるイベント </summary>
    public event EventHandler DoorInfoChanged;
    /// <summary>ドア状態情報</summary>
    public DoorInfo Door { get { return __DoorInfo; } private set { __DoorInfo = value; DoorInfoChanged?.Invoke(value, new EventArgs()); } }
    private DoorInfo __DoorInfo = new DoorInfo();

    /// <summary> ControlDが更新された際に呼ばれるイベント </summary>
    public event EventHandler ControlDChanged;
    /// <summary>各種操作情報</summary>
    public ControlD ControlData { get { return __ControlD; } set { __ControlD = value; ControlDChanged?.Invoke(value, new EventArgs()); } }
    private ControlD __ControlD = new ControlD();

    /// <summary> StaDが更新された際に呼ばれるイベント </summary>
    public event EventHandler StaDChanged;
    /// <summary>ドア状態情報</summary>
    public StaD Stations { get { return __StaD; } private set { __StaD = value; StaDChanged?.Invoke(value, new EventArgs()); } }
    private StaD __StaD = new StaD();


    static private readonly uint size = (uint)Marshal.SizeOf(typeof(BIDSSharedMemoryData));
    static readonly IntPtr hSharedMemory = CreateFileMapping(UIntPtr.Zero, IntPtr.Zero, 4, 0, size, SRAMName);
    static readonly IntPtr pMemory = MapViewOfFile(hSharedMemory, 983071, 0, 0, size);

    public SMemLib()
    {

    }

    public void Dispose()
    {

    }

  }
}
