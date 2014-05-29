using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using Sws.Streams.Supplemental.Common.Internal;

namespace Sws.Streams.Supplemental.StreamImplementations
{
    public class NonBlockingReadNetworkStream : NonBlockingReadStream
    {

        public NonBlockingReadNetworkStream(TcpClient tcpClient, int clientPollWaitTimeoutMicroseconds)
            : this(tcpClient, tcpClient.GetStream(), clientPollWaitTimeoutMicroseconds)
        {
        }

        private NonBlockingReadNetworkStream(TcpClient tcpClient, NetworkStream networkStream, int clientPollWaitTimeoutMicroseconds)
            : base(networkStream, () => ShouldTryRead(tcpClient, networkStream, clientPollWaitTimeoutMicroseconds))
        {
        }

        private static bool ShouldTryRead(TcpClient tcpClient, NetworkStream networkStream, int clientPollWaitTimeoutMicroseconds)
        {
            bool output = networkStream.DataAvailable;

            if (!output)
            {
                output = tcpClient.Client.Poll(clientPollWaitTimeoutMicroseconds, SelectMode.SelectRead);
                if (output && (!networkStream.DataAvailable))
                {
                    throw new IOException(ExceptionMessages.SocketClosed);
                }
            }

            return output;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                this.SourceStream.Dispose();
            }

        }

    }
}
