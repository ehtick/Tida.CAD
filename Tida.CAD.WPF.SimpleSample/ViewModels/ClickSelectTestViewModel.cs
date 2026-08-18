using System;
using System.Windows;
using System.Windows.Media;
using Tida.CAD;
using Tida.CAD.DrawObjects;

namespace Tida.CAD.WPF.SimpleSample.ViewModels;

/// <summary>
/// 点击选择测试窗口的视图模型;
/// </summary>
public class ClickSelectTestViewModel : BindableBase
{
    public ClickSelectTestViewModel()
    {
        var rectPen = new Pen(Brushes.White, 2);
        rectPen.Freeze();

        Layers[0].AddDrawObject(new Rectangle(new CADRect(new Point(-2, -2), new Size(4, 4)))
        {
            Pen = rectPen,
            Background = Brushes.Orange
        });

        Layers[0].AddDrawObject(new Rectangle(new CADRect(new Point(4, -2), new Size(4, 4)))
        {
            Pen = rectPen,
            Background = Brushes.Orange
        });
    }

    /// <summary>
    /// 绑定至画布控件的图层集合;
    /// </summary>
    public CADLayer[] Layers { get; } = { new CADLayer() };

    /// <summary>
    /// 可选的点击选择模式;
    /// </summary>
    public ClickSelectMode[] ClickSelectModes { get; } = Enum.GetValues<ClickSelectMode>();

    private ClickSelectMode _selectedClickMode;
    /// <summary>
    /// 当前点击选择模式;
    /// </summary>
    public ClickSelectMode SelectedClickMode
    {
        get => _selectedClickMode;
        set => SetProperty(ref _selectedClickMode, value);
    }
}
