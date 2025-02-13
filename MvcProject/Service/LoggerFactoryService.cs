using log4net;

namespace MvcProject.Service
{
    public interface ILoggerFactoryService
    {
        ILog GetLogger<T>();
    }

    public class LoggerFactoryService : ILoggerFactoryService
    {
        public ILog GetLogger<T>()
        {
            return LogManager.GetLogger(typeof(T));
        }
    }

}
