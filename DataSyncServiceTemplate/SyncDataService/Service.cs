using log4net;
using System.ServiceProcess;

namespace SyncDataService
{
    public partial class Service : ServiceBase
    {
        private static readonly ILog Logger = LogManager.GetLogger("MyLogger");

        private readonly InstantEmailService email;
        private readonly DataService s;
        public Service()
        {
            InitializeComponent();
            email = new InstantEmailService();
            s = new DataService();
        }

        protected override void OnStart(string[] args)
        {
            email.Start();
            s.Start();
            Logger.Info("服务已启动！");
        }

        protected override void OnStop()
        {
            Logger.Info("服务已停止！");
            s.Stop();
            email.Stop();
        }

        protected override void OnShutdown()
        {
            Logger.Info("服务因操作系统原因意外终止！");
            Stop();
        }


        public void Start()
        {
            OnStart(null);
        }
    }
}
