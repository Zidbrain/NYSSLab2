using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

namespace NYSSLab2;

public class Model : INotifyPropertyChanged
{
    private int _id;
    private string _name;
    private string _description;
    private string _source;
    private string _object;
    private bool _confidentialityCompromised;
    private bool _integrityCompromised;
    private bool _accessCompromised;

    public Model() { }

    public Model Copy() => MemberwiseClone() as Model;

    public int ID
    {
        get => _id;
        set
        {
            OnPropertyChanging();
            _id = value;
            OnPropertyChanged();
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            OnPropertyChanging();
            _name = value;
            OnPropertyChanged();
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            OnPropertyChanging();
            _description = value;
            OnPropertyChanged();
        }
    }

    public string Source
    {
        get => _source;
        set
        {
            OnPropertyChanging();
            _source = value;
            OnPropertyChanged();
        }
    }

    public string Object
    {
        get => _object;
        set
        {
            OnPropertyChanging();
            _object = value;
            OnPropertyChanged();
        }
    }

    public bool ConfidentialityCompromised
    {
        get => _confidentialityCompromised;
        set
        {
            OnPropertyChanging();
            _confidentialityCompromised = value;
            OnPropertyChanged();
        }
    }

    public bool IntegrityCompromised
    {
        get => _integrityCompromised;
        set
        {
            OnPropertyChanging();
            _integrityCompromised = value;
            OnPropertyChanged();
        }
    }

    public bool AccessCompromised
    {
        get => _accessCompromised;
        set
        {
            OnPropertyChanging();
            _accessCompromised = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public event PropertyChangingEventHandler PropertyChanging;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected void OnPropertyChanging([CallerMemberName] string propertyName = null) =>
        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
}

public class Models : ObservableCollection<Model>
{
    public Models()
    {
        Add(new Model()
        {
            ID = 1,
            Name = "Dad",
            Description = "abc",
            AccessCompromised = true,
            ConfidentialityCompromised = false,
            IntegrityCompromised = true,
            Object = "Dad",
            Source = "Dsada"
        });
    }
}
