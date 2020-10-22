using System;
using System.Runtime.InteropServices;
using AOT;
using System.Threading;
using System.Collections.Generic;
using Wrld.Utilities;

namespace Wrld.Concurrency
{
    internal class ThreadService
    {
        private IntPtr m_handleToSelf;
        private Dictionary<int, Thread> m_threads = new Dictionary<int,  Thread>();
        private int m_nextThreadID;
        private const int InvalidThreadID = -1;

        internal delegate IntPtr ThreadStartDelegate(IntPtr startData);

        internal delegate int CreateThreadDelegate(IntPtr threadServiceHandle, ThreadStartDelegate runFunc, IntPtr startData);

        internal delegate void JoinThreadDelegate(IntPtr threadServiceHandle, int threadHandle);

        internal ThreadService()
        {
            m_handleToSelf = NativeInteropHelpers.AllocateNativeHandleForObject(this);
        }

        internal IntPtr GetHandle()
        {
            return m_handleToSelf;
        }

        internal void Destroy()
        {
            NativeInteropHelpers.FreeNativeHandle(m_handleToSelf);
        }

        [MonoPInvokeCallback(typeof(CreateThreadDelegate))]
        static internal int CreateThread(IntPtr threadServiceHandle, ThreadStartDelegate runFunc, IntPtr startData)
        {
            var threadService = threadServiceHandle.NativeHandleToObject<ThreadService>();

            return threadService.CreateThreadInternal(runFunc, startData);
        }

        private int CreateThreadInternal(ThreadStartDelegate runFunc, IntPtr startData)
        {
            int threadID;
            Thread thread;
            
            lock (m_threads)
            {
                threadID = GenerateThreadID();
                thread = new Thread(new ParameterizedThreadStart(start => runFunc((IntPtr)start)));
                m_threads[threadID] = thread;
            }

            thread.Start(startData);

            return threadID;
        }

        [MonoPInvokeCallback(typeof(JoinThreadDelegate))]
        static internal void JoinThread(IntPtr threadServiceHandle, int threadID)
        {
            var threadService = threadServiceHandle.NativeHandleToObject<ThreadService>();

            threadService.JoinThreadInternal(threadID);
        }

        private void JoinThreadInternal(int threadID)
        {
            Thread thread;

            lock (m_threads)
            {
                thread = m_threads[threadID];
                m_threads.Remove(threadID);
            }

            thread.Join();
        }

        private int GenerateThreadID()
        {
            int threadID;

            do
            {
                threadID = m_nextThreadID++;
            }
            while(m_threads.ContainsKey(threadID) || threadID == InvalidThreadID);

            return threadID;
        }
    }
}

