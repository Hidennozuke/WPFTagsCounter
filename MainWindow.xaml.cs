using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WebTagsСounterWPF.Classes;

namespace WebTagsСounterWPF
{
    public partial class MainWindow : Window
    {
        private List<string> linksList = new List<string>();
        private List<LinkRecord> linkRecords = new List<LinkRecord>();
        private BackgroundWorker worker = null;
        private HtmlWeb web = new HtmlWeb();

        public MainWindow()
        {
            InitializeComponent();
            VisualiseFrontComponents();
            GetJson();
            InitWorkerComponents();
        }

        //inicilise worker
        private void InitWorkerComponents()
        {
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        }

        //show/hide visual modules
        private void VisualiseFrontComponents()
        {
            if (lvLinksShow.Items.Count == 0)
            {
                txbNameOfList.Visibility = Visibility.Collapsed;
                stpHeaderComplect.Visibility = Visibility.Collapsed;

                stpPreview.Visibility = Visibility.Visible;
                stpListStack.Visibility = Visibility.Collapsed;
            }
            else
            {
                txbNameOfList.Visibility = Visibility.Visible;
                stpHeaderComplect.Visibility = Visibility.Visible;

                stpPreview.Visibility = Visibility.Collapsed;
                stpListStack.Visibility = Visibility.Visible;
            }

            if (pbStatus.Visibility == Visibility.Collapsed)
            {
                pbStatus.Visibility = Visibility.Visible;
                txbProgressTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                pbStatus.Visibility = Visibility.Collapsed;
                txbProgressTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        //get json data
        public void GetJson()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "./DataStorage/UrlList.json");
            string jsonContents = File.ReadAllText(path);
            var getModel = Newtonsoft.Json.JsonConvert.DeserializeObject<UrlModel>(jsonContents);
            foreach (var item in getModel.links)
            {
                linksList.Add(item.ToString());
            }
        }

        //grabbing
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            int WorkerCounter = 0;
            worker.ReportProgress(0, String.Format("Соединение..."));
            try
            {
                foreach (var item in linksList)
                {
                    try
                    {
                        HtmlDocument doc = web.Load(item);
                        int aCount = doc.DocumentNode.Descendants("a").Count();

                        if (!worker.CancellationPending)
                        {
                            linkRecords.Add(new LinkRecord(item, aCount));
                        }
                        if (worker.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        worker.ReportProgress(WorkerCounter++, String.Format("Страниц просмотрено {0}", WorkerCounter));
                    }
                    catch (Exception)
                    {
                        worker.ReportProgress(0, String.Format("Ошибка чтения страницы"));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            worker.ReportProgress(100);
            ResultShow();
        }

        //fill listview
        private async void ResultShow()
        {
            await Task.Run(() =>
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    foreach (var res in linkRecords)
                    {
                        lvLinksShow.Items.Add($"∙  Адрес: {res.link}, количество тегов: {res.count}");
                    }

                    var favorite = linkRecords.OrderByDescending(x => x.count).FirstOrDefault();
                    if (favorite != null)
                    {
                        txbFavorite.Text = $"Адрес: {favorite.link}, количество тегов: {favorite.count}";
                    }

                    VisualiseFrontComponents();
                    linkRecords.Clear();
                }));
            });
        }

        //progressbar
        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbStatus.Maximum = linksList.Count();
            pbStatus.Value = e.ProgressPercentage;
            txbProgressTextBlock.Text = (string)e.UserState;
        }

        //action in end of run worker
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                txbProgressTextBlock.Text = "Отменено";
            }
            else
            {
                txbProgressTextBlock.Text = $"Завершено проходов: {linksList.Count()}";
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (!worker.IsBusy)
            {
                lvLinksShow.Items.Clear();
                VisualiseFrontComponents();
                worker.RunWorkerAsync();
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (worker.IsBusy)
            {
                VisualiseFrontComponents();
                worker.CancelAsync();
                linkRecords.Clear();
            }
        }
    }
}