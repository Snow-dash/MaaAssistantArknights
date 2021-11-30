using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Stylet;
using StyletIoC;
using Windows.UI.Xaml.Documents;

namespace MeoAsstGui
{
    public class TaskQueueViewModel : Screen
    {
        private IWindowManager _windowManager;
        private IContainer _container;

        public ObservableCollection<DragItemViewModel> TaskItemViewModels { get; set; }
        public ObservableCollection<LogItemViewModel> LogItemViewModels { get; set; }

        public TaskQueueViewModel(IContainer container, IWindowManager windowManager)
        {
            _container = container;
            _windowManager = windowManager;
            DisplayName = "一键长草";
            TaskItemViewModels = new ObservableCollection<DragItemViewModel>();
            LogItemViewModels = new ObservableCollection<LogItemViewModel>();
            InitializeItems();
        }

        public void InitializeItems()
        {
            string stroageKey = "TaskQueue.";
            TaskItemViewModels.Add(new DragItemViewModel("刷理智", stroageKey));
            TaskItemViewModels.Add(new DragItemViewModel("基建换班", stroageKey));
            TaskItemViewModels.Add(new DragItemViewModel("访问好友", stroageKey));
            TaskItemViewModels.Add(new DragItemViewModel("收取信用及购物", stroageKey));
            TaskItemViewModels.Add(new DragItemViewModel("领取日常奖励", stroageKey));
        }

        public void AddLog(string content, string color = "Black", string weight = "Regular")
        {
            LogItemViewModels.Add(new LogItemViewModel(content, color, weight));
            //LogItemViewModels.Insert(0, new LogItemViewModel(time + content, color, weight));
        }

        public void ClearLog()
        {
            LogItemViewModels.Clear();
        }

        public async void LinkStart()
        {
            ClearLog();

            AddLog("正在捕获模拟器窗口……");

            var asstProxy = _container.Get<AsstProxy>();
            var task = Task.Run(() =>
            {
                return asstProxy.AsstCatchDefault();
            });
            bool catchd = await task;
            if (!catchd)
            {
                AddLog("捕获模拟器窗口失败，若是第一次运行，请尝试使用管理员权限", "Red");
                return;
            }
            AddLog("正在运行中……");

            bool ret = true;
            // 直接遍历TaskItemViewModels里面的内容，是排序后的
            foreach (var item in TaskItemViewModels)
            {
                if (item.IsChecked == false)
                {
                    continue;
                }

                if (item.Name == "基建换班")
                {
                    ret &= appendInfrast();
                }
                else if (item.Name == "刷理智")
                {
                    ret &= appendFight();
                }
                else if (item.Name == "访问好友")
                {
                    ret &= asstProxy.AsstAppendVisit();
                }
                else if (item.Name == "收取信用及购物")
                {
                    ret &= appendMall();
                }
                else if (item.Name == "领取日常奖励")
                {
                    ret &= asstProxy.AsstAppendAward();
                }
            }
            setPenguinId();
            ret &= asstProxy.AsstStart();

            if (!ret)
            {
                AddLog("出现未知错误");
            }
            Idle = !ret;
        }

        public void Stop()
        {
            var asstProxy = _container.Get<AsstProxy>();
            asstProxy.AsstStop();
            AddLog("已停止");
            Idle = true;
        }

        private bool appendFight()
        {
            int medicine = 0;
            if (UseMedicine)
            {
                if (!int.TryParse(MedicineNumber, out medicine))
                {
                    medicine = 0;
                }
            }
            int stone = 0;
            if (UseStone)
            {
                if (!int.TryParse(StoneNumber, out stone))
                {
                    stone = 0;
                }
            }
            int times = int.MaxValue;
            if (HasTimesLimited)
            {
                if (!int.TryParse(MaxTimes, out times))
                {
                    times = 0;
                }
            }

            var asstProxy = _container.Get<AsstProxy>();
            return asstProxy.AsstAppendFight(medicine, stone, times);
        }

        private bool appendInfrast()
        {
            var settings = _container.Get<SettingsViewModel>();
            var order = settings.GetInfrastOrderList();
            int orderLen = order.Count;
            var asstProxy = _container.Get<AsstProxy>();
            return asstProxy.AsstAppendInfrast((int)settings.InfrastWorkMode, order.ToArray(), orderLen,
                (int)settings.UsesOfDrones, settings.DormThreshold / 100.0);
        }

        private bool appendMall()
        {
            var settings = _container.Get<SettingsViewModel>();
            var asstProxy = _container.Get<AsstProxy>();
            return asstProxy.AsstAppendMall(settings.CreditShopping);
        }

        private void setPenguinId()
        {
            var settings = _container.Get<SettingsViewModel>();
            var asstProxy = _container.Get<AsstProxy>();
            asstProxy.AsstSetPenguinId(settings.PenguinId);
        }

        public void CheckAndShutdown()
        {
            if (Shutdown == true)
            {
                System.Diagnostics.Process.Start("shutdown.exe", "-s -t 60");

                var result = _windowManager.ShowMessageBox("已刷完，即将关机，是否取消？", "提示", MessageBoxButton.OK);
                if (result == MessageBoxResult.OK)
                {
                    System.Diagnostics.Process.Start("shutdown.exe", "-a");
                }
            }
        }

        private bool _idle = true;

        public bool Idle
        {
            get { return _idle; }
            set
            {
                SetAndNotify(ref _idle, value);
                var settings = _container.Get<SettingsViewModel>();
                settings.Idle = value;
            }
        }

        private bool _shutdown = false;

        public bool Shutdown
        {
            get { return _shutdown; }
            set
            {
                SetAndNotify(ref _shutdown, value);
            }
        }

        private bool _useMedicine = System.Convert.ToBoolean(ViewStatusStorage.Get("MainFunction.UseMedicine", bool.TrueString));

        public bool UseMedicine
        {
            get { return _useMedicine; }
            set
            {
                SetAndNotify(ref _useMedicine, value);
                ViewStatusStorage.Set("MainFunction.UseMedicine", value.ToString());
            }
        }

        private string _medicineNumber = "999";

        public string MedicineNumber
        {
            get { return _medicineNumber; }
            set
            {
                SetAndNotify(ref _medicineNumber, value);
            }
        }

        private bool _useStone;

        public bool UseStone
        {
            get { return _useStone; }
            set
            {
                SetAndNotify(ref _useStone, value);
            }
        }

        private string _stoneNumber = "0";

        public string StoneNumber
        {
            get { return _stoneNumber; }
            set
            {
                SetAndNotify(ref _stoneNumber, value);
            }
        }

        private bool _hasTimesLimited;

        public bool HasTimesLimited
        {
            get { return _hasTimesLimited; }
            set
            {
                SetAndNotify(ref _hasTimesLimited, value);
            }
        }

        private string _maxTimes = "5";

        public string MaxTimes
        {
            get { return _maxTimes; }
            set
            {
                SetAndNotify(ref _maxTimes, value);
            }
        }
    }
}