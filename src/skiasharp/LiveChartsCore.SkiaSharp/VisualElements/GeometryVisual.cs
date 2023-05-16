﻿// The MIT License(MIT)
//
// Copyright(c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.VisualElements;

namespace LiveChartsCore.SkiaSharpView.VisualElements;

/// <summary>
/// Defines a visual element in a chart that draws a rectangle geometry in the user interface.
/// </summary>
public class GeometryVisual<TGeometry> : BaseGeometryVisual
    where TGeometry : ISizedGeometry<SkiaSharpDrawingContext>, new()
{
    internal TGeometry? _geometry;
    private LvcSize _actualSize = new();
    private LvcPoint _targetLocation = new();

    /// <summary>
    /// Occurs when the geometry is initialized.
    /// </summary>
    public event Action<TGeometry>? GeometryInitialized;

    /// <inheritdoc cref="VisualElement{TDrawingContext}.OnInvalidated(Chart{TDrawingContext}, Scaler, Scaler)"/>
    protected internal override void OnInvalidated(Chart<SkiaSharpDrawingContext> chart, Scaler? primaryScaler, Scaler? secondaryScaler)
    {
        var x = (float)X;
        var y = (float)Y;

        if (LocationUnit == MeasureUnit.ChartValues)
        {
            if (primaryScaler is null || secondaryScaler is null)
                throw new Exception($"You can not use {MeasureUnit.ChartValues} scale at this element.");

            x = secondaryScaler.ToPixels(x);
            y = primaryScaler.ToPixels(y);
        }

        _targetLocation = new((float)X, (float)Y);
        _ = Measure(chart, primaryScaler, secondaryScaler);

        if (_geometry is null)
        {
            _geometry = new TGeometry
            {
                X = (float)X,
                Y = (float)Y,
                Width = _actualSize.Width,
                Height = _actualSize.Height
            };
            GeometryInitialized?.Invoke(_geometry);

            _geometry.Animate(chart);
        }

        _geometry.X = x;
        _geometry.Y = y;
        _geometry.Width = _actualSize.Width;
        _geometry.Height = _actualSize.Height;

        var drawing = chart.Canvas.Draw();
        if (Fill is not null) _ = drawing.SelectPaint(Fill).Draw(_geometry);
        if (Stroke is not null) _ = drawing.SelectPaint(Stroke).Draw(_geometry);
    }

    /// <inheritdoc cref="VisualElement{TDrawingContext}.SetParent(IGeometry{TDrawingContext})"/>
    protected internal override void SetParent(IGeometry<SkiaSharpDrawingContext> parent)
    {
        if (_geometry is null) return;
        _geometry.Parent = parent;
    }

    /// <inheritdoc cref="VisualElement{TDrawingContext}.Measure(Chart{TDrawingContext}, Scaler, Scaler)"/>
    public override LvcSize Measure(Chart<SkiaSharpDrawingContext> chart, Scaler? primaryScaler, Scaler? secondaryScaler)
    {
        var w = (float)Width;
        var h = (float)Height;

        if (SizeUnit == MeasureUnit.ChartValues)
        {
            if (primaryScaler is null || secondaryScaler is null)
                throw new Exception($"You can not use {MeasureUnit.ChartValues} scale at this element.");

            w = secondaryScaler.MeasureInPixels(w);
            h = primaryScaler.MeasureInPixels(h);
        }

        return _actualSize = new LvcSize(w, h);
    }

    /// <inheritdoc cref="VisualElement{TDrawingContext}.GetTargetSize"/>
    public override LvcSize GetTargetSize()
    {
        return _actualSize;
    }

    /// <inheritdoc cref="VisualElement{TDrawingContext}.GetTargetLocation"/>
    public override LvcPoint GetTargetLocation()
    {
        return _targetLocation;
    }
}
