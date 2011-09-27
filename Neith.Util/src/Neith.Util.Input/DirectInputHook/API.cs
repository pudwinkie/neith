using System;
using System.IO.Pipes;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Neith.Util;

namespace Neith.Util.Input.DirectInputHook
{
    public class API : DisposableObject
    {
        /// <summary>コマンド送信用ネームドパイプ名</summary>
        private const string PipeName = @"\\.\pipe\DIHOOK-A4A191C7-FA9A-451f-B8B3-2080A9C1D4A7";

        /// <summary>レジストリ名</summary
        private const string RegKeyName = @"Software\DirectInput-Hook";

        private static API loader = null;

        [DllImport("DIHOOK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        extern private static void InstallHook();

        [DllImport("DIHOOK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        extern private static void RemoveHook();

        /// <summary>
        /// APIローダ
        /// </summary>
        /// <param name="processName">Hookを設定するプロセス名</param>
        public static void Load(string processName)
        {
            if (loader != null) loader.Dispose();
            loader = new API(processName);
        }

        /// <summary>
        /// 送信内容を登録します。
        /// </summary>
        /// <param name="key"></param>
        public static void Add(KeyCode key)
        {
            if (loader != null) return;
            if (loader.Data.Count > 0) loader.Data.Add(0x20);
            loader.Data.Add((byte)key);
        }

        /// <summary>
        /// 送信内容を登録します。
        /// </summary>
        /// <param name="keys"></param>
        public static void Add(params KeyCode[] keys)
        {
            if (loader != null) return;
            foreach (KeyCode key in keys) Add(key);
        }

        /// <summary>
        /// 送信を行います。
        /// </summary>
        public static void Send()
        {
            if (loader != null) return;
            byte[] data = loader.Data.ToArray();
            loader.Data.Clear();
            loader.Pipe.Write(data, 0, data.Length);
            loader.Pipe.WaitForPipeDrain();
        }

        /// <summary>
        /// パラメータの内容を送信します。
        /// </summary>
        /// <param name="key"></param>
        public static void Send(KeyCode key)
        {
            Add(key);
            Send();
        }

        /// <summary>
        /// パラメータの内容を送信します。
        /// </summary>
        /// <param name="keys"></param>
        public static void Send(params KeyCode[] keys)
        {
            Add(keys);
            Send();
        }



        private readonly NamedPipeClientStream Pipe;
        private readonly List<byte> Data = new List<byte>();


        /// <summary>
        /// コンストラクタ。DLLローダ
        /// </summary>
        private API(string processName)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(RegKeyName);
            try {
                key.SetValue(null, processName);
                key.Close();
                key = null;
                InstallHook();
            }
            finally {
                Registry.CurrentUser.DeleteSubKeyTree(RegKeyName);
            }
            Pipe = new NamedPipeClientStream(PipeName);
        }

        /// <summary>
        /// アンマネージリソースの開放
        /// </summary>
        protected override void DisposeUnManage()
        {
            base.DisposeUnManage();
            if (Pipe != null) Pipe.Close();
            RemoveHook();
        }


    }
}
