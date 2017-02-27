using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyWpfForismatic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region class variables

        System.Windows.Threading.DispatcherTimer dispatcherTimer;
        DateTime dtLastForismaticApiUpdated = DateTime.Now; // DateTime.Now.AddSeconds(-10);

        // Transparency for main window
        bool isMainWindowTransparent = false;

        bool IsTopMostWindow = false; // is main window - TopMost ???
        bool IsFirstMonitor = true; // radioFirstMonitor
        bool IsLeftTopCorner = true; // radioLeftTop

        int CorrectionX = 0; // textCorrectionX
        int CorrectionY = 0; // textCorrectionY

        // main window transparency
        //
        int MainWindowTransparency = 80;

        bool isStopTimer = false;
        // Timer tick at seconds
        public int TimerTicks = 30;        //  every 10 second timer will ticks

        //// Initial main window sizes
        //double iniWidth = 300;
        //double iniHeight = 230; //240;

        // current sizes of main window
        EnumMainWindowSize windowSize = EnumMainWindowSize.Small;

        // There is two properties you can look into:
        double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
        double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

        //
        //---------------   Forismatic API members & handlers ---------------->>>
        //
        BackgroundWorker workerForismatic;
        public Forismatic thought = new Forismatic();

        public bool isWorkerThreadDone = false;

        //// PrevId = NextId;
        //// PrevAuthorId = NextAuthorId;
        //
        public int PrevId = -1;
        public int PrevAuthorId = -10;
        public string PrevThought = "";
        public string PrevAuthor = "";
        public string PrevLink = "";

        // NextId = -1;
        // NextAuthorId = -1;

        /// <summary> NextId == Thoughts.Id at SqlServer </summary>
        public int NextId = -1;
        /// <summary> NextAuthorId == Thoughts.AuthorId at SqlServer </summary>
        public int NextAuthorId = -1;
        /// <summary> NextThought == Forismatic.Text from forismatic.com </summary>
        public string NextThought = "";
        /// <summary> NextAuthor == Forismatic.Author from forismatic.com </summary>
        public string NextAuthor = "";
        /// <summary> NextLink == Forismatic.Link from forismatic.com </summary>
        public string NextLink = "";

        #endregion class variables

        #region MainWindow()

        public MainWindow()
        {
            // for Circle ProgressBar 
            this.DataContext = this;

            InitializeComponent();

            //  DispatcherTimer setup
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);

            // dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            // dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 800);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);

            // dispatcherTimer.Start();

            //
            //----------------    Перехватываем вход / выход из Hibernate Mode   -------->>>
            //
            SystemEvents.PowerModeChanged += OnPowerChange;
        }
        #endregion MainWindow()

        #region OnPowerChange()
        /// <summary>
        /// Перехватываем вход / выход из Hibernate Mode
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void OnPowerChange(object s, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                // The operating system is about to resume from a suspended state.
                case PowerModes.Resume:
                    Console.WriteLine("--- OnPowerChange()  ---    PowerModes.Resume ---");
                    //++
                    // Возобновляем текущую задачу
                    btnPlay_Click(null, null);
                    break;

                // The operating system is about to be suspended.
                case PowerModes.Suspend:
                    Console.WriteLine("--- OnPowerChange()  ---    PowerModes.Suspend ---");
                    //++
                    // Ставим на паузу текущую задачу
                    btnPause_Click(null, null);
                    break;
            }
        }
        #endregion OnPowerChange()

        #region Window_Loaded()

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Set main window position
            SetMainWindowPosition(true);

            // set resource for popup label VolumePopupText
            try
            {
                this.Resources["VolumePopupText"] = TimerTicks.ToString() + " s.";
            }
            catch
            {
                // MessageBox.Show(x.Message);
            }

            //
            //------------------  Getting smart thoughts from Forismatic.com   ----------------------------->>>
            //
            workerForismatic = new BackgroundWorker();
            workerForismatic.DoWork += workerForismatic_DoWork;
            workerForismatic.RunWorkerCompleted += workerForismatic_RunWorkerCompleted;

            // start timer
            dispatcherTimer.Start(); // chkReload.Checked = true;

            //
            //------------------  Load thoughts asynchronosly from SqlServer  ----------------------------->>>
            //
            ThoughtsRepository.LoadDataFromServer();
        }
        #endregion Window_Loaded()

        //
        //---------------   Forismatic API members & handlers ---------------->>>
        //
        #region Getting API Forismatic.com

        private void GetForismaticThought()
        {
            try
            {
                workerForismatic.RunWorkerAsync();
            }
            catch { }
        }

        public void workerForismatic_DoWork(object sender, DoWorkEventArgs e)
        {
            isWorkerThreadDone = false;

            // Return the value through the Result property.
            e.Result = Forismatic.GetNextThought();
        }
        public void workerForismatic_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Access the result through the Result property.
            thought = e.Result as Forismatic;

            if (thought != null)
            {
                PrevId = NextId;
                NextId = -1;
                PrevAuthorId = NextAuthorId;
                NextAuthorId = -1;

                if (!string.IsNullOrEmpty(thought.Text))
                {
                    PrevThought = NextThought.Trim();
                    NextThought = thought.Text;
                }

                if (!string.IsNullOrEmpty(thought.Author))
                {
                    // NextThought += "\r\n---\r\n" + thought.Author;
                    PrevAuthor = NextAuthor.Trim();
                    NextAuthor = thought.Author.Trim();
                }

                //++ AuthorId
                //
                PrevAuthorId = NextAuthorId;
                NextAuthorId = ThoughtsRepository.GetAuthorIdByAuthorName(NextAuthor);


                // NextLink
                if (!string.IsNullOrEmpty(thought.Link))
                {
                    // NextThought += "\r\n---\r\n" + thought.Author;
                    PrevLink = NextLink.Trim();
                    NextLink = thought.Link.Trim();
                }
            }
            else
            {
                NextId = -1;
                NextAuthorId = -1;

                NextThought = "";
                NextAuthor = "There was an error.";
                NextLink = "";
            }

            //++
            isWorkerThreadDone = true;
            swapPathAnimation(EnumPathGeometryState.Play);
        }

        #endregion Getting API Forismatic.com

        //
        //--------------------------    Timer  ticks    -------------------------->>>
        //
        #region  Timer  ticks  
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // string s1 = "", s2 = "";
            DateTime dtNow = DateTime.Now;
            try
            {
                // show new image every TimerTicks second
                //
                if (!isStopTimer)
                {
                    if (dtLastForismaticApiUpdated.AddSeconds(TimerTicks) < dtNow)
                    {
                        // to handle this code block once
                        //
                        dtLastForismaticApiUpdated = dtNow;

                        btnPlay_Click(null, null);
                    }
                    else
                    {
                        // my circle progress bar position
                        //
                        if (isWorkerThreadDone)
                        {
                            TimeSpan ts_passed = dtNow.Subtract(dtLastForismaticApiUpdated);
                            double passed = ts_passed.TotalMilliseconds;
                            PctComplete = passed / (10 * TimerTicks); // (double)(100.0 / TimerTicks); //+= 1.0;

                            // set resource for popup label VolumePopupText
                            try
                            {
                                TimeSpan ts_left = dtLastForismaticApiUpdated.AddSeconds(TimerTicks) - dtNow;

                                int min_left = ts_left.Minutes;
                                int sec_left = ts_left.Seconds;
                                int ms_left = ts_left.Milliseconds;
                                if (ms_left > 500 && sec_left < 59)
                                    sec_left += 1;
                                string txt_left = string.Format("{0}:{1}", min_left, sec_left.ToString("00"));

                                this.Resources["LeftTimeText"] = txt_left;
                            }
                            catch
                            {
                                // MessageBox.Show(x.Message);
                            }

                            if (!NextThought.Equals(txtThought.Text))
                            {
                                txtThought.Text = NextThought.Trim();
                                txtThought.FontSize = NextThought.Length > 250 ? 12 : NextThought.Length > 150 ? 13 : 14;
                                txtAuthor.Text = NextAuthor.Trim();
                            }

                            // Show mesage at lblAddThought
                            //
                            if (NextId > 0)
                            {
                                lblAddThought.Content = string.Format("Id = {0}", NextId);
                            }
                            //else
                            //{
                            //    lblAddThought.Content = "";
                            //}
                        }
                        else
                        {
                            //// Show mesage at lblAddThought
                            ////
                            //if (NextId > 0)
                            //{
                            //    lblAddThought.Content = string.Format("Id = {0}", NextId);
                            //}
                            //else
                            //{
                            lblAddThought.Content = "";
                            //}
                            dtLastForismaticApiUpdated = DateTime.Now;
                        }

                        if (PctComplete >= 100.0)
                            PctComplete = 100;
                    }
                } //                     if (!isStopTimer)
            }
            catch (Exception x)
            {
                // textPosition.Text = x.Message;
            }

            // Forcing the CommandManager to raise the RequerySuggested event
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion  Timer  ticks  


        //
        //------------------ Circle ProgressBar handlers   ------------------------>>>
        //
        #region Circle ProgressBar handlers 

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected virtual void OnPropertyChanged(string prop)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private double pctComplete = 10.0;
        public double PctComplete
        {
            get { return pctComplete; }
            set
            {
                if (pctComplete != value)
                {
                    pctComplete = value;
                    OnPropertyChanged("PctComplete");
                }
            }
        }
        #endregion Circle ProgressBar handlers 


        //
        //------------------ Main Window Position & Transparency   ------------------------>>>
        //
        #region SetMainWindowPosition()
        /// <summary>
        /// Set position for main window
        /// </summary>
        /// <param name="is_update_image">update image inside this function</param>
        private void SetMainWindowPosition(bool is_update_image = false)
        {
            // Load settings from registry
            //
            LoadSettingFromRegistry();

            // is topmost window ???
            this.Topmost = IsTopMostWindow;

            // transparency for main window
            SetMainWindowTransparency();

            // left correction for main window position
            // double left = 0;
            double left = windowSize == EnumMainWindowSize.Largest ? 202 : 0;

            // set timer for Images
            if (is_update_image)
                dtLastForismaticApiUpdated = DateTime.Now.AddSeconds(-TimerTicks);

            // There is two properties you can look into:
            screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

            if (IsFirstMonitor)
            {
                if (IsLeftTopCorner)
                    this.Left = CorrectionX - left;
                else
                    this.Left = screenWidth - this.Width - CorrectionX - left;

                this.Top = CorrectionY; // 220; // 136;
            }
            else
            {
                var secondaryScreen = Screen.AllScreens.Where(s => !s.Primary).FirstOrDefault();

                if (secondaryScreen != null)
                {
                    double virtual_height = SystemParameters.VirtualScreenHeight;
                    double virtual_width = SystemParameters.VirtualScreenWidth;
                    double virtual_top = SystemParameters.VirtualScreenTop;
                    // double virtual_left = SystemParameters.VirtualScreenLeft;


                    if (IsLeftTopCorner)
                        this.Left = screenWidth + CorrectionX - left;
                    else
                        this.Left = virtual_width - this.Width - CorrectionX - left;

                    this.Top = virtual_top + CorrectionY;

                    //++ set screen sizes for btnOpenDb_Click() handler

                    // screenHeight = virtual_height * screenWidth / virtual_width - virtual_top;
                    screenHeight = screenHeight * (virtual_width - screenWidth) / screenWidth + virtual_top - 27;
                    screenWidth = virtual_width - screenWidth;
                }
                else
                {
                    if (IsLeftTopCorner)
                        this.Left = CorrectionX;
                    else
                        this.Left = screenWidth - this.Width - CorrectionX;

                    this.Top = CorrectionY;
                }
            }
        }

        #endregion SetMainWindowPosition()

        #region SetMainWindowTransparency()
        private void SetMainWindowTransparency()
        {
            // isMainWindowTransparent = !isMainWindowTransparent;
            if (isMainWindowTransparent)
            {
                this.Opacity = (double)MainWindowTransparency / 100.0; // 0.75; // 0.8;
            }
            else
            {
                this.Opacity = 1;
            }
        }
        #endregion SetMainWindowTransparency()


        //
        //------------------ Save & Restore settings to/from registry   -------------------------->>>
        //
        #region SaveSettingToRegistry()

        /// <summary>
        /// Сохряняем настройки программы в реестр
        /// </summary>
        /// <returns></returns>
        private bool SaveSettingToRegistry()
        {
            // Create or get existing Registry subkey
            RegistryKey key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\MyWpfForismatic");


            // Transparency Checkbox initial setting
            //
            key.SetValue("isMainWindowTransparent", isMainWindowTransparent);

            // TopMost Checkbox initial setting
            //
            key.SetValue("IsTopMostWindow", IsTopMostWindow);

            // Position at Monitor
            //
            key.SetValue("IsFirstMonitor", IsFirstMonitor);
            key.SetValue("IsLeftTopCorner", IsLeftTopCorner);

            // Main window X and Y corrections
            //
            key.SetValue("CorrectionX", CorrectionX < 0 ? 0 : CorrectionX > 1000 ? 1000 : CorrectionX);
            key.SetValue("CorrectionY", CorrectionY < 0 ? 0 : CorrectionY > 1000 ? 1000 : CorrectionY);

            // main window transparency
            //
            // int MainWindowTransparency = 80;
            key.SetValue("MainWindowTransparency", MainWindowTransparency < 10 ? 10 : MainWindowTransparency > 100 ? 100 : MainWindowTransparency);

            // Timer tick at seconds
            // public int TimerTicks = 10;        //  every 10 second timer will ticks
            //
            key.SetValue("TimerTicks", TimerTicks < 5 ? 5 : TimerTicks > 100 ? 100 : TimerTicks);

            return true;
        }

        #endregion SaveSettingToRegistry()

        #region LoadSettingFromRegistry()

        /// <summary>
        /// Загружаем настройки из реестра
        /// </summary>
        /// <returns></returns>
        private bool LoadSettingFromRegistry()
        {
            // Get the value stored in the Registry
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\MyWpfForismatic");

            // Common settings
            //
            if (key == null)
            {
                // Transparency Checkbox initial setting
                //
                // key.SetValue("isMainWindowTransparent", isMainWindowTransparent);
                isMainWindowTransparent = false;

                // TopMost Checkbox initial setting
                //
                IsTopMostWindow = true;

                // Position at Monitor
                //
                IsFirstMonitor = true;
                IsLeftTopCorner = true;

                // Main window X and Y corrections
                //
                CorrectionX = 10;
                CorrectionY = 200;

                // main window transparency
                //
                MainWindowTransparency = 80;


                // Timer tick at seconds
                // public int TimerTicks = 10;        //  every 10 second timer will ticks
                //
                // key.SetValue("TimerTicks", TimerTicks < 5 ? 5 : TimerTicks > 100 ? 100 : TimerTicks);
                TimerTicks = 10;
            }
            else // if (key == null)
            {
                bool is_test = false;
                // string s_test = "";

                // Transparency Checkbox initial setting
                //
                // key.SetValue("isMainWindowTransparent", isMainWindowTransparent);
                // isMainWindowTransparent = false;
                if (key.GetValue("isMainWindowTransparent") != null && bool.TryParse(key.GetValue("isMainWindowTransparent").ToString(), out is_test))
                    isMainWindowTransparent = is_test;


                // TopMost Checkbox initial setting
                //
                if (key.GetValue("IsTopMostWindow") != null && bool.TryParse(key.GetValue("IsTopMostWindow").ToString(), out is_test))
                    IsTopMostWindow = is_test;

                // Position at Monitor
                //
                if (key.GetValue("IsFirstMonitor") != null && bool.TryParse(key.GetValue("IsFirstMonitor").ToString(), out is_test))
                    IsFirstMonitor = is_test;
                if (key.GetValue("IsLeftTopCorner") != null && bool.TryParse(key.GetValue("IsLeftTopCorner").ToString(), out is_test))
                    IsLeftTopCorner = is_test;

                // Main window X and Y corrections
                //
                if (key.GetValue("CorrectionX") != null)
                    CorrectionX = (int)key.GetValue("CorrectionX");
                if (key.GetValue("CorrectionY") != null)
                    CorrectionY = (int)key.GetValue("CorrectionY");

                // main window transparency
                //
                // MainWindowTransparency = 80;
                if (key.GetValue("MainWindowTransparency") != null)
                    MainWindowTransparency = (int)key.GetValue("MainWindowTransparency");



                // Timer tick at seconds
                //
                // key.SetValue("TimerTicks", TimerTicks < 5 ? 5 : TimerTicks > 100 ? 100 : TimerTicks);
                if (key.GetValue("TimerTicks") != null)
                    TimerTicks = (int)key.GetValue("TimerTicks");

            }
            return true;
        }
        #endregion LoadSettingFromRegistry()



        #region question_MouseEnter()
        private void question_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            popVideoControls.IsOpen = true;
        }
        #endregion question_MouseEnter()

        #region question_MouseLeftButtonDown()
        private void question_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            popVideoControls.IsOpen = false;

            //if (IsShowImages)
            //{
            //    btnSize2_Click(null, null);
            //}
            btnPrev_Click(null, null);
        }
        #endregion question_MouseLeftButtonDown()

        #region question_MouseRightButtonDown()
        private void question_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            popVideoControls.IsOpen = false;
        }
        #endregion question_MouseRightButtonDown()

        #region btnOpenDb_Click()
        private void btnOpenDb_Click(object sender, RoutedEventArgs e)
        {
            popVideoControls.IsOpen = false;

            frmThoughts frm = new frmThoughts();
            frm.Owner = this;
            frm.ShowDialog();
        }
        #endregion btnOpenDb_Click()

        #region btnSettings_Click()
        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            popVideoControls.IsOpen = false;

            // sliderMain.Focus();

            //
            frmSettings frm = new frmSettings(this);
            frm.Owner = this;

            frm.IsFirstMonitor = IsFirstMonitor;
            frm.IsLeftTopCorner = IsLeftTopCorner;

            frm.CorrectionX = CorrectionX;
            frm.CorrectionY = CorrectionY;

            // main window transparency
            //
            frm.MainWindowTransparency = MainWindowTransparency;

            // stop playing video
            // btnStop_Click(null, null);
            btnPause_Click(null, null);

            if (frm.ShowDialog() == true)
            {
                IsFirstMonitor = frm.IsFirstMonitor;
                IsLeftTopCorner = frm.IsLeftTopCorner;

                CorrectionX = frm.CorrectionX;
                CorrectionY = frm.CorrectionY;

                // main window transparency
                //
                MainWindowTransparency = frm.MainWindowTransparency;

                // Save settings to registry
                //
                SaveSettingToRegistry();

                // Set position for main window
                SetMainWindowPosition(true);

                // start playing video
                btnPlay_Click(null, null);
            }
        }
        #endregion btnSettings_Click()

        #region btnTopMost_On_Click()
        private void btnTopMost_On_Click(object sender, RoutedEventArgs e)
        {
            popVideoControls.IsOpen = false;

            // sliderMain.Focus();

            IsTopMostWindow = true;
            // this.Topmost = IsTopMostWindow;

            //++
            SaveSettingToRegistry();

            // Set main window position
            SetMainWindowPosition();

            // show images or player buttons
            //
            ShowPlayerOrImagesButtons(true);
        }
        #endregion btnTopMost_On_Click()

        #region btnTopMost_Off_Click()
        private void btnTopMost_Off_Click(object sender, RoutedEventArgs e)
        {
            popVideoControls.IsOpen = false;

            // sliderMain.Focus();

            IsTopMostWindow = false;
            // this.Topmost = IsTopMostWindow;

            //++
            SaveSettingToRegistry();

            // Set main window position
            SetMainWindowPosition();

            // show images or player buttons
            //
            ShowPlayerOrImagesButtons(true);
        }
        #endregion btnTopMost_Off_Click()

        #region btnExit_Click()
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            popVideoControls.IsOpen = false;
            this.Close();
        }
        #endregion btnExit_Click()

        #region btnPlay_Click()
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            popVideoControls.IsOpen = false;

            PctComplete = 0.0;
            swapPathAnimation(EnumPathGeometryState.Stopped);

            // sliderMain.Focus();
            dtLastForismaticApiUpdated = DateTime.Now;

            //// Show mesage at lblAddThought
            ////
            //if (NextId > 0)
            //{
            //    lblAddThought.Content = string.Format("Id = {0}", NextId);
            //}
            //else
            //{
            lblAddThought.Content = "";
            //}

            try
            {
                // show images or player buttons
                //
                ShowPlayerOrImagesButtons(true);

                GetForismaticThought();

                isStopTimer = false;
            }
            catch (Exception x)
            {
                System.Windows.MessageBox.Show(x.Message, "btnPlay_Click()");
            }
        }
        #endregion btnPlay_Click()

        #region btnPause_Click()
        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            popVideoControls.IsOpen = false;

            // sliderMain.Focus();
            dtLastForismaticApiUpdated = DateTime.Now;

            // pause path animation
            swapPathAnimation(EnumPathGeometryState.Paused);

            try
            {
                // show images or player buttons
                //
                ShowPlayerOrImagesButtons(false);

                isStopTimer = true;

                //// fileImagesNameShort
                //// textFileName2.Text = fileImagesNameShort.Trim();
                //textFileName2.Text = windowSize == EnumMainWindowSize.Small ? currentImage.ImagePathShort : currentImage.ImagePath;

                //textStatus.Text = string.Format("{0} {1}", isStopTimer ? "pause" : "play", TimerTicks);
            }
            catch (Exception x)
            {
                System.Windows.MessageBox.Show(x.Message, "btnPause_Click()");
            }
        }
        #endregion btnPause_Click()

        #region btnPrev_Click()
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            popVideoControls.IsOpen = false;

            // pause path animation
            swapPathAnimation(EnumPathGeometryState.Stopped);
            isStopTimer = true;

            try
            {
                if (PrevThought.Length > 0)
                {
                    // NextId & NextAuthorId
                    //
                    int n_swap = NextId;
                    NextId = PrevId;
                    PrevId = n_swap;

                    n_swap = NextAuthorId;
                    NextAuthorId = PrevAuthorId;
                    PrevAuthorId = n_swap;


                    // NextThought & NextAuthor & NextLink
                    //
                    string s_swap = NextThought.Trim();
                    NextThought = PrevThought.Trim();
                    PrevThought = s_swap;
                    s_swap = NextAuthor.Trim();
                    NextAuthor = PrevAuthor.Trim();
                    PrevAuthor = s_swap;

                    s_swap = NextLink.Trim();
                    NextLink = PrevLink.Trim();
                    PrevLink = s_swap;

                    //++
                    txtThought.Text = NextThought.Trim();
                    txtThought.FontSize = NextThought.Length > 150 ? 13 : 14;
                    txtAuthor.Text = NextAuthor.Trim();

                    // Show mesage at lblAddThought
                    //
                    if (NextId > 0)
                    {
                        lblAddThought.Content = string.Format("Id = {0}", NextId);
                    }
                    else
                    {
                        lblAddThought.Content = "";
                    }
                }

                // resume path animation
                isStopTimer = false;
                swapPathAnimation(EnumPathGeometryState.Play);

                //++
                // Plays the sound associated with the Asterisk system event.
                // SystemSounds.Asterisk.Play();
                SystemSounds.Beep.Play();
            }
            catch (Exception x)
            {
                //++
                // Plays the sound associated with the Asterisk system event.
                // SystemSounds.Asterisk.Play();
                SystemSounds.Hand.Play();

                System.Windows.MessageBox.Show(x.Message, "btnPrev_Click()");
            }
        }
        #endregion btnPrev_Click()

        #region btnTransparencyOn_Click()
        private void btnTransparencyOn_Click(object sender, RoutedEventArgs e)
        {
            popVideoControls.IsOpen = false;

            // set transparency flag here
            isMainWindowTransparent = true;

            //++
            SaveSettingToRegistry();

            // Set main window position
            SetMainWindowPosition();

            // show images or player buttons
            //
            ShowPlayerOrImagesButtons(true);
        }
        #endregion btnTransparencyOn_Click()

        #region btnTransparencyOff_Click()
        private void btnTransparencyOff_Click(object sender, RoutedEventArgs e)
        {
            popVideoControls.IsOpen = false;

            // set transparency flag here
            isMainWindowTransparent = false;

            //++
            SaveSettingToRegistry();

            // Set main window position
            SetMainWindowPosition();

            // show images or player buttons
            //
            ShowPlayerOrImagesButtons(true);
        }
        #endregion btnTransparencyOff_Click()

        #region btnVolumeDown_Click()
        private void btnVolumeDown_Click(object sender, RoutedEventArgs e)
        {
            // popVideoControls.IsOpen = false;

            try
            {
                // TimerTicks
                // TimerTicks = TimerTicks < 6 ? 5 : TimerTicks < 20 ? TimerTicks - 1 : TimerTicks < 40 ? TimerTicks - 2 : TimerTicks - 3;
                TimerTicks = TimerTicks <= 5 ? 5 : TimerTicks - 1;

                // set resource for popup label VolumePopupText
                try
                {
                    this.Resources["VolumePopupText"] = TimerTicks.ToString() + " s.";
                }
                catch
                {
                    // MessageBox.Show(x.Message);
                }

                // Stop path animation
                //
                swapPathAnimation(EnumPathGeometryState.Paused);

                // save value to registry
                SaveSettingToRegistry();

                //  Set new geomerty for path animation
                //
                SetNewPathGeometry(!isStopTimer);

            }
            catch (Exception x)
            {
                System.Windows.MessageBox.Show(x.Message, "btnVolumeUp_Click()");
            }
        }
        #endregion btnVolumeDown_Click()

        #region btnVolumeUp_Click()
        private void btnVolumeUp_Click(object sender, RoutedEventArgs e)
        {
            // popVideoControls.IsOpen = false;

            try
            {
                // TimerTicks
                // TimerTicks = TimerTicks > 95 ? 100 : TimerTicks > 40 ? TimerTicks + 3 : TimerTicks > 20 ? TimerTicks + 2 : TimerTicks + 1;
                TimerTicks = TimerTicks >= 100 ? 100 : TimerTicks + 1;

                // set resource for popup label VolumePopupText
                try
                {
                    this.Resources["VolumePopupText"] = TimerTicks.ToString() + " s.";
                }
                catch
                {
                    // MessageBox.Show(x.Message);
                }

                // Stop path animation
                //
                swapPathAnimation(EnumPathGeometryState.Paused);

                // save value to registry
                SaveSettingToRegistry();

                //  Set new geomerty for path animation
                //
                SetNewPathGeometry(!isStopTimer);
            }
            catch (Exception x)
            {
                System.Windows.MessageBox.Show(x.Message, "btnVolumeUp_Click()");
            }
        }
        #endregion btnVolumeUp_Click()


        #region ShowPlayerOrImagesButtons()
        /// <summary>
        /// What show - images or player buttons at Popup window ???
        /// </summary>
        /// <param name="is_playing">is now playing mode ???</param>
        private void ShowPlayerOrImagesButtons(bool is_playing = true)
        {
            double ImageButtonsWidth = 0;
            double VideoButtonsWidth = 0;

            // canvasThought.Background.Opacity = 1;

            VideoButtonsWidth = 24;
            try
            {
                // Transparency Checkbox initial setting
                //
                // key.SetValue("isMainWindowTransparent", isMainWindowTransparent);
                // isMainWindowTransparent = false;
                this.Resources["IsTransparencyOn"] = isMainWindowTransparent ? VideoButtonsWidth : 0;
                this.Resources["IsTransparencyOff"] = isMainWindowTransparent ? 0 : VideoButtonsWidth;


                // is main window - topmost ???
                this.Resources["IsTopMostOn"] = IsTopMostWindow ? VideoButtonsWidth : 0;
                this.Resources["IsTopMostOff"] = IsTopMostWindow ? 0 : VideoButtonsWidth;

                this.Resources["ImageButtonsWidth"] = ImageButtonsWidth;
                this.Resources["VideoButtonsWidth"] = VideoButtonsWidth;

                // set resource for popup label VolumePopupText
                this.Resources["VolumePopupText"] = TimerTicks.ToString() + " s.";

                // if now playing mode ==> btnPause - invisible
                this.Resources["PlayButtonsWidth"] = is_playing ? 0 : VideoButtonsWidth;
                this.Resources["PauseButtonsWidth"] = !is_playing ? 0 : VideoButtonsWidth;
            }
            catch (Exception x)
            {
                // MessageBox.Show(x.Message);
            }
        }
        #endregion ShowPlayerOrImagesButtons()

        #region swapPathAnimation()
        /// <summary>
        /// Start / stop path animation
        /// </summary>
        /// <param name="play_state">Start / Stop / Pause animation ???</param>
        public void swapPathAnimation(EnumPathGeometryState play_state = EnumPathGeometryState.Stopped)
        {
            Storyboard story5 = (Storyboard)this.FindResource("story5");
            if (story5 != null)
            {
                // Begin new animation
                //
                if (play_state == EnumPathGeometryState.Play)
                {
                    // total duration = TimerTicks seconds
                    //
                    Duration dur1 = new Duration(TimeSpan.FromSeconds(TimerTicks));


                    var pathAnim1 = story5.Children[0] as DoubleAnimationUsingPath;
                    if (pathAnim1 != null)
                    {
                        pathAnim1.SetValue(DoubleAnimationUsingPath.DurationProperty, dur1);
                    }
                    var pathAnim2 = story5.Children[1] as DoubleAnimationUsingPath;
                    if (pathAnim2 != null)
                    {
                        pathAnim2.SetValue(DoubleAnimationUsingPath.DurationProperty, dur1);
                    }
                    //
                    // <Storyboard Storyboard.TargetName="ball5" BeginTime="-0:0:2">
                    //
                    // dtLastForismaticApiUpdated = DateTime.Now;
                    TimeSpan tsStartTime = TimeSpan.FromSeconds(0); // dtLastForismaticApiUpdated.Subtract(DateTime.Now);
                    story5.BeginTime = tsStartTime;
                    story5.Begin();
                }
                else if (play_state == EnumPathGeometryState.Resumed)
                {
                    // Resume current animation
                    //

                    // total duration = TimerTicks seconds
                    //
                    Duration dur1 = new Duration(TimeSpan.FromSeconds(TimerTicks));


                    var pathAnim1 = story5.Children[0] as DoubleAnimationUsingPath;
                    if (pathAnim1 != null)
                    {
                        pathAnim1.SetValue(DoubleAnimationUsingPath.DurationProperty, dur1);
                    }
                    var pathAnim2 = story5.Children[1] as DoubleAnimationUsingPath;
                    if (pathAnim2 != null)
                    {
                        pathAnim2.SetValue(DoubleAnimationUsingPath.DurationProperty, dur1);
                    }

                    //
                    // <Storyboard Storyboard.TargetName="ball5" BeginTime="-0:0:2">
                    //
                    // dtLastForismaticApiUpdated = DateTime.Now;
                    TimeSpan tsStartTime = dtLastForismaticApiUpdated.Subtract(DateTime.Now);
                    story5.BeginTime = tsStartTime;

                    // story5.Resume();
                    story5.Begin();
                }
                else if (play_state == EnumPathGeometryState.Paused)
                {
                    // Pause current animation
                    //

                    story5.Pause();
                    // story5.Stop();
                }
                else if (play_state == EnumPathGeometryState.Stopped)
                {
                    // Stop current animation
                    //

                    dtLastForismaticApiUpdated = DateTime.Now;
                    story5.Stop();
                }
            }
        }
        #endregion swapPathAnimation()


        #region SetNewPathGeometry()
        /// <summary>
        /// Set new geomerty for path animation
        /// </summary>
        /// <param name="resume_path_anim">Resume path animation after changing window size ???</param>
        /// <returns></returns>
        private bool SetNewPathGeometry(bool resume_path_anim = false)
        {
            bool ret = false;
            double wid = this.Width;
            double hei = this.Height;

            //if (this.WindowState == WindowState.Maximized)
            //{
            //    wid = screenWidth;
            //    hei = screenHeight;
            //}

            //// path animation - new geometry
            ////
            //// Height="320" Width="200"
            ////
            ////< PathGeometry x: Key = "geometryPath" >  
            ////      < PathFigure IsClosed = "True" StartPoint = "0, 0" >     
            ////             < PolyLineSegment Points = "178,0   178,298   0,298" />      
            ////          </ PathFigure >      
            ////      </ PathGeometry >

            try
            {
                PathGeometry path5 = (PathGeometry)this.FindResource("geometryPath");
                if (path5 != null)
                {
                    if (path5.Figures[0] != null)
                    {
                        path5.Figures[0].StartPoint = new Point(0, 0);
                        PolyLineSegment seg1 = path5.Figures[0].Segments[0] as PolyLineSegment;
                        if (seg1 != null)
                        {
                            // seg1.Points = new PointCollection();
                            seg1.Points.Clear();
                            seg1.Points.Add(new Point(wid - 22, 0));
                            seg1.Points.Add(new Point(wid - 22, hei - 22));
                            seg1.Points.Add(new Point(0, hei - 22));

                            ret = true;
                        }
                    }
                }
            }
            catch (Exception x)
            {
                ret = false;
            }

            // start / stop new path animation
            //
            swapPathAnimation(isStopTimer ? EnumPathGeometryState.Stopped : resume_path_anim ? EnumPathGeometryState.Resumed : EnumPathGeometryState.Play);

            return ret;
        }


        #endregion SetNewPathGeometry()

        #region btnAddRecordToDb_Click()
        private void btnAddRecordToDb_Click(object sender, RoutedEventArgs e)
        {
            popVideoControls.IsOpen = false;

            if (NextId > 0)
            {
                // If thought was added earlier - return
                //
                lblAddThought.Content = string.Format("Id = {0}", NextId);
                return;
            }

            // txtThought.Text = NextThought.Trim();
            // txtAuthor.Text = NextAuthor.Trim();
            //
            Forismatic new_forismatic = new Forismatic(NextThought, NextAuthor, NextLink);
            Console.WriteLine("--- new_forismatic: {0}, {1}, {2} ---", NextThought, NextAuthor, NextLink);

            NextId = ThoughtsRepository.AddRecordToSqlServer(new_forismatic);
            Console.WriteLine("--- NextId = ThoughtsRepository.AddRecordToSqlServer(new_forismatic): {0} ---", NextId);

            //++ Show to User that everything is OK
            //
            if (NextId > 0)
            {
                borderThought.BorderBrush = Brushes.Wheat;

                //if (NextAuthorId < 0)
                //{
                NextAuthorId = ThoughtsRepository.GetAuthorIdByAuthorName(NextAuthor);
                Console.WriteLine("--- NextAuthorId = ThoughtsRepository.GetAuthorIdByAuthorName(NextAuthor): {0} ---", NextAuthorId);
                //}

                // Show mesage at lblAddThought
                //
                if (NextId > 0)
                {
                    lblAddThought.Content = string.Format("Id = {0}", NextId);
                }
                else
                {
                    lblAddThought.Content = "";
                }
            }
            else
            {
                // Show ERROR at lblAddThought
                //
                if (NextId > 0)
                {
                    lblAddThought.Content = string.Format("ERROR: Id = {0}", NextId);
                }
                else
                {
                    lblAddThought.Content = "ERROR !!!";
                }
                borderThought.BorderBrush = Brushes.Red;
            }

            // Reload Data from SqlServer
            ThoughtsRepository.LoadDataFromServer();
            Console.WriteLine("--- Reload Data from SqlServer ---");
        }
        #endregion txtThought_MouseLeftButtonUp()

        #region txtThought_MouseLeftButtonUp()
        private void txtThought_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            btnAddRecordToDb_Click(null, null);
        }
        #endregion txtThought_MouseLeftButtonUp()

        private void txtAuthor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // if No Author - no actions
            //
            if (txtAuthor.Text.Length > 0)
            {
                frmThoughts frm = new frmThoughts();
                frm.Owner = this;

                //++ NextAuthorId
                if (NextAuthorId <= 0)
                {
                    NextAuthorId = ThoughtsRepository.GetAuthorIdByAuthorName(NextAuthor);
                }
                frm.NextAuthorId = NextAuthorId;

                frm.ShowDialog();
            }
        }

        #region txtAuthor_MouseEnter()
        private void txtAuthor_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // if No Author - not change Cursor
            //
            if (txtAuthor.Text.Length > 0)
            {
                // txtAuthor.Background = Brushes.Brown;
                borderAuthor.BorderBrush = Brushes.Red;
                lblShowThoughts.Visibility = Visibility.Visible;

                //++ Cursor == Hand
                txtAuthor.Cursor = System.Windows.Input.Cursors.Hand;
            }
            else
            {
                //++ Cursor == Arrow
                txtAuthor.Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }
        #endregion txtAuthor_MouseEnter()

        #region txtAuthor_MouseLeave()
        private void txtAuthor_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // txtAuthor.Background = Brushes.Black;
            borderAuthor.BorderBrush = Brushes.Transparent; // Brushes.Black;
            lblShowThoughts.Visibility = Visibility.Collapsed;
        }
        #endregion txtAuthor_MouseLeave()

        #region txtThought_MouseEnter()
        private void txtThought_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // txtThought.Background = Brushes.Brown;
            borderThought.BorderBrush = Brushes.Red;

            // Show mesage at lblAddThought
            //
            if (NextId > 0)
            {
                lblAddThought.Content = string.Format("Id = {0}", NextId);
            }
            else
            {
                lblAddThought.Content = "Add Thought to Db";
            }
        }
        #endregion txtThought_MouseEnter()

        #region txtThought_MouseLeave()
        private void txtThought_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // txtThought.Background = Brushes.Black;
            borderThought.BorderBrush = Brushes.Transparent; // Brushes.Black;

            // Show mesage at lblAddThought
            //
            if (NextId > 0)
            {
                lblAddThought.Content = string.Format("Id = {0}", NextId);
            }
            else
            {
                lblAddThought.Content = "";
            }
        }
        #endregion txtThought_MouseLeave()
    }
}
