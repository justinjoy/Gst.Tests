using System;
using System.Threading;
using NUnit.Framework;

namespace Gst.Tests
{
    [TestFixture]
    public abstract class TestBase
    {
        GLib.MainLoop mainLoop;
        Thread thread;

        void Run(object o)
        {
            GLib.MainContext context = new GLib.MainContext();
            mainLoop = new GLib.MainLoop(context);
        }

        [SetUp]
        public void BaseSetUp()
        {
            Assert.IsTrue(Application.InitCheck());

            thread = new Thread(Run);
            thread.Start();
        }

        [TearDown]
        public void BaseTearDown()
        {
            mainLoop.Quit();
            thread.Join();
        }
    }
}
