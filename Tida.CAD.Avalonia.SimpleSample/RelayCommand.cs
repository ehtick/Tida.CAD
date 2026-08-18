using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Tida.CAD.Avalonia.SimpleSample;

internal class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute,Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }
    
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// 通知命令绑定源刷新 <see cref="CanExecute"/> 状态;
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool CanExecute(object? parameter)
    {
        if (_canExecute == null)
        {
            return true;
        }
        return _canExecute();
    }

    public void Execute(object? parameter)
    {
        _execute();
    }
}
