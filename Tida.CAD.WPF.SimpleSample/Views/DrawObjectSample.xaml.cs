using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Tida.CAD.DrawObjects;

namespace Tida.CAD.WPF.SimpleSample.Views;

/// <summary>
/// Interaction logic for DrawObjectSample.xaml
/// </summary>
public partial class DrawObjectSample : Window
{
    public DrawObjectSample()
    {
        InitializeComponent();
        _cadLayer = new CADLayer();
        cadControl.Layers = new CADLayer[] { _cadLayer };   
    }

    private readonly CADLayer _cadLayer;
    private void Addline_Click(object sender, RoutedEventArgs e)
    {
        var line = new Line { Start = new Point(0, 0), End = new Point(10, 10) };
        line.Pen = new Pen { Thickness = 2, Brush = Brushes.White };
        _cadLayer.AddDrawObject(line);
    }

    private void AddRect_Click(object sender, RoutedEventArgs e)
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

        _cadLayer.AddDrawObject(rect);
    }


    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        _cadLayer.Clear();
    }

    private void ChangeLayerBackground_Click(object sender, RoutedEventArgs e)
    {
        if(_cadLayer.Background == null)
        {
            _cadLayer.Background = Brushes.Blue;
        }
        else
        {
            _cadLayer.Background = null;
        }
    }

    private void AddPolygon_Click(object sender, RoutedEventArgs e)
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
        _cadLayer.AddDrawObject(polygon);
    }

    private void AddArc_Click(object sender, RoutedEventArgs e)
    {
        _cadLayer.AddDrawObject
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

    private void AddText_Click(object sender, RoutedEventArgs e)
    {
        var text = new Text
        {
            Content = "Hello World",
            Position = new Point(0, 0),
            FontSize = 14
        };
        _cadLayer.AddDrawObject(text);
    }

    /// <summary>
    /// 当前批次索引,每点击一次递增,使各批次图形在横向上隔开显示;
    /// </summary>
    private int _batchIndex;

    /// <summary>
    /// 一次性生成大量绘制对象(200个),与上一批在横向上隔开;
    /// </summary>
    private void AddBatch_Click(object sender, RoutedEventArgs e)
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
        _cadLayer.AddDrawObjects(drawObjects);
        _batchIndex++;
    }
}
