using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrooxEngine;
using HarmonyLib;

namespace DynVarSpaceTree
{
    internal class SpaceTree
    {
        private readonly Slot slot;
        private readonly DynamicVariableSpace space;
        private SpaceTree[] children;
        private IDynamicVariable[] dynVars;

        public SpaceTree(DynamicVariableSpace space, Slot slot = null)
        {
            this.space = space;
            this.slot = slot ?? space.Slot;
        }

        public bool Process()
        {
            dynVars = slot.GetComponents<IDynamicVariable>(isLinkedDynVar).ToArray();

            children = slot.Children.Select(child => new SpaceTree(space, child)).Where(tree => tree.Process()).ToArray();

            return dynVars.Any() || children.Any();
        }

        public override string ToString()
        {
            var builder = new StringBuilder(space.Slot.Name).Append(": Namespace ").AppendLine(space.SpaceName);

            buildString(builder, "");
            builder.Remove(builder.Length - Environment.NewLine.Length, Environment.NewLine.Length);

            return builder.ToString();
        }

        private void appendDynVar(StringBuilder builder, string indent, IDynamicVariable dynVar, bool last = false)
        {
            builder.Append(indent);
            builder.Append(last ? "└─" : "├─");
            builder.Append(dynVar.VariableName);
            builder.Append(" (");
            builder.AppendTypeName(dynVar.GetType());
            builder.AppendLine(")");
        }

        private void appendSlot(StringBuilder builder, string indent, SpaceTree child, bool first, bool last)
        {
            if (!first)
            {
                builder.Append(indent);
                builder.AppendLine("│");
            }

            builder.Append(indent);
            builder.Append(last ? "└─" : "├─");
            builder.AppendLine(child.slot.Name);

            child.buildString(builder, indent + (last ? "  " : "│ "));
        }

        private void buildString(StringBuilder builder, string indent)
        {
            if (dynVars.Any())
            {
                for (var i = 0; i < dynVars.Length - 1; ++i)
                    appendDynVar(builder, indent, dynVars[i]);

                appendDynVar(builder, indent, dynVars[dynVars.Length - 1], !children.Any());

                if (children.Any())
                {
                    builder.Append(indent);
                    builder.AppendLine("│");
                }
            }

            if (children.Any())
                for (var i = 0; i < children.Length; ++i)
                    appendSlot(builder, indent, children[i], i == 0, i == children.Length - 1);
        }

        private bool isLinkedDynVar(IDynamicVariable dynVar)
        {
            return Traverse.Create(dynVar).Field("handler").Field("_currentSpace").GetValue() == space;
        }
    }
}