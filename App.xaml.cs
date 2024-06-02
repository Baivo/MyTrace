namespace MyTrace
{
    public partial class App : Application
    {
        public static event Action OnNavigatedBack = delegate { };
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new MainPage());
        }
        public static void NotifyNavigatedBack()
        {
            OnNavigatedBack.Invoke();
        }
    }
}
