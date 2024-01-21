using Engine;

namespace ProjectActions
{
    public class BaseActions
    {        
        protected Log log;
        public BaseActions(Log l, string prefix)
        {
            log = l;
            log.logPrefix = prefix;
        }

        protected void Wait(int timeout)
        {
            log.Write("Wait in sec.:", timeout.ToString());
            Thread.Sleep(TimeSpan.FromSeconds(timeout));
        }
    }
}