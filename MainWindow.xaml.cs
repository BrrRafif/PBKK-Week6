using StudentRegistration.ViewModels;
using System.Windows;


namespace StudentRegistration;

// [PDF bagian 15] [PPT slide 19] DataContext
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new StudentViewModel();
    }
}