using System;

namespace LogicService.FlowTask
{
    public abstract class ForegroundTask
    {

        protected string id;
        protected bool isrunning;
        protected bool isavailable;

        public string ID { get { return id; } }

        public bool IsRunning { get { return isrunning; } }

        public bool IsAvailable { get { return isavailable; } }

        public abstract void Run();

    }

    public class TaskCompletedEventArgs : EventArgs
    {
        private int log;

        public int Log
        {
            get { return log; }
            set { log = value; }
        }
    }
}
