namespace QuanLyKho.Services;

public interface INavigationService
{
    object CurrentView { get; }
    void NavigateTo<T>() where T : class;
    void NavigateTo(Type viewModelType);
    event Action? CurrentViewChanged;
}
