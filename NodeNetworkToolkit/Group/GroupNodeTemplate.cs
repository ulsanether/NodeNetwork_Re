using System.Collections.Generic;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace NodeNetwork.Toolkit.Group
{
    /// <summary>
    /// A reusable group template that can be instantiated multiple times across one or more networks.
    /// Inspired by Blender's Node Group concept: the internal subnet is shared by all instances,
    /// so editing the group in one place propagates the structural changes to every instance.
    /// </summary>
    public class GroupNodeTemplate : ReactiveObject
    {
        #region Name
        /// <summary>
        /// Display name of this template (shown on each group node instance).
        /// </summary>
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
        private string _name;
        #endregion

        /// <summary>
        /// The shared subnet that defines the internal logic of this group template.
        /// All instances reference this same network.
        /// </summary>
        public NetworkViewModel SubNetwork { get; }

        /// <summary>
        /// The entrance node inside <see cref="SubNetwork"/>.
        /// Its outputs represent the data entering the group (correspond to each instance's inputs).
        /// </summary>
        public NodeViewModel EntranceNode { get; }

        /// <summary>
        /// The exit node inside <see cref="SubNetwork"/>.
        /// Its inputs receive the data leaving the group (correspond to each instance's outputs).
        /// </summary>
        public NodeViewModel ExitNode { get; }

        private readonly List<GroupNodeViewModel> _instances = new List<GroupNodeViewModel>();

        /// <summary>
        /// All currently registered instances of this template.
        /// </summary>
        public IReadOnlyList<GroupNodeViewModel> Instances => _instances;

        /// <param name="subNetwork">The shared subnet.</param>
        /// <param name="entranceNode">Entrance node already added to <paramref name="subNetwork"/>.</param>
        /// <param name="exitNode">Exit node already added to <paramref name="subNetwork"/>.</param>
        public GroupNodeTemplate(NetworkViewModel subNetwork, NodeViewModel entranceNode, NodeViewModel exitNode)
        {
            SubNetwork = subNetwork;
            EntranceNode = entranceNode;
            ExitNode = exitNode;
            Name = "Group";
        }

        /// <summary>
        /// Registers a new instance of this template.
        /// Called automatically by <see cref="GroupNodeViewModel"/>.
        /// </summary>
        internal void RegisterInstance(GroupNodeViewModel instance)
        {
            _instances.Add(instance);
        }

        /// <summary>
        /// Removes a previously registered instance.
        /// Call this when a <see cref="GroupNodeViewModel"/> is removed from its parent network.
        /// </summary>
        internal void UnregisterInstance(GroupNodeViewModel instance)
        {
            _instances.Remove(instance);
        }
    }
}
