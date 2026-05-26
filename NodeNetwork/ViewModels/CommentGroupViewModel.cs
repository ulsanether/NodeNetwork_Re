using System.Windows;
using System.Windows.Media;
using NodeNetwork.Views;
using ReactiveUI;
using Splat;

namespace NodeNetwork.ViewModels
{
    /// <summary>
    /// ViewModel for an inline comment box that visually groups nodes on the canvas
    /// without affecting data flow or creating sub-networks.
    /// Similar to Unreal Engine Blueprint comment nodes.
    /// </summary>
    public class CommentGroupViewModel : ReactiveObject
    {
        static CommentGroupViewModel()
        {
            NNViewRegistrar.AddRegistration(() => new CommentGroupView(), typeof(IViewFor<CommentGroupViewModel>));
        }

        #region Name
        /// <summary>
        /// The label displayed in the header of the comment box.
        /// </summary>
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
        private string _name = "Comment";
        #endregion

        #region Color
        /// <summary>
        /// The accent color used for the border and header of the comment box.
        /// </summary>
        public Color Color
        {
            get => _color;
            set => this.RaiseAndSetIfChanged(ref _color, value);
        }
        private Color _color = Color.FromArgb(180, 80, 140, 200);
        #endregion

        #region Position
        /// <summary>
        /// The top-left position of the comment box on the network canvas.
        /// </summary>
        public Point Position
        {
            get => _position;
            set => this.RaiseAndSetIfChanged(ref _position, value);
        }
        private Point _position;
        #endregion

        #region Width
        /// <summary>
        /// The width of the comment box.
        /// </summary>
        public double Width
        {
            get => _width;
            set => this.RaiseAndSetIfChanged(ref _width, value < MinWidth ? MinWidth : value);
        }
        private double _width = 300;

        public const double MinWidth = 80;
        #endregion

        #region Height
        /// <summary>
        /// The height of the comment box.
        /// </summary>
        public double Height
        {
            get => _height;
            set => this.RaiseAndSetIfChanged(ref _height, value < MinHeight ? MinHeight : value);
        }
        private double _height = 200;

        public const double MinHeight = 60;
        #endregion

        #region IsSelected
        /// <summary>
        /// If true, this comment box is currently selected in the UI.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }
        private bool _isSelected;
        #endregion

        #region CanBeRemovedByUser
        /// <summary>
        /// If true, the user can delete this comment box. True by default.
        /// </summary>
        public bool CanBeRemovedByUser
        {
            get => _canBeRemovedByUser;
            set => this.RaiseAndSetIfChanged(ref _canBeRemovedByUser, value);
        }
        private bool _canBeRemovedByUser = true;
        #endregion

        #region Parent
        /// <summary>
        /// The network that contains this comment box.
        /// </summary>
        public NetworkViewModel Parent
        {
            get => _parent;
            internal set => this.RaiseAndSetIfChanged(ref _parent, value);
        }
        private NetworkViewModel _parent;
        #endregion
    }
}
