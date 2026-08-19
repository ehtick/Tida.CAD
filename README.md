<div align="center">

# Tida.CAD

A lightweight, extensible CAD canvas framework for the .NET platform.

![MIT License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blue)
![WPF](https://img.shields.io/badge/UI-WPF-green)
![Avalonia](https://img.shields.io/badge/UI-Avalonia-purple)
![Version 0.65](https://img.shields.io/badge/version-0.65-orange)

**English** | [简体中文](README.zh-CN.md)

</div>

---

## 📖 Introduction

Tida.CAD is a CAD framework built on the .NET platform, targeting high extensibility, MVVM friendliness and high performance. It provides a platform-independent canvas core (layers, draw objects, coordinate conversion, input and events) with ready-made UI implementations for **WPF** and **Avalonia** — so the same drawing code runs on both frameworks unchanged.

![example](Images/Line.JPG)

![example](Images/Tida.JPG)

---

## ✨ Features

- **Platform-independent core** — a shared project with contracts and types that compile into both UI implementations.
- **WPF & Avalonia** — identical public API across the two UI frameworks.
- **Layer-based canvas model** — `CADLayer` as the container of draw objects, with per-layer background and visibility.
- **Built-in draw objects** — `Line`, `Arc`, `Rectangle`, `Polygon`, `Text`.
- **Out-of-the-box interactions** — mouse wheel zoom, drag-to-pan, click selection (`Multiple` / `Single`), drag-box selection (any-point & all-point).
- **High-performance rendering** — one `DrawingVisual` per layer; per-draw-object content is cached as frozen `DrawingGroup`s and replayed on repaint.
- **Geometry-based hit-testing** — selection does not depend on the visual tree.

---

## 🚀 Getting Started

### Prerequisites

| Runtime | Framework |
|---|---|
| WPF version | .NET Framework 4.5+ / .NET Core 3.1+ / .NET 5+ (Windows) |
| Avalonia version | .NET 8.0 |

### Installation

Reference the projects in the solution, or add the built `Tida.CAD.WPF.dll` / `Tida.CAD.Avalonia.dll` assemblies to your project. The WPF library is published as NuGet package **v0.65**.

---

### 🖥️ WPF — Minimum Example

**XAML** — declare the `https://github.com/Tida.CAD` XML namespace and drop the control in:

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

**Code-behind** — create a layer, add draw objects to it, and assign it to the control:

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

        // 1. Create a layer and bind it to the control
        var layer = new CADLayer();
        cadControl.Layers = new[] { layer };

        // 2. Create draw objects
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
            IsSelected = true   // draw objects can be pre-selected
        };

        var text = new Text
        {
            Content = "Hello Tida.CAD",
            Position = new Point(0, 0),
            FontSize = 14
        };

        // 3. Add them to the layer
        layer.AddDrawObjects(new DrawObject[] { line, rect, text });

        // 4. Interaction is enabled by default
        cadControl.IsMouseWheelingZoomEnabled = true; // zoom with mouse wheel
        cadControl.IsDragEnabled = true;              // pan by dragging
        cadControl.ClickSelectMode = ClickSelectMode.Multiple;
    }
}
```

> 💡 **Note:** `Point`, `Size`, `Pen`, `Brush` are aliased to the platform types (`System.Windows.*` on WPF, `Avalonia.*` on Avalonia), so the drawing code above is **identical** on both platforms.

---

### 🧩 Avalonia — Minimum Example

**XAML** — MVVM-friendly: bind `Layers` to an `AvaloniaList<CADLayer>`:

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

**ViewModel** — the same draw-object API as WPF:

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

## 🧠 Core Concepts

### Layer

A `CADLayer` is the container of draw objects. Each layer can have its own `Background` and visibility (`IsVisible`). Layers are rendered in order (later layers on top); the control holds them via the `Layers` property, and the active layer via `ActiveLayer`.

```csharp
var backgroundLayer = new CADLayer { Background = Brushes.DarkSlateBlue };
var drawingLayer   = new CADLayer();
cadControl.Layers = new[] { backgroundLayer, drawingLayer };
```

### Draw Object

Any class implementing `IDrawable` can be drawn on a layer. Built-in draw objects live in the `Tida.CAD.DrawObjects` namespace: `Line`, `Arc`, `Rectangle`, `Polygon`, `Text`. To create your own, implement `IDrawable.Draw(ICanvas canvas)` — everything drawn through `ICanvas` is automatically converted from CAD coordinates to screen coordinates.

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

### Coordinate System

`ICADScreenConverter` (`cadControl.CADScreenConverter`) converts between **CAD world coordinates** and **screen (view) coordinates**. `Zoom` controls the scale, `PanScreenPosition` is the screen position of the CAD origin — both are regular `DependencyProperty`s, so they can be data-bound or animated.

```csharp
// Convert a CAD point to a screen point
Point screenPoint = cadControl.CADScreenConverter.ToScreen(new Point(5, 5));

