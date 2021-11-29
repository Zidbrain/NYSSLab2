using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NYSSLab2;
/// <summary>
/// Interaction logic for SuccessWindow.xaml
/// </summary>
public partial class SuccessWindow : Window
{
    public IEnumerable<Model> OldModels { get; set; }
    public IEnumerable<Model> NewModels { get; set; }

    public int ItemsChanged { get; set; }

    public SuccessWindow()
    {
        InitializeComponent();
    }

    public SuccessWindow(IEnumerable<Model> oldModels, IEnumerable<Model> newModels)
    {
        OldModels = oldModels;
        NewModels = newModels;
        ItemsChanged = OldModels.Count();
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        Close();
    }
}
