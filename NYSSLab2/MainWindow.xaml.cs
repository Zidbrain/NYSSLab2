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
using Microsoft.Win32;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NYSSLab2;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public ModelView ModelView { get; set; }

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(Model), typeof(MainWindow));

    public Model SelectedItem
    {
        get => (Model)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public static string OriginalSourceFile { get; set; }

    public MainWindow()
    {
        ModelView = new ModelView(OriginalSourceFile);

        if (!ModelView.TryLoad(out var exception))
        {
            MessageBox.Show($"Произошла ошибка: {exception.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
        }

        InitializeComponent();
    }

    private void CheckButtonsEnabled()
    {
        pageLeftButton.IsEnabled = ModelView.Page != 1;
        pageRightButton.IsEnabled = ModelView.Page != ModelView.PagesCount;
    }

    private void pageLeftButton_Click(object sender, RoutedEventArgs e)
    {
        ModelView.Page--;
        CheckButtonsEnabled();
    }

    private void pageRightButton_Click(object sender, RoutedEventArgs e)
    {
        ModelView.Page++;
        CheckButtonsEnabled();
    }

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source is TabControl)
        {
            var prevStart = (ModelView.Page - 1) * ModelView.ItemsPerPage;

            if (e.AddedItems[0] == minimalView)
                ModelView.ItemsPerPage = 50;
            else if (e.AddedItems[0] == fullView)
                ModelView.ItemsPerPage = 20;

            ModelView.Page = prevStart / ModelView.ItemsPerPage + 1;

            CheckButtonsEnabled();
        }
    }

    public static RoutedCommand OpenFromInternet = new RoutedCommand();

    private void OpenFileModel(object sender, ExecutedRoutedEventArgs e)
    {
        var dialog = new OpenFileDialog() { Filter = "Файл EXCEL (*.xlsx)|*.xlsx" };
        if (dialog.ShowDialog() ?? false)
        {
            ModelView.SourceFile = dialog.FileName;
            if (!ModelView.TryOpen(out var exception))
                MessageBox.Show($"Произошла ошибка: {exception.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenFromInterntModel(object sender, ExecutedRoutedEventArgs e)
    {
        if (!ModelView.TryOpenFromInternet(out var exception))
            MessageBox.Show($"Произошла ошибка: {exception.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void SaveAsModel(object sender, ExecutedRoutedEventArgs e)
    {
        var dialog = new SaveFileDialog() { Filter = "Файл EXCEL (*.xlsx)|*.xlsx" };
        if (dialog.ShowDialog() ?? false)
        {
            if (!ModelView.TrySave(dialog.FileName, out var exception))
                MessageBox.Show($"Произошла ошибка: {exception.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                ModelView.SourceFile = dialog.FileName;
                MessageBox.Show("Сохранение прошло успешно!");
            }
        }
    }

    private void UpdateModel(object sender, ExecutedRoutedEventArgs e)
    {
        if (!ModelView.TryUpdate(out var oldModels, out var newModels, out var exception))
            MessageBox.Show($"Произошла ошибка: {exception.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        else
        {
            var window = new SuccessWindow(oldModels, newModels);
            window.ShowDialog();
        }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        ModelView.Dispose();

        Application.Current.Shutdown();
    }

    private void CloseWindow(object sender, ExecutedRoutedEventArgs e)
    {
        Close();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (ModelView.HasUnsavedChanges)
        {
            var result = MessageBox.Show("Имеются несохранённые изменения.\nСохранить?", "", MessageBoxButton.YesNoCancel);

            if (result == MessageBoxResult.Yes)
                UpdateModel(this, null);
            else if (result == MessageBoxResult.Cancel)
                e.Cancel = true;
        }
    }
}
