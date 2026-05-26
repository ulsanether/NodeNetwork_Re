using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace NodeNetwork.Toolkit.Group
{
    /// <summary>
    /// An IOBinding for <see cref="GroupNodeViewModel"/> instances that are based on a
    /// <see cref="GroupNodeTemplate"/>.
    ///
    /// Unlike <see cref="ValueNodeGroupIOBinding"/> (where the group node drives the subnet),
    /// this binding observes the shared template's entrance/exit nodes and derives the
    /// group node's inputs/outputs from them.
    /// That means: adding a port to the template's entrance/exit node automatically adds a
    /// matching port to every registered instance.
    /// </summary>
    public class ValueTemplateNodeGroupIOBinding : NodeGroupIOBinding
    {
        // Maps: NodeOutputViewModel (subnet side) → NodeInputViewModel (group node side)
        //   for entrance:  entranceOutput → groupNodeInput
        // Maps: NodeOutputViewModel (group node side) → NodeInputViewModel (subnet side)
        //   for exit:      groupNodeOutput → exitNodeInput
        private readonly IDictionary<NodeOutputViewModel, NodeInputViewModel> _outputInputMapping =
            new Dictionary<NodeOutputViewModel, NodeInputViewModel>();

        /// <param name="groupNode">The instance node in the parent network.</param>
        /// <param name="entranceNode">The shared entrance node from <see cref="GroupNodeTemplate"/>.</param>
        /// <param name="exitNode">The shared exit node from <see cref="GroupNodeTemplate"/>.</param>
        public ValueTemplateNodeGroupIOBinding(
            GroupNodeViewModel groupNode,
            NodeViewModel entranceNode,
            NodeViewModel exitNode)
            : base(groupNode, entranceNode, exitNode)
        {
            // For each output on the entrance node → create a matching input on the group node
            entranceNode.Outputs.Connect()
                .Filter(o => o.PortPosition == PortPosition.Left)
                .Transform(o =>
                {
                    NodeInputViewModel input = CreateCompatibleInput((dynamic)o);
                    BindOutputToInput((dynamic)o, (dynamic)input);
                    return input;
                }).PopulateInto(groupNode.Inputs);

            entranceNode.Outputs.Connect()
                .Filter(o => o.PortPosition == PortPosition.Right)
                .Transform(o =>
                {
                    NodeInputViewModel input = CreateCompatibleInput((dynamic)o);
                    BindOutputToInput((dynamic)o, (dynamic)input);
                    return input;
                }).PopulateInto(groupNode.Inputs);

            entranceNode.Outputs.Connect().OnItemRemoved(o =>
                _outputInputMapping.Remove(o));

            // For each input on the exit node → create a matching output on the group node
            exitNode.Inputs.Connect()
                .Filter(i => i.PortPosition == PortPosition.Right)
                .Transform(i =>
                {
                    NodeOutputViewModel output = CreateCompatibleOutput((dynamic)i);
                    BindOutputToInput((dynamic)output, (dynamic)i);
                    return output;
                }).PopulateInto(groupNode.Outputs);

            exitNode.Inputs.Connect()
                .Filter(i => i.PortPosition == PortPosition.Left)
                .Transform(i =>
                {
                    NodeOutputViewModel output = CreateCompatibleOutput((dynamic)i);
                    BindOutputToInput((dynamic)output, (dynamic)i);
                    return output;
                }).PopulateInto(groupNode.Outputs);

            exitNode.Inputs.Connect().OnItemRemoved(i =>
                _outputInputMapping.Remove(
                    _outputInputMapping.First(kvp => kvp.Value == i)));
        }

        #region Endpoint binding helpers

        protected virtual void BindEndpointProperties(NodeOutputViewModel output, NodeInputViewModel input)
        {
            input.WhenAnyValue(vm => vm.Name).BindTo(output, vm => vm.Name);
            output.WhenAnyValue(vm => vm.Name).BindTo(input, vm => vm.Name);
            input.WhenAnyValue(vm => vm.SortIndex).BindTo(output, vm => vm.SortIndex);
            output.WhenAnyValue(vm => vm.SortIndex).BindTo(input, vm => vm.SortIndex);
            input.WhenAnyValue(vm => vm.Icon).BindTo(output, vm => vm.Icon);
            output.WhenAnyValue(vm => vm.Icon).BindTo(input, vm => vm.Icon);
        }

        /// <summary>
        /// Binds a subnet output (entrance) to a group node input, or a group node output to an exit node input.
        /// </summary>
        protected virtual void BindOutputToInput<T>(ValueNodeOutputViewModel<T> output, ValueNodeInputViewModel<T> input)
        {
            BindEndpointProperties(output, input);
            output.Value = input.ValueChanged;
            _outputInputMapping.Add(output, input);
        }

        protected virtual void BindOutputToInput<T>(ValueNodeOutputViewModel<IObservableList<T>> output, ValueListNodeInputViewModel<T> input)
        {
            BindEndpointProperties(output, input);
            output.Value = Observable.Return(input.Values);
            _outputInputMapping.Add(output, input);
        }

        #endregion

        #region Endpoint factories

        public virtual ValueNodeInputViewModel<T> CreateCompatibleInput<T>(ValueNodeOutputViewModel<T> output)
        {
            return new ValueNodeInputViewModel<T>
            {
                Name = output.Name,
                Icon = output.Icon
            };
        }

        public virtual ValueListNodeInputViewModel<T> CreateCompatibleInput<T>(ValueNodeOutputViewModel<IObservableList<T>> output)
        {
            return new ValueListNodeInputViewModel<T>
            {
                Name = output.Name,
                Icon = output.Icon
            };
        }

        public virtual ValueNodeOutputViewModel<T> CreateCompatibleOutput<T>(ValueNodeInputViewModel<T> input)
        {
            return new ValueNodeOutputViewModel<T>
            {
                Name = input.Name,
                Icon = input.Icon
            };
        }

        public virtual ValueNodeOutputViewModel<IObservableList<T>> CreateCompatibleOutput<T>(ValueListNodeInputViewModel<T> input)
        {
            return new ValueNodeOutputViewModel<IObservableList<T>>();
        }

        #endregion

        #region AddNew – adds to the TEMPLATE's entrance/exit nodes so all instances update

        /// <inheritdoc/>
        public override NodeInputViewModel AddNewGroupNodeInput(NodeOutputViewModel candidateOutput)
        {
            // Adding to the template's entrance node propagates to all instances via reactive chain.
            NodeOutputViewModel entranceOutput = CreateCompatibleOutput((dynamic)candidateOutput);
            EntranceNode.Outputs.Add(entranceOutput);
            entranceOutput.SortIndex = EntranceNode.Outputs.Items.Select(o => o.SortIndex).DefaultIfEmpty(-1).Max() + 1;
            // Return this instance's corresponding group node input.
            return GetGroupNodeInput(entranceOutput);
        }

        /// <inheritdoc/>
        public override NodeOutputViewModel AddNewSubnetInlet(NodeInputViewModel candidateInput)
        {
            NodeOutputViewModel entranceOutput = CreateCompatibleOutput((dynamic)candidateInput);
            EntranceNode.Outputs.Add(entranceOutput);
            entranceOutput.SortIndex = EntranceNode.Outputs.Items.Select(o => o.SortIndex).DefaultIfEmpty(-1).Max() + 1;
            return entranceOutput;
        }

        /// <inheritdoc/>
        public override NodeOutputViewModel AddNewGroupNodeOutput(NodeInputViewModel candidateInput)
        {
            // Adding to the template's exit node propagates to all instances via reactive chain.
            NodeInputViewModel exitInput = CreateCompatibleInput((dynamic)candidateInput);
            ExitNode.Inputs.Add(exitInput);
            exitInput.SortIndex = ExitNode.Inputs.Items.Select(i => i.SortIndex).DefaultIfEmpty(-1).Max() + 1;
            // Return this instance's corresponding group node output.
            return GetGroupNodeOutput(exitInput);
        }

        /// <inheritdoc/>
        public override NodeInputViewModel AddNewSubnetOutlet(NodeOutputViewModel candidateOutput)
        {
            NodeInputViewModel exitInput = CreateCompatibleInput((dynamic)candidateOutput);
            ExitNode.Inputs.Add(exitInput);
            exitInput.SortIndex = ExitNode.Inputs.Items.Select(i => i.SortIndex).DefaultIfEmpty(-1).Max() + 1;
            return exitInput;
        }

        #endregion

        #region Getters

        /// <inheritdoc/>
        public override NodeInputViewModel GetGroupNodeInput(NodeOutputViewModel entranceOutput)
        {
            return _outputInputMapping[entranceOutput];
        }

        /// <inheritdoc/>
        public override NodeOutputViewModel GetSubnetInlet(NodeInputViewModel groupNodeInput)
        {
            return _outputInputMapping.Single(p => p.Value == groupNodeInput).Key;
        }

        /// <inheritdoc/>
        public override NodeInputViewModel GetSubnetOutlet(NodeOutputViewModel groupNodeOutput)
        {
            return _outputInputMapping[groupNodeOutput];
        }

        /// <inheritdoc/>
        public override NodeOutputViewModel GetGroupNodeOutput(NodeInputViewModel exitInput)
        {
            return _outputInputMapping.Single(p => p.Value == exitInput).Key;
        }

        #endregion

        /// <summary>
        /// Removes an endpoint from the shared template (affects all instances).
        /// Pass the endpoint from either the group node, entrance node, or exit node.
        /// </summary>
        public virtual void DeleteEndpoint(Endpoint endpoint)
        {
            if (endpoint is NodeInputViewModel input)
            {
                if (input.Parent == GroupNode)
                {
                    // Remove the corresponding entrance output from the template
                    var entranceOutput = GetSubnetInlet(input);
                    EntranceNode.Outputs.Remove(entranceOutput);
                }
                else
                {
                    // It's on the exit node — remove it directly from the template
                    ExitNode.Inputs.Remove(input);
                }
            }
            else if (endpoint is NodeOutputViewModel output)
            {
                if (output.Parent == GroupNode)
                {
                    // Remove the corresponding exit input from the template
                    var exitInput = GetSubnetOutlet(output);
                    ExitNode.Inputs.Remove(exitInput);
                }
                else
                {
                    // It's on the entrance node — remove it directly from the template
                    EntranceNode.Outputs.Remove(output);
                }
            }
        }
    }
}
