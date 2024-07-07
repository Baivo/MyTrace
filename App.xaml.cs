namespace MyTrace
{
    public partial class App : Application
    {
        public static event Action OnNavigatedBack = delegate { };
        public static IServiceProvider ServiceProvider { get; private set; }

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            ServiceProvider = serviceProvider;
            MainPage = new NavigationPage(ServiceProvider.GetRequiredService<MainPage>());
        }

        public static void NotifyNavigatedBack()
        {
            OnNavigatedBack.Invoke();
        }
    }
}
