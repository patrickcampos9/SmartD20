namespace D20Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell(Views.MainPage mainPage)
        {
            InitializeComponent();
            MainShellContent.Content = mainPage;
        }
    }
}
