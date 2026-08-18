using System.Windows;
using System.Windows.Media;
using Tida.CAD;
using Tida.CAD.DrawObjects;

namespace Tida.CAD.WPF.SimpleSample.ViewModels;

/// <summary>
/// 属性测试窗口的视图模型;
/// </summary>
public class PropertiesTestViewModel : BindableBase
{
    public PropertiesTestViewModel()
    {
        var line = new Line { Start = new Point(0, 0), End = new Point(10, 10) };
        line.Pen = new Pen { Thickness = 2, Brush = Brushes.White };
        Layers[0].AddDrawObject(line);
    }

    /// <summary>
    /// 绑定至画布控件的图层集合;
    /// </summary>
    public CADLayer[] Layers { get; } = { new CADLayer() };
}
