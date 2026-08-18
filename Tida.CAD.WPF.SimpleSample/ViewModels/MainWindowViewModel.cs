using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace Tida.CAD.WPF.SimpleSample.ViewModels;

/// <summary>
/// 主窗口的视图模型:通过反射加载所有测试命令并生成可执行按钮;
/// </summary>
internal class MainWindowViewModel : BindableBase
{
    public MainWindowViewModel()
    {
        //使用反射获取所有测试命令;
        var types = Assembly.GetExecutingAssembly().GetTypes();
        var testCommandTypes = types.Where(type => type != typeof(ITestCommand) && typeof(ITestCommand).IsAssignableFrom(type));
        var testCommands = testCommandTypes
            .Select(commandType => Activator.CreateInstance(commandType) as ITestCommand)
            .OfType<ITestCommand>()
            .OrderBy(p => p.Order);

        foreach (var testCommand in testCommands)
        {
            TestCommands.Add(new TestCommandItem(testCommand));
        }
    }

    /// <summary>
    /// 测试命令集合;
    /// </summary>
    public ObservableCollection<TestCommandItem> TestCommands { get; } = new();
}

/// <summary>
/// 测试命令的可绑定包装项;
/// </summary>
internal class TestCommandItem
{
    public TestCommandItem(ITestCommand testCommand)
    {
        Name = testCommand.Name;
        Command = new RelayCommand(() => testCommand.Execute(new TestExecuteContext()));
    }

    /// <summary>
    /// 命令显示名称;
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 执行命令;
    /// </summary>
    public ICommand Command { get; }
}
