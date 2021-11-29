using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Data.OleDb;
using System.IO;
using System.Net.Http;
using System.Data.Common;
using System.Data;
using System.Collections.ObjectModel;
using System.Text;

namespace NYSSLab2;

public class ModelView : INotifyPropertyChanged, IDisposable
{
    private DbDataAdapter _adapter;
    private DataSet _dataSet;
    private string _connectionString;
    private string _updateText;

    private string _sourceFile;
    public string SourceFile
    {
        get => _sourceFile;
        set
        {
            _sourceFile = value;
            _connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={value};Extended Properties=""Excel 12.0 Xml;HDR=Yes""";
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public bool TryLoad(out Exception exception)
    {
        try
        {
            _adapter = OleDbFactory.Instance.CreateDataAdapter();

            if (SourceFile == null)
                SourceFile = @".\database.xlsx";

            bool result;
            if (!File.Exists(SourceFile))
                result = TryOpenFromInternet(out exception);
            else
                result = TryOpen(out exception);

            if (!result)
                return false;

            var col = _dataSet.Tables[0].Columns;

            var updateCommand = OleDbFactory.Instance.CreateCommand();

            var updateText = new StringBuilder();

            updateText.Append("UPDATE [@sheetName] SET");

            for (int i = 0; i < 8; i++)
            {
                updateText.Append($" [{col[i].ColumnName}]=@p{i}");

                if (i != 7)
                    updateText.Append(',');

                var updParam = updateCommand.CreateParameter();
                updParam.ParameterName = $"@p{i}";
                updParam.SourceColumn = $"{col[i].ColumnName}";

                updateCommand.Parameters.Add(updParam);
            }

            updateText.Append($" WHERE [{col[0].ColumnName}]=@p0");

            _updateText = updateText.ToString();
            _adapter.UpdateCommand = updateCommand;
            _adapter.AcceptChangesDuringUpdate = true;

            ItemsPerPage = 20;
            Page = 1;
        }
        catch (Exception exc)
        {
            exception = exc;
            return false;
        }

        exception = null;
        return true;
    }

    public ModelView(string sourceFile) => SourceFile = sourceFile;

    private Dictionary<int, (Model old, Model replace)> _changedData = new Dictionary<int, (Model, Model)>();

    private static bool BoolRepr(object value) =>
        (double)value == 1.0;

    private static string StringRepr(object value)
    {
        if (value is DBNull)
            return null;
        else return Convert.ToString(value);
    }

    private void FillData()
    {
        AllData = new ObservableCollection<Model>(
            _dataSet.Tables[0].Select().Select(row =>
            {
                var model = new Model()
                {
                    ID = Convert.ToInt32(row[0]),
                    Name = StringRepr(row[1]),
                    Description = StringRepr(row[2]),
                    Source = StringRepr(row[3]),
                    Object = StringRepr(row[4]),
                    ConfidentialityCompromised = BoolRepr(row[5]),
                    IntegrityCompromised = BoolRepr(row[6]),
                    AccessCompromised = BoolRepr(row[7]),
                };

                model.PropertyChanging += (s, e) =>
                {
                    if (!_changedData.ContainsKey((s as Model).ID))
                        _changedData.Add((s as Model).ID, ((s as Model).Copy(), s as Model));
                };

                return model;
            }
            ).Take(1..)
        );

        Page = 1;
    }

    private string GetSheetName(OleDbConnection connection)
    {
        var schema = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
        return schema.Rows[0]["TABLE_NAME"].ToString();
    }

    public bool TryOpenFromInternet(out Exception exception)
    {
        try
        {
            using var client = new HttpClient() { BaseAddress = new Uri(@"https://bdu.fstec.ru/") };
            using var stream = client.GetStreamAsync(@"files/documents/thrlist.xlsx").Result;

            if (File.Exists(SourceFile))
                File.Delete(SourceFile);

            using var write = File.OpenWrite(SourceFile);
            stream.CopyTo(write);
        }
        catch (Exception ex)
        {
            exception = ex;
            return false;
        }

        return TryOpen(out exception);
    }

    public bool TryOpen(out Exception exception)
    {
        try
        {
            using var connection = new OleDbConnection($@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={SourceFile};Extended Properties=""Excel 12.0 Xml;HDR=Yes""");
            connection.Open();

            _adapter.SelectCommand = new OleDbCommand($"SELECT * FROM [{GetSheetName(connection)}]")
            {
                Connection = connection
            };

            _dataSet = new DataSet();
            if (_dataSet != null)
                _dataSet.Dispose();
            _adapter.Fill(_dataSet);

            connection.Close();

            FillData();
        }
        catch (Exception ex)
        {
            exception = ex;
            return false;
        }

