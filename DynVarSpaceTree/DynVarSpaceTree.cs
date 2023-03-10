using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BaseX;
using CodeX;
using FrooxEngine;
using FrooxEngine.LogiX;
using FrooxEngine.LogiX.Data;
using FrooxEngine.LogiX.ProgramFlow;
using FrooxEngine.UIX;
using HarmonyLib;
using NeosModLoader;

namespace DynVarSpaceTree
{
    public class DynVarSpaceTree : NeosMod
    {
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> EnableLinkedVariablesList = new ModConfigurationKey<bool>("EnableLinkedVariablesList", "Allow generating a list of dynamic variable definitions for a space.", () => true);

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> EnableVariableHierarchy = new ModConfigurationKey<bool>("EnableVariableHierarchy", "Allow generating a hierarchy of dynamic variable components for a space.", () => true);

        private static ModConfiguration Config;

        public override string Author => "Banane9";
        public override string Link => "https://github.com/Banane9/NeosDynVarSpaceTree";
        public override string Name => "DynVarSpaceTree";
        public override string Version => "1.0.1";

        public override void OnEngineInit()
        {
            var harmony = new Harmony($"{Author}.{Name}");
            Config = GetConfiguration();
            Config.Save(true);
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(WorkerInspector))]
        private static class WorkerInspectorPatches
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(WorkerInspector.BuildInspectorUI))]
            private static void BuildInspectorUIPostfix(Worker worker, UIBuilder ui)
            {
                if (!(worker is DynamicVariableSpace space))
                    return;

                if (Config.GetValue(EnableLinkedVariablesList))
                {
                    var namesButton = ui.Button("Copy Names of linked Variables", color.Pink);
                    namesButton.LocalPressed += (button, data) => copyVariableNames(space);
                    namesButton.RequireLockInToPress.Value = true;
                }

                if (Config.GetValue(EnableVariableHierarchy))
                {
                    var treeButton = ui.Button("Copy Tree of linked Variable Hierarchy", color.Pink);
                    treeButton.LocalPressed += (button, data) => copyVariableHierarchy(space);
                    treeButton.RequireLockInToPress.Value = true;
                }
            }

            private static void copyVariableHierarchy(DynamicVariableSpace space)
            {
                var hierarchy = new SpaceTree(space);

                if (hierarchy.Process())
                    space.InputInterface.Clipboard.SetText(hierarchy.ToString());
                else
                    space.InputInterface.Clipboard.SetText("");
            }

            private static void copyVariableNames(DynamicVariableSpace space)
            {
                var names = new StringBuilder("Variables linked to Namespace ");
                names.Append(space.SpaceName);
                names.AppendLine(":");

                foreach (var identity in space._dynamicValues.Keys)
                {
                    var traverse = Traverse.Create(identity);

                    names.Append(identity.name);
                    names.Append(" (");
                    names.AppendTypeName(identity.type);
                    names.AppendLine(")");
                }

                names.Remove(names.Length - Environment.NewLine.Length, Environment.NewLine.Length);

                space.InputInterface.Clipboard.SetText(names.ToString());
            }
        }
    }
}