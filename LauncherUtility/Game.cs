using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LauncherUtility
{
    public class Game
    {
        public string ResourcePath { get; set; } = "";
        public string LoginData { get; set; } = "";
        public string ExePath { get; set; } = "";
        private MemoryMappedFile? file;
        private NamedPipeServerStream? pipeServer;
        private const ulong GameID = 29080;

        public Process? Launch()
        {
            if (string.IsNullOrEmpty(ResourcePath) || string.IsNullOrEmpty(LoginData) || string.IsNullOrEmpty(ExePath))
                throw new ArgumentNullException();

            List<byte> dataBytes = [];
            dataBytes.Add(0); // Unused for now
            dataBytes.AddRange(Encoding.UTF8.GetBytes(ResourcePath));
            dataBytes.Add(0);
            dataBytes.AddRange(Encoding.UTF8.GetBytes(LoginData));
            dataBytes.Add(0);

            pipeServer = new NamedPipeServerStream("goodpipe", PipeDirection.InOut, 1);

            new Thread(delegate ()
            {
                pipeServer.WaitForConnection();
                Console.WriteLine("pipe 1 got client");

                var b = dataBytes.ToArray();

                for (int i = 0; i < b.Length; i++)
                {
                    pipeServer.WriteByte(b[i]);
                }
                pipeServer.WriteByte(0);
                pipeServer.Close();
                pipeServer = null;
            }).Start();

            file = MemoryMappedFile.CreateNew($"Sail.SharedMemory.{GameID}", 0x1000);

            // inform the game about DLL path and its game ID

            var dllPath = AppDomain.CurrentDomain.BaseDirectory;
            var dllPathRaw = Encoding.Unicode.GetBytes(dllPath);

            var vs = file.CreateViewAccessor();
            vs.Write(0, GameID);
            for (int i = 0; i < dllPathRaw.Length; i++)
            {
                vs.Write(i + 8, dllPathRaw[i]);
            }
            vs.Write(8 + dllPathRaw.Length, 0); // null terminator

            return Process.Start(new ProcessStartInfo()
            {
                FileName = ExePath,
                UseShellExecute = false,
            });
        }

        public void Cleanup()
        {
            file?.Dispose();
            if (pipeServer != null)
            {
                pipeServer.Close();
                pipeServer.Dispose();
                pipeServer = null;
            }
        }
    }
}
