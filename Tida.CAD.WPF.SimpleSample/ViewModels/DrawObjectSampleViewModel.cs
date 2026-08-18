using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Tida.CAD;
using Tida.CAD.DrawObjects;

namespace Tida.CAD.WPF.SimpleSample.ViewModels;

/// <summary>
/// 绘制对象示例窗口的视图模型;
/// </summary>
public class DrawObjectSampleViewModel : BindableBase
{
    public DrawObjectSampleViewModel()
    {
        Layer = new CADLayer();
        Layers = new CADLayer[] { Layer };
    }

    /// <summary>
    /// 当前图层;
    /// </summary>
    public CADLayer Layer { get; }

    /// <summary>
    /// 绑定至画布控件的图层集合;
    /// </summary>
    public CADLayer[] Layers { get; }

    private RelayCommand? _addLineCommand;
    public ICommand AddLineCommand => _addLineCommand ??= new RelayCommand(AddLine);
    private void AddLine()
    {
        var line = new Line { Start = new Point(0, 0), End = new Point(10, 10) };
        line.Pen = new Pen { Thickness = 2, Brush = Brushes.White };
        Layer.AddDrawObject(line);
    }

    private RelayCommand? _addRectCommand;
    public ICommand AddRectCommand => _addRectCommand ??= new RelayCommand(AddRect);
    private void AddRect()
    {
        var rectPen = new Pen(Brushes.White, 2);
        var rectBackground = Brushes.Orange;
        rectPen.Freeze();

        var rect = new Rectangle(new CADRect(new Point(-2, -2), new Size(4, 4)))
        {
            Pen = rectPen,
            IsSelected = true,
            Background = rectBackground
        };

        Layer.AddDrawObject(rect);
    }

    private RelayCommand? _clearCommand;
    public ICommand ClearCommand => _clearCommand ??= new RelayCommand(Clear);
    private void Clear()
    {
        Layer.Clear();
    }

    private RelayCommand? _changeLayerBackgroundCommand;
    public ICommand ChangeLayerBackgroundCommand => _changeLayerBackgroundCommand ??= new RelayCommand(ChangeLayerBackground);
    private void ChangeLayerBackground()
    {
        if (Layer.Background == null)
        {
            Layer.Background = Brushes.Blue;
        }
        else
        {
            Layer.Background = null;
        }
    }

    private RelayCommand? _addPolygonCommand;
    public ICommand AddPolygonCommand => _addPolygonCommand ??= new RelayCommand(AddPolygon);
    private void AddPolygon()
    {
        var polygon = new Polygon
        {
            Points = new[]
            {
                new Point(2,0),
                new Point(4,0),
                new Point(6,2),
                new Point(6,4),
                new Point(4,6),
                new Point(2,6),
                new Point(0,4),
                new Point(0,2),
                new Point(2,0)
            },
            Pen = new Pen(Brushes.White,2),
            Brush = null
        };
        Layer.AddDrawObject(polygon);
    }

    private RelayCommand? _addArcCommand;
    public ICommand AddArcCommand => _addArcCommand ??= new RelayCommand(AddArc);
    private void AddArc()
    {
        Layer.AddDrawObject
        (
            new Arc
            {
                Pen = new Pen { Brush = Brushes.White, Thickness = 2 },
                Center = new Point(0, 0),
                Radius = 2,
                BeginAngle = 0,
                Angle = ConvertDegreesToRadians(185.0)
            }
        );
    }

    private static double ConvertDegreesToRadians(double v)
    {
        return v / 180 * Math.PI;
    }

    private RelayCommand? _addTextCommand;
    public ICommand AddTextCommand => _addTextCommand ??= new RelayCommand(AddText);
    private void AddText()
    {
        var text = new Text
        {
            Content = "Hello World",
            Position = new Point(0, 0),
            FontSize = 14
        };
        Layer.AddDrawObject(text);
    }

    private RelayCommand? _addBatchCommand;
    public ICommand AddBatchCommand => _addBatchCommand ??= new RelayCommand(AddBatch);

    /// <summary>
    /// 当前批次索引,每点击一次递增,使各批次图形在横向上隔开显示;
    /// </summary>
    private int _batchIndex;

    /// <summary>
    /// 一次性生成大量绘制对象(200个),与上一批在横向上隔开;
    /// </summary>
    private void AddBatch()
    {
        const int columnCount = 20;
        const int rowCount = 10;
        const double cellSize = 3;
        //每批图形占据一块区域,批次间互不重叠;
        var batchOffsetX = _batchIndex * (columnCount * cellSize);

        var random = new Random();
        var brushes = new[]
        {
            Brushes.White, Brushes.Orange, Brushes.Yellow,
            Brushes.Cyan, Brushes.LimeGreen, Brushes.DeepSkyBlue
        };

        var drawObjects = new List<DrawObject>();

        for (var row = 0; row < rowCount; row++)
        {
            for (var col = 0; col < columnCount; col++)
            {
                var cellOrigin = new Point(batchOffsetX + col * cellSize, row * cellSize);
                var brush = brushes[random.Next(brushes.Length)];
                var pen = new Pen(brush, 0.5);
                pen.Freeze();

                switch ((col + row) % 5)
                {
                    case 0: //直线;
                        drawObjects.Add(new Line
                        {
                            Start = new Point(cellOrigin.X + random.NextDouble() * cellSize, cellOrigin.Y + random.NextDouble() * cellSize),
                            End = new Point(cellOrigin.X + random.NextDouble() * cellSize, cellOrigin.Y + random.NextDouble() * cellSize),
                            Pen = pen
                        });
                        break;
                    case 1: //矩形;
                        drawObjects.Add(new Rectangle(new CADRect(
                            new Point(cellOrigin.X + 0.5, cellOrigin.Y + 0.5),
                            new Size(cellSize - 1, cellSize - 1)))
                        {
                            Pen = pen,
                            Background = brush
                        });
                        break;
                    case 2: //多边形;
                        drawObjects.Add(new Polygon
                        {
                            Points = new[]
                            {
                                new Point(cellOrigin.X + 0.5, cellOrigin.Y + 0.5),
                                new Point(cellOrigin.X + cellSize - 0.5, cellOrigin.Y + 0.5),
                                new Point(cellOrigin.X + cellSize - 0.5, cellOrigin.Y + cellSize - 0.5),
                                new Point(cellOrigin.X + 0.5, cellOrigin.Y + cellSize - 0.5)
                            },
                            Pen = pen,
                            Brush = brush
                        });
                        break;
                    case 3: //圆弧;
                        drawObjects.Add(new Arc
                        {
                            Pen = pen,
                            Center = new Point(cellOrigin.X + cellSize / 2, cellOrigin.Y + cellSize / 2),
                            Radius = cellSize / 2 - 0.3,
                            BeginAngle = 0,
                            Angle = random.NextDouble() * Math.PI * 2
                        });
                        break;
                    default: //文字;
                        drawObjects.Add(new Text
                        {
                            Content = "Hi",
                            Position = cellOrigin,
                            FontSize = 5
                        });
                        break;
                }
            }
        }

        //批量添加,只触发一次图层重录;
        Layer.AddDrawObjects(drawObjects);
        _batchIndex++;
    }
}
