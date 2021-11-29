using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using Microsoft.Win32;

namespace NYSSLab2;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        if (!File.Exists(@".\database.xlsx"))
        {
            var result = MessageBox.Show("База данных с угрозами безопасности информации не обнаружена.\n" +
                "Желаете загрузить базу данных из официального банка данных угроз ФСТЭК России? (Нет - выбрать из файла)", "Отсутвует база данных", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    return;
                case MessageBoxResult.No:
                    var dialog = new OpenFileDialog() { Filter = "Файл EXCEL (*.xlsx)|*.xlsx" };
                    var dialogResult = dialog.ShowDialog();

                    if (dialogResult ?? false)
                        NYSSLab2.MainWindow.OriginalSourceFile = dialog.FileName;

                        break;
                default:
                    Environment.Exit(0);
                    break;
            }
        }
    }
}
