using System.Windows;
using FileCopier.ViewModels;

namespace FileCopier.Views
{
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();

            DataContext = new MainViewModel();
        }

        private void Window_OnClosed(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var dataContext = DataContext as MainViewModel;

            if (dataContext.ThreadStarted)
            {
                if (dataContext.CopierThread.ThreadState != System.Threading.ThreadState.Suspended)
                {
                    try
                    {
                        dataContext.CopierThread?.Suspend();
                    }
                    catch { }
                }

                var result = MessageBox.Show("Are you sure you want to exit programs?", "The operation is not over yet!", MessageBoxButton.YesNo, MessageBoxImage.Hand);


                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;

                    try
                    {
                        dataContext.CopierThread?.Resume();
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        dataContext.CopierThread?.Resume();
                        dataContext.CopierThread?.Abort();
                    }
                    catch { }
                }
            }
        }
    }
}
