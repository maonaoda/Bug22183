using R3;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

#nullable disable
namespace MauiCollectionViewContentViewbug
{
    public class CustomObservableCollection<T> : ObservableCollection<T>
    {
        public CustomObservableCollection() { }

        public CustomObservableCollection(IEnumerable<T> collection) : base(collection)
        {
            foreach (var item in collection)
            {
                SubscribeToItemPropertyChanged(item);
            }
        }

        public CustomObservableCollection(List<T> list) : base(list)
        {
            foreach (var item in list)
            {
                SubscribeToItemPropertyChanged(item);
            }
        }

        public CustomObservableCollection(object value)
        {
            CollectionChanged += CustomObservableCollection_CollectionChanged;
        }

        private void CustomObservableCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (T item in e.NewItems)
                {
                    SubscribeToItemPropertyChanged(item);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (T item in e.OldItems)
                {
                    UnsubscribeFromItemPropertyChanged(item);
                }
            }
        }

        private void SubscribeToItemPropertyChanged(T item)
        {
            if (item != null && item is INotifyPropertyChanged notifyPropertyChangedItem)
            {
                notifyPropertyChangedItem.PropertyChanged += Item_PropertyChanged<T>;
            }
        }

        private void UnsubscribeFromItemPropertyChanged(T item)
        {
            if (item != null && item is INotifyPropertyChanged notifyPropertyChangedItem)
            {
                notifyPropertyChangedItem.PropertyChanged -= Item_PropertyChanged<T>;
            }
        }

        private void Item_PropertyChanged<T1>(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShowOptions")
            {
                //OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, sender, null, this.IndexOf((T)sender)));
            }
        }
    }

    public class MainPageViewModel
    {
        public CustomObservableCollection<MainPageDto> ItemSource { get; } = new CustomObservableCollection<MainPageDto>([new MainPageDto()
        {
            GroupId = 0,
        }, new MainPageDto()
        {
            GroupId = 1,
        }]);
    }

    public class MainPageDto : INotifyPropertyChanged
    {
        public int GroupId { get; set; }

        public string False => $"False_{GroupId}";

        public string True => $"True_{GroupId}";

        public BindableReactiveProperty<bool> ShowOptions { get; }

        public CustomObservableCollection<Options> Options { get; }

        public MainPageDto()
        {
            ShowOptions = new BindableReactiveProperty<bool>(false);
            ShowOptions.Subscribe(x => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowOptions))));

            Options = new CustomObservableCollection<Options>();
            for (int i = 0; i < 10; i++)
            {
                Options.Add(new Options
                {
                    Name = $"Options_{i}",
                });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class Options
    {
        public string Name { get; set; }
    }

    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            BindingContext = new MainPageViewModel();
        }
    }

}
