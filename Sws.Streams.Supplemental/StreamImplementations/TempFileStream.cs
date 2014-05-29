using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Supplemental.StreamImplementations
{

    public class TempFileStream : FileStream
    {

        private bool DeleteOnClose { get; set; }

        private readonly object _closeSyncObject = new object();

        private object CloseSyncObject { get { return _closeSyncObject; } }

        private static string GetTempFileName()
        {
            return Path.GetTempFileName();
        }

        public TempFileStream(FileMode mode, FileAccess access)
            : this(GetTempFileName(), mode, access)
        {
        }

        private TempFileStream(string path, FileMode mode, FileAccess access)
            : base(path, mode, access)
        {
            this.DeleteOnClose = true;
        }

        public TempFileStream CloseAndReopen(FileMode mode, FileAccess access)
        {
            lock (CloseSyncObject)
            {
                string path = this.Name;

                this.DeleteOnClose = false;

                this.Close(false);

                return new TempFileStream(path, mode, access);
            }
        }

        public override void Close()
        {
            Close(true);
        }

        private void Close(bool lockRequired)
        {
            if (lockRequired)
            {
                lock (CloseSyncObject)
                {
                    Close(false);
                }
            }
            else
            {
                string path = this.Name;

                base.Close();

                if (this.DeleteOnClose)
                {
                    File.Delete(path);
                }
            }
        }

    }

}