// Zoom to a fixed scale
cadControl.Zoom = 2.5;
```

---

## 🖱️ Interactions

| Interaction | Property |
|---|---|
| Mouse wheel zoom | `IsMouseWheelingZoomEnabled` |
| Drag to pan | `IsDragEnabled` |
| Click to select | `ClickSelectMode` (`Multiple` / `Single`) |
| Drag-box select | `IsDragSelectEnabled` |
| `Tab` cycles hovered objects | — |

Selection changes are observable via `ClickSelecting` / `ClickSelected` / `ClickUnselecting` / `ClickUnselected` events, and per-object via `DrawObject.IsSelectedChanged`. Every selected object can be interacted with — the framework routes mouse/keyboard input to the selected draw objects (`DrawObject.OnMouseDown`, `OnMouseMove`, `OnKeyDown`...), which is how editing handles are implemented.

---

## 📁 Project Structure

```
Tida.CAD/                      # Core: platform-independent contracts & types
├── ICADControl.cs             #   Canvas control contract
├── ICADScreenConverter.cs     #   CAD <-> screen coordinate conversion
├── CADLayer.cs                #   Layer: container of draw objects
├── DrawObject.cs              #   Base class of draw objects
├── DrawObjects/               #   Built-in objects: Line, Arc, Rectangle, Polygon, Text
├── Events/                    #   Selection, drag-select, add/remove events
├── Input/                     #   Platform-independent mouse/keyboard input wrappers
└── Extensions/                #   CADControl / geometry helper extensions

Tida.CAD.WPF/                  # WPF implementation (net45 ~ net8.0-windows)
└── CADControl.cs              #   One DrawingVisual per layer, DrawingGroup-cached content

Tida.CAD.Avalonia/             # Avalonia 11.x implementation (net8.0)
└── CADControl.cs              #   Avalonia visual-tree based canvas

Tida.CAD.WPF.SimpleSample/     # WPF sample app
Tida.CAD.Avalonia.SimpleSample/# Avalonia sample app
SimpleSample/                  # Minimal standalone sample (WPF)
Documents/zh_CN/               # Chinese docs: getting-started & interaction guide
```

---

## 📚 Samples & Docs

- **WPF sample** ([Tida.CAD.WPF.SimpleSample](Tida.CAD.WPF.SimpleSample/)): draw objects, layer background, drag-select, properties panel, UI-element overlay.
- **Avalonia sample** ([Tida.CAD.Avalonia.SimpleSample](Tida.CAD.Avalonia.SimpleSample/)): the same demos with MVVM bindings.
- **Chinese docs** ([Documents/zh_CN](Documents/zh_CN/)): 《Tida.CAD入门文档》《CanvasInteractionHandler使用方法》(`.docx`).

---

## 🛠️ Build

```bash
# WPF version (all target frameworks)
dotnet build Tida.CAD.WPF/Tida.CAD.WPF.csproj

# WPF sample
dotnet build Tida.CAD.WPF.SimpleSample/Tida.CAD.WPF.SimpleSample.csproj

# Avalonia version
dotnet build Tida.CAD.Avalonia/Tida.CAD.Avalonia.csproj

# Whole solution
dotnet build Tida.CAD.slnx
```

The WPF project targets `net45;net46;netcoreapp3.1;net5.0-windows;net6.0-windows;net7.0-windows;net8.0-windows`; the Avalonia project targets `net8.0`.

---

## 📄 License

[MIT](LICENSE) © 2021 JanusTida. Free to use, modify and distribute — commercial use included.

---

<div align="center">

**⭐ Star this repo if you find it useful!**

</div>