        exception = null;
        return true;
    }

    public bool TrySave(string destination, out Exception exception)
    {
        try
        {
            if (File.Exists(destination))
                File.Delete(destination);

            using var connection = new OleDbConnection(_connectionString);
            connection.Open();

            using var con = new OleDbConnection($@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={destination};Extended Properties=""Excel 12.0 Xml;HDR=Yes""");
            con.Open();

            var co = new OleDbCommand($"CREATE TABLE [#Sheet]", con);
            co.ExecuteNonQuery();

            var command = new OleDbCommand($"SELECT * INTO [#Sheet] IN '{destination}' [Excel 12.0 Xml;HDR=YES] FROM [{GetSheetName(connection)}]", connection);
            command.ExecuteNonQuery();

            connection.Close();
        }
        catch (Exception ex)
        {
            exception = ex;
            return false;
        }

        exception = null;
        return true;
    }

    public bool TryUpdate(out IEnumerable<Model> oldModels, out IEnumerable<Model> newModels, out Exception exception)
    {
        try
        {
            foreach (var change in _changedData)
            {
                foreach (DataRow row in _dataSet.Tables[0].Rows)
                {
                    if (row[0].GetType() == typeof(double) && Convert.ToInt32(row[0]) == change.Key)
                    {
                        row.SetField(0, Convert.ToDouble(change.Value.replace.ID));
                        row.SetField(1, change.Value.replace.Name);
                        row.SetField(2, change.Value.replace.Description);
                        row.SetField(3, change.Value.replace.Source);
                        row.SetField(4, change.Value.replace.Object);
                        row.SetField(5, change.Value.replace.ConfidentialityCompromised);
                        row.SetField(6, change.Value.replace.IntegrityCompromised);
                        row.SetField(7, change.Value.replace.AccessCompromised);
                    }
                }
            }

            using var connection = new OleDbConnection(_connectionString);
            connection.Open();
            _adapter.UpdateCommand.CommandText = _updateText.Replace("@sheetName", GetSheetName(connection));
            _adapter.UpdateCommand.Connection = connection;
            _adapter.Update(_dataSet);

            oldModels = _changedData.Values.Select(s => s.old).ToList();
            newModels = _changedData.Values.Select(s => s.replace).ToList();

            _changedData.Clear();

            connection.Close();
        }
        catch (Exception ex)
        {
            oldModels = null;
            newModels = null;
            exception = ex;

            return false;
        }

        exception = null;
        return true;
    }

    private void SetPageData(int page)
    {
        var newPage = new ObservableCollection<Model>();
        for (int i = page * ItemsPerPage; i < (page + 1) * ItemsPerPage && i < AllData.Count; i++)
            newPage.Add(AllData[i]);
        PageData = newPage;
    }

    private ObservableCollection<Model> _allData;
    public ObservableCollection<Model> AllData
    {
        get => _allData;
        set
        {
            _allData = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<Model> _pagesData;
    public ObservableCollection<Model> PageData
    {
        get => _pagesData;
        set
        {
            _pagesData = value;
            OnPropertyChanged();
        }
    }

    private int _itemsPerPage;
    public int ItemsPerPage
    {
        get => _itemsPerPage;
        set
        {
            _itemsPerPage = value;

            PagesCount = AllData.Count / _itemsPerPage;

            OnPropertyChanged();
        }
    }

    private int _page;
    public int Page
    {
        get => _page;
        set
        {
            _page = value;

            SetPageData(_page - 1);

            OnPropertyChanged();
        }
    }

    public bool HasUnsavedChanges => _changedData.Count > 0;

    private int _pagesCount;
    private bool disposedValue;

    public int PagesCount
    {
        get => _pagesCount;
        set
        {
            _pagesCount = value;
            OnPropertyChanged();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _adapter.UpdateCommand.Dispose();
                _adapter.SelectCommand.Dispose();
                _adapter.Dispose();
                _dataSet.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
