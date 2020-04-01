using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicService.FlowTask
{
    public class ForegroundTask
    {

        private string _id;
        private bool _isrunning;
        private bool _isavailable;

        public string ID
        {
            get { return _id; }
        }

        public bool IsRunning
        {
            get { return _isrunning; }
        }

        public bool IsAvailable
        {
            get { return _isavailable; }
        }

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
