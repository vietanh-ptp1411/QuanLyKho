using Microsoft.Extensions.DependencyInjection;

namespace QuanLyKho.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private object _currentView = null!;

    public object CurrentView
    {
        get => _currentView;
        private set
        {
            _currentView = value;
            CurrentViewChanged?.Invoke();
        }
    }

    public event Action? CurrentViewChanged;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateTo<T>() where T : class
    {
        CurrentView = _serviceProvider.GetRequiredService<T>();
    }

    public void NavigateTo(Type viewModelType)
    {
        CurrentView = _serviceProvider.GetRequiredService(viewModelType);
    }
}
