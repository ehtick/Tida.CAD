<div align="center">

# Tida.CAD

一个面向 .NET 平台的轻量级、可高度扩展的 CAD 画布框架。

![MIT License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blue)
![WPF](https://img.shields.io/badge/UI-WPF-green)
![Avalonia](https://img.shields.io/badge/UI-Avalonia-purple)
![Version 0.65](https://img.shields.io/badge/version-0.65-orange)

[English](README.md) | **简体中文**

</div>

---

## 📖 简介

Tida.CAD 是一个基于 .NET 平台的 CAD 框架，致力于高度可扩展、MVVM 友好和高性能。它提供平台无关的画布核心（图层、绘制对象、坐标转换、输入与事件），并内置 **WPF** 和 **Avalonia** 两套 UI 实现——同一套绘制代码可在这两个 UI 框架上原样运行。

![example](Images/Line.JPG)

![example](Images/Tida.JPG)

---

## ✨ 特性

- **平台无关的核心层**——共享工程中的契约与类型，编译进两套 UI 实现。
- **WPF 与 Avalonia**——两套 UI 框架，公共 API 完全一致。
- **基于图层的画布模型**——`CADLayer` 作为绘制对象容器，支持独立背景色与可见性。
- **内置绘制对象**——`Line`（直线）、`Arc`（圆弧）、`Rectangle`（矩形）、`Polygon`（多边形）、`Text`（文字）。
- **开箱即用的交互**——滚轮缩放、拖拽平移、点击选择（多选/单选）、拖拽框选（任意相交/完全包含）。
- **高性能渲染**——每图层一个 `DrawingVisual`；每个绘制对象的内容以冻结的 `DrawingGroup` 缓存，重绘时仅做指令回放。
- **基于几何的命中测试**——选择操作不依赖视觉树。

---

## 🚀 快速开始

### 环境要求

| 版本 | 运行时 |
|---|---|
| WPF 版 | .NET Framework 4.5+ / .NET Core 3.1+ / .NET 5+（Windows） |
| Avalonia 版 | .NET 8.0 |

### 安装

在解决方案中引用对应项目，或向项目添加生成的 `Tida.CAD.WPF.dll` / `Tida.CAD.Avalonia.dll`。WPF 库已发布为 NuGet 包 **v0.65**。

---

### 🖥️ WPF 最小示例

**XAML** —— 声明 `https://github.com/Tida.CAD` XML 命名空间并放入控件：

```xml
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:tidacad="https://github.com/Tida.CAD"
        Title="Tida.CAD Demo" Height="450" Width="800">
    <Grid>
        <tidacad:CADControl x:Name="cadControl"/>
    </Grid>
</Window>
```

**Code-behind** —— 创建图层、向图层添加绘制对象、并赋给控件：

```csharp
using System.Windows;
using System.Windows.Media;
using Tida.CAD;
using Tida.CAD.DrawObjects;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // 1. 创建图层并赋给控件
        var layer = new CADLayer();
        cadControl.Layers = new[] { layer };

        // 2. 创建绘制对象
        var line = new Line
        {
            Start = new Point(0, 0),
            End = new Point(10, 10),
            Pen = new Pen { Thickness = 2, Brush = Brushes.White }
        };

        var rect = new Rectangle(new CADRect(new Point(-2, -2), new Size(4, 4)))
        {
            Pen = new Pen(Brushes.Orange, 2),
            Background = Brushes.DarkSlateBlue,
            IsSelected = true   // 可预设选中状态
        };

        var text = new Text
        {
            Content = "Hello Tida.CAD",
            Position = new Point(0, 0),
            FontSize = 14
        };

        // 3. 添加到图层
        layer.AddDrawObjects(new DrawObject[] { line, rect, text });

        // 4. 交互默认可用，以下为显式配置
        cadControl.IsMouseWheelingZoomEnabled = true; // 滚轮缩放
        cadControl.IsDragEnabled = true;              // 拖拽平移
        cadControl.ClickSelectMode = ClickSelectMode.Multiple;
    }
}
```

> 💡 **提示：** `Point`、`Size`、`Pen`、`Brush` 为平台类型别名（WPF 下为 `System.Windows.*`，Avalonia 下为 `Avalonia.*`），因此上面的绘制代码在两个平台上**完全相同**。

---

### 🧩 Avalonia 最小示例

**XAML** —— 对 MVVM 友好：将 `Layers` 绑定到 `AvaloniaList<CADLayer>`：

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:tidacad="https://github.com/Tida.CAD"
        x:Class="MyApp.MainWindow">
    <Grid>
        <tidacad:CADControl Layers="{Binding Layers}"/>
    </Grid>
</Window>
```

**ViewModel** —— 与 WPF 完全相同的绘制对象 API：

```csharp
using Avalonia.Collections;
using Avalonia.Media;
using Tida.CAD;
using Tida.CAD.DrawObjects;

public class MainWindowViewModel
{
    public AvaloniaList<CADLayer> Layers { get; } = [];

    public MainWindowViewModel()
    {
        var layer = new CADLayer();
        Layers.Add(layer);

        layer.AddDrawObject(new Line
        {
            Start = new Point(0, 0),
            End = new Point(10, 10),
            Pen = new Pen { Thickness = 2, Brush = Brushes.White }
        });
    }
}
```

---

## 🧠 核心概念

### 图层（Layer）

`CADLayer` 是绘制对象的容器。每个图层可有独立的 `Background` 与可见性（`IsVisible`）。图层按顺序渲染（后面的在上层）；控件通过 `Layers` 属性持有它们，并通过 `ActiveLayer` 表示当前活动图层。

```csharp
var backgroundLayer = new CADLayer { Background = Brushes.DarkSlateBlue };
var drawingLayer   = new CADLayer();
cadControl.Layers = new[] { backgroundLayer, drawingLayer };
```

### 绘制对象（Draw Object）

任何实现 `IDrawable` 的类都可以绘制在图层上。内置绘制对象位于 `Tida.CAD.DrawObjects` 命名空间：`Line`、`Arc`、`Rectangle`、`Polygon`、`Text`。自定义对象只需实现 `IDrawable.Draw(ICanvas canvas)` —— 所有通过 `ICanvas` 的绘制都会自动完成 CAD 坐标到屏幕坐标的转换。

```csharp
public class Cross : IDrawable
{
    public Point Center { get; set; }

    public void Draw(ICanvas canvas)
    {
        var pen = new Pen { Thickness = 1, Brush = Brushes.Yellow };
        canvas.DrawLine(pen, Center, Center + new Vector(1, 0));
        canvas.DrawLine(pen, Center, Center + new Vector(0, 1));
    }
}
```

### 坐标系（Coordinate System）

`ICADScreenConverter`（`cadControl.CADScreenConverter`）负责 **CAD 世界坐标**与**屏幕（视图）坐标**之间的转换。`Zoom` 控制缩放比例，`PanScreenPosition` 是 CAD 原点在屏幕上的位置——两者都是普通依赖属性，可数据绑定或动画化。

```csharp
// 将 CAD 坐标转为屏幕坐标
Point screenPoint = cadControl.CADScreenConverter.ToScreen(new Point(5, 5));

// 缩放到固定比例
cadControl.Zoom = 2.5;
```

---

## 🖱️ 交互

| 交互 | 属性 |
|---|---|
| 滚轮缩放 | `IsMouseWheelingZoomEnabled` |
| 拖拽平移 | `IsDragEnabled` |
| 点击选择 | `ClickSelectMode`（`Multiple` / `Single`） |
| 拖拽框选 | `IsDragSelectEnabled` |
| `Tab` 键在悬停对象间切换 | — |

选择变化可通过 `ClickSelecting` / `ClickSelected` / `ClickUnselecting` / `ClickUnselected` 事件观察，单个对象的变化通过 `DrawObject.IsSelectedChanged` 观察。框架会把鼠标/键盘输入路由到被选中的绘制对象（`DrawObject.OnMouseDown`、`OnMouseMove`、`OnKeyDown`…），编辑手柄正是基于此机制实现。

---

## 📁 项目结构

```
Tida.CAD/                      # 核心：平台无关的契约与类型
├── ICADControl.cs             #   画布控件契约
├── ICADScreenConverter.cs     #   CAD 与屏幕坐标转换
├── CADLayer.cs                #   图层：绘制对象容器
├── DrawObject.cs              #   绘制对象基类
├── DrawObjects/               #   内置对象：直线、圆弧、矩形、多边形、文字
├── Events/                    #   选择、框选、增删事件
├── Input/                     #   平台无关的鼠标/键盘输入包装
└── Extensions/                #   CADControl / 几何辅助扩展

Tida.CAD.WPF/                  # WPF 实现（net45 ~ net8.0-windows）
└── CADControl.cs              #   每图层一个 DrawingVisual，内容以 DrawingGroup 缓存

Tida.CAD.Avalonia/             # Avalonia 11.x 实现（net8.0）
└── CADControl.cs              #   基于 Avalonia 视觉树

Tida.CAD.WPF.SimpleSample/     # WPF 示例程序
Tida.CAD.Avalonia.SimpleSample/# Avalonia 示例程序
SimpleSample/                  # 最小独立示例（WPF）
Documents/zh_CN/               # 中文文档：入门与交互说明
```

---

## 📚 示例与文档

- **WPF 示例**（[Tida.CAD.WPF.SimpleSample](Tida.CAD.WPF.SimpleSample/)）：绘制对象、图层背景、框选、属性面板、UI 元素叠加。
- **Avalonia 示例**（[Tida.CAD.Avalonia.SimpleSample](Tida.CAD.Avalonia.SimpleSample/)）：同样的演示，采用 MVVM 绑定。
- **中文文档**（[Documents/zh_CN](Documents/zh_CN/)）：《Tida.CAD入门文档》《CanvasInteractionHandler使用方法》（`.docx`）。

---

## 🛠️ 构建

```bash
# WPF 版（全部目标框架）
dotnet build Tida.CAD.WPF/Tida.CAD.WPF.csproj

# WPF 示例
dotnet build Tida.CAD.WPF.SimpleSample/Tida.CAD.WPF.SimpleSample.csproj

# Avalonia 版
dotnet build Tida.CAD.Avalonia/Tida.CAD.Avalonia.csproj

# 整个解决方案
dotnet build Tida.CAD.sln
```

WPF 项目目标框架为 `net45;net46;netcoreapp3.1;net5.0-windows;net6.0-windows;net7.0-windows;net8.0-windows`；Avalonia 项目目标框架为 `net8.0`。

---

## 📄 许可证

[MIT](LICENSE) © 2021 JanusTida。可自由使用、修改和分发——包括商业用途。

---

<div align="center">

**如果觉得有用，欢迎 Star！**

</div>
