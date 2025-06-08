using System.ComponentModel;

namespace GMap.NET.ObjectModel;

public interface INotifyPropertyChanged
{
	event PropertyChangedEventHandler PropertyChanged;
}
