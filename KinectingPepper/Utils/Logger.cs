using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Kinect_ing_Pepper.Utils
{
    public class Logger
    {
        private static Logger _instance;

        public static Logger Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Logger();
                }
                return _instance;
            }
        }

        private ListView _uiListLogger;

        public void Init(ListView uiListLogger)
        {
            _uiListLogger = uiListLogger;
        }

        public void LogMessage(string message)
        {
            _uiListLogger.Items.Add(message);
            _uiListLogger.ScrollIntoView(message);
        }
    }
}
