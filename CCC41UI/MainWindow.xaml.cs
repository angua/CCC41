using System.Windows;

namespace CCCUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnScenarioChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is MainViewModel viewmodel)
        {
            if (e.NewValue is ScenarioNode node && node.FileDataSet != null)
            {
                viewmodel.CurrentFileDataSet = node.FileDataSet;
            }
        }
    }

}