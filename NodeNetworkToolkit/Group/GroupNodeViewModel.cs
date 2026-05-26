using NodeNetwork.ViewModels;

namespace NodeNetwork.Toolkit.Group
{
    /// <summary>
    /// A node that represents one instance of a <see cref="GroupNodeTemplate"/>.
    /// Multiple <see cref="GroupNodeViewModel"/> instances can reference the same template,
    /// sharing the internal subnet while each having independent external connections.
    /// </summary>
    public class GroupNodeViewModel : NodeViewModel
    {
        /// <summary>
        /// The template that defines this group node's internal structure.
        /// </summary>
        public GroupNodeTemplate Template { get; }

        /// <summary>
        /// The IOBinding that maps this instance's inputs/outputs to the shared subnet endpoints.
        /// Set by <see cref="NodeGrouper"/> after construction.
        /// </summary>
        public NodeGroupIOBinding IOBinding { get; internal set; }

        /// <param name="template">The shared template to instantiate.</param>
        public GroupNodeViewModel(GroupNodeTemplate template)
        {
            Template = template;
            Name = template.Name;
            template.RegisterInstance(this);
        }

        /// <summary>
        /// Unregisters this instance from its template.
        /// Call this when permanently removing the node from its parent network.
        /// </summary>
        public void Dispose()
        {
            Template.UnregisterInstance(this);
        }
    }
}
