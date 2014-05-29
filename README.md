Sws.Streams
===========

VS2010 solution for code supporting general streaming tasks.

Work in progress.

Sws.Streams.Core.Adapters:

Some of the features described below are not currently implemented as flat Stream subclasses in order to provide maximum flexibility.  In a couple of cases, I've added adapters which aim to simplify access to these.  Entry point is AdapterFactory.

Sws.Streams.Core.Common:

General common code.

Sws.Streams.Core.Forwarding:

Allows a sequentially readable source Stream to be continually read and forwarded on to a sequentially writable target Stream.  Entry point is StreamForwarderBuilder.

Sws.Streams.Core.FrameDropping:

Allows a sequentially readable source Stream to be wrapped in a Stream which drops data if the client code reads too slowly.  (Requires functionality from Sws.Streams.Core.Rewinding.)  Entry point is FrameDroppingStreamBuilder.

Sws.Streams.Core.Recording:

Wraps a sequentially writable target Stream and appends additional metadata, recording the time offset between the construction of the recording Stream and the invocation of the Write method.  Can later be played back through a playback Stream, which only exposes data for reading after the time offset has passed.  (Useful for recording and later re-creating e.g. data transmitted over a network.)  Entry points are RecordingStreamBuilder and PlaybackStreamBuilder.

Sws.Streams.Core.Rewinding:

Wraps a sequentially readable source Stream so that it can be "rewound".  Rewinding by a certain number of bytes allows those bytes to be re-read, even if the source Stream does not support seeking.  Data may be cached in an arbitrary location, although a default in-memory implementation is supplied.  Entry point is RewindableBuilder.

Sws.Streams.Core.Rolling:

Exposes a sequentially writable Stream to which data is written and a sequentially readable Stream from which data is later read.  In the meantime, data is stored in an arbitrary location (a default in-memory implementation is supplied).  Entry point is RollingMemoryBuilder.

Sws.Streams.Core.Splitting:

Wraps up multiple writable Streams into a single one, allowing client code to write to multiple Streams simultaneously.  Entry point is SplitStreamBuilder.

Example usage:

Pulling a live data stream from a NetworkStream, where you don't want to hold up whatever is on the other side of the NetworkStream, and simply want to drop old data if your own code is too slow (e.g. for displaying a live image feed).

In this case, you can use forwarding to forward the Stream to rolling memory (so that you can read it at leisure without slowing the NetworkStream down), then wrap the rolling memory's read Stream in a frame dropping Stream (via a Rewindable).