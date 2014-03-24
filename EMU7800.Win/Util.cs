using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Threading;
using EMU7800.Core;

namespace EMU7800.Win
{
    internal class Util
    {
        public static bool IsProcess32Bit
        {
            [DebuggerStepThrough]
            get { return IntPtr.Size == 4; }
        }

        public static bool IsCriticalException(Exception ex)
        {
            return ex is OutOfMemoryException
                || ex is StackOverflowException
                || ex is SecurityException
                || ex is ThreadAbortException;
        }

        public static void SerializeMachineToFile(MachineBase m, string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            using (var bw = new BinaryWriter(fs))
            {
                m.Serialize(bw);
                bw.Flush();
                bw.Close();
            }
        }

        public static MachineBase DeserializeMachineFromFile(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open))
            using (var br = new BinaryReader(fs))
            {
                return MachineBase.Deserialize(br);
            }
        }
    }
}
