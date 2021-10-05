using System;

namespace LogicService.FlowTask
{
    public abstract class ForegroundTask
    {
        public ForegroundTask()
        {
            this.id = "";
            this.isrunning = false;
            this.isavailable = false;
        }

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
        private string log;

        public string Log
        {
            get { return log; }
            set { log = value; }
        }
    }
}
