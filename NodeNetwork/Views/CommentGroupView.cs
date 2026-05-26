using System;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace NodeNetwork.Views
{
    /// <summary>
    /// A visual overlay control that draws a labeled, colored bounding box around a group of nodes
    /// on the network canvas. This is a purely visual grouping and does not affect data flow.
    /// Similar to comment nodes in Unreal Engine Blueprint.
    /// </summary>
    [TemplatePart(Name = nameof(NameLabel), Type = typeof(TextBlock))]
    [TemplatePart(Name = nameof(ResizeHorizontalThumb), Type = typeof(Thumb))]
    [TemplatePart(Name = nameof(ResizeVerticalThumb), Type = typeof(Thumb))]
    [TemplatePart(Name = nameof(ResizeDiagonalThumb), Type = typeof(Thumb))]
    public class CommentGroupView : Control, IViewFor<CommentGroupViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(CommentGroupViewModel), typeof(CommentGroupView), new PropertyMetadata(null));

        public CommentGroupViewModel ViewModel
        {
            get => (CommentGroupViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (CommentGroupViewModel)value;
        }
        #endregion

        #region AccentBrush
        public static readonly DependencyProperty AccentBrushProperty = DependencyProperty.Register(
            nameof(AccentBrush), typeof(Brush), typeof(CommentGroupView), new PropertyMetadata(Brushes.CornflowerBlue));

        /// <summary>
        /// The brush used for the header background and border of the comment box.
        /// Derived from the ViewModel's Color property.
        /// </summary>
        public Brush AccentBrush
        {
            get => (Brush)GetValue(AccentBrushProperty);
            set => SetValue(AccentBrushProperty, value);
        }
        #endregion

        #region FillBrush
        public static readonly DependencyProperty FillBrushProperty = DependencyProperty.Register(
            nameof(FillBrush), typeof(Brush), typeof(CommentGroupView), new PropertyMetadata(null));

        /// <summary>
        /// The semi-transparent fill brush of the comment box body.
        /// Derived from the ViewModel's Color property.
        /// </summary>
        public Brush FillBrush
        {
            get => (Brush)GetValue(FillBrushProperty);
            set => SetValue(FillBrushProperty, value);
        }
        #endregion

        private TextBlock NameLabel { get; set; }
        private Thumb ResizeHorizontalThumb { get; set; }
        private Thumb ResizeVerticalThumb { get; set; }
        private Thumb ResizeDiagonalThumb { get; set; }

        public CommentGroupView()
        {
            DefaultStyleKey = typeof(CommentGroupView);
            SetupBindings();
            SetupEvents();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            NameLabel = GetTemplateChild(nameof(NameLabel)) as TextBlock;
            ResizeHorizontalThumb = GetTemplateChild(nameof(ResizeHorizontalThumb)) as Thumb;
            ResizeVerticalThumb = GetTemplateChild(nameof(ResizeVerticalThumb)) as Thumb;
            ResizeDiagonalThumb = GetTemplateChild(nameof(ResizeDiagonalThumb)) as Thumb;

            if (ResizeHorizontalThumb != null)
                ResizeHorizontalThumb.DragDelta += (s, e) => ApplyResize(e, true, false);
            if (ResizeVerticalThumb != null)
                ResizeVerticalThumb.DragDelta += (s, e) => ApplyResize(e, false, true);
            if (ResizeDiagonalThumb != null)
                ResizeDiagonalThumb.DragDelta += (s, e) => ApplyResize(e, true, true);
        }

        private void ApplyResize(DragDeltaEventArgs e, bool horizontal, bool vertical)
        {
            if (ViewModel == null) return;
            if (horizontal)
                ViewModel.Width += e.HorizontalChange;
            if (vertical)
                ViewModel.Height += e.VerticalChange;
        }

        private void SetupBindings()
        {
            this.WhenActivated(d =>
            {
                // Sync Width and Height from ViewModel
                this.WhenAnyValue(v => v.ViewModel.Width)
                    .Subscribe(w => Width = w)
                    .DisposeWith(d);
                this.WhenAnyValue(v => v.ViewModel.Height)
                    .Subscribe(h => Height = h)
                    .DisposeWith(d);

                // Build brushes from the ViewModel Color
                this.WhenAnyValue(v => v.ViewModel.Color)
                    .Subscribe(color =>
                    {
                        var accent = new SolidColorBrush(color);
                        accent.Freeze();
                        AccentBrush = accent;

                        var fill = new SolidColorBrush(Color.FromArgb(40, color.R, color.G, color.B));
                        fill.Freeze();
                        FillBrush = fill;
                    })
                    .DisposeWith(d);
            });
        }

        private void SetupEvents()
        {
            MouseLeftButtonDown += (sender, e) =>
            {
                if (ViewModel == null) return;

                if (!ViewModel.IsSelected)
                {
                    // Clear other comment group selections if Ctrl is not held
                    if (ViewModel.Parent != null && !Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        foreach (var cg in ViewModel.Parent.CommentGroups.Items)
                        {
                            cg.IsSelected = false;
                        }
                        ViewModel.Parent.ClearSelection();
                    }
                    ViewModel.IsSelected = true;
                }
            };
        }
    }
}
