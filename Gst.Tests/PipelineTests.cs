using NUnit.Framework;
using System;

namespace Gst.Tests
{
    [TestFixture]
    public class PipelineTests : TestBase
    {
        [Test]
        public void TestPipelineDispose()
        {
            Gst.Pipeline pipeline = new Gst.Pipeline();
            var src = ElementFactory.Make("videotestsrc");
            src["num-buffers"] = 10;

            var vsink = ElementFactory.Make("fakesink");

            pipeline.Add(src, vsink);
            src.Link(vsink);

            var srcWeakRef = new WeakReference(src);
            var vsinkWeakRef = new WeakReference(vsink);
            var pipelineWeakRef = new WeakReference(pipeline);
            var busWeakRef = new WeakReference(pipeline.Bus);

            pipeline.SetState(State.Playing);
            bool terminated = false;
            do
            {
                using (Message msg = pipeline.Bus.PopFiltered(MessageType.StateChanged))
                {
                    if (msg == null || msg.Src != pipeline)
                        continue;

                    msg.ParseStateChanged(out State oldstate, out State newstate, out State pendingstate);

                    if (newstate == State.Playing) terminated = true;
                }
            } while (!terminated);

            pipeline.SetState(State.Null);

            pipeline.Dispose();
            pipeline = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.IsFalse(srcWeakRef.IsAlive);
            Assert.IsFalse(vsinkWeakRef.IsAlive);
            Assert.IsFalse(busWeakRef.IsAlive);
            Assert.IsFalse(pipelineWeakRef.IsAlive);
        }

        [Test]
        public void TestPipelineParseAndDispose()
        {
            var pipeline = Parse.Launch("videotestsrc num-buffers=10 ! fakesink");
            var pipelineWeakRef = new WeakReference(pipeline);
            var busWeakRef = new WeakReference(pipeline.Bus);

            pipeline.SetState(State.Playing);
            bool terminated = false;
            do
            {
                using (Message msg = pipeline.Bus.PopFiltered(MessageType.StateChanged))
                {
                    if (msg == null || msg.Src != pipeline)
                        continue;

                    msg.ParseStateChanged(out State oldstate, out State newstate, out State pendingstate);

                    if (newstate == State.Playing) terminated = true;
                }
            } while (!terminated);

            pipeline.SetState(State.Null);

            pipeline.Dispose();
            pipeline = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.IsFalse(busWeakRef.IsAlive);
            Assert.IsFalse(pipelineWeakRef.IsAlive);
        }

        [Test]
        public void TestPipelineUriDecodeBinForcedlyDespose()
        {
            Pipeline pipeline = (Pipeline)Parse.Launch("uridecodebin name=uridecodebin ! fakesink name=vsink");

            var uridecodebin = pipeline.GetByName("uridecodebin");
            uridecodebin["uri"] = "rtsp://184.72.239.149/vod/mp4:BigBuckBunny_175k.mov";

            var pipelineWeakRef = new WeakReference(pipeline);
            var busWeakRef = new WeakReference(pipeline.Bus);

            pipeline.DeepElementRemoved += DeepElementRemoved;

            pipeline.SetState(State.Playing);
            bool terminated = false;
            do
            {
                using (Message msg = pipeline.Bus.PopFiltered(MessageType.StateChanged))
                {
                    if (msg == null || msg.Src != pipeline)
                        continue;

                    msg.ParseStateChanged(out State oldstate, out State newstate, out State pendingstate);

                    if (newstate == State.Playing) terminated = true;
                }
            } while (!terminated);

            pipeline.SetState(State.Null);

            pipeline.Bus.Dispose();

            pipeline.Dispose();
            pipeline.DeepElementRemoved -= DeepElementRemoved;

            pipeline = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.IsFalse(busWeakRef.IsAlive);
            Assert.IsFalse(pipelineWeakRef.IsAlive);
        }

        [Test]
        public void TestPipelineDisposeManually()
        {
            Gst.Pipeline pipeline = new Gst.Pipeline();
            var src = ElementFactory.Make("videotestsrc");
            src["num-buffers"] = 10;

            var vsink = ElementFactory.Make("fakesink");

            pipeline.Add(src, vsink);
            src.Link(vsink);

            var srcWeakRef = new WeakReference(src);
            var vsinkWeakRef = new WeakReference(vsink);
            var pipelineWeakRef = new WeakReference(pipeline);
            var busWeakRef = new WeakReference(pipeline.Bus);

            pipeline.SetState(State.Playing);
            bool terminated = false;
            do
            {
                using (Message msg = pipeline.Bus.PopFiltered(MessageType.StateChanged))
                {
                    if (msg == null || msg.Src != pipeline)
                        continue;

                    msg.ParseStateChanged(out State oldstate, out State newstate, out State pendingstate);

                    if (newstate == State.Playing) terminated = true;
                }
            } while (!terminated);

            pipeline.SetState(State.Null);

            src.Unlink(vsink);
            pipeline.Remove(src);
            src.Dispose();
            src = null;

            pipeline.Remove(vsink);
            vsink.Dispose();
            vsink = null;

            pipeline.Bus.Dispose();

            pipeline.Dispose();
            pipeline = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.IsFalse(srcWeakRef.IsAlive);
            Assert.IsFalse(vsinkWeakRef.IsAlive);
            Assert.IsFalse(busWeakRef.IsAlive);
            Assert.IsFalse(pipelineWeakRef.IsAlive);
        }

        private void DeepElementRemoved(object o, DeepElementRemovedArgs args)
        {
            Gst.Object obj = args.Args[1] as Gst.Object;
            obj.Dispose();
            obj = null;
        }
    }
}
