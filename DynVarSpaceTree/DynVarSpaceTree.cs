using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BaseX;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using NeosModLoader;

namespace DynVarSpaceTree
{
    public class DynVarSpaceTree : NeosMod
    {
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> EnableLinkedVariablesList = new("EnableLinkedVariablesList", "Allow generating a list of dynamic variable definitions for a space.", () => true);

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> EnableVariableHierarchy = new("EnableVariableHierarchy", "Allow generating a hierarchy of dynamic variable components for a space.", () => true);

        private static ModConfiguration Config;

        public override string Author => "Banane9";
        public override string Link => "https://github.com/Banane9/NeosDynVarSpaceTree";
        public override string Name => "DynVarSpaceTree";
        public override string Version => "2.0.0";

        public override void OnEngineInit()
        {
            var harmony = new Harmony($"{Author}.{Name}");
            Config = GetConfiguration();
            Config.Save(true);
            harmony.PatchAll();
            CustomUILib.CustomUILib.AddCustomInspector<DynamicVariableSpace>(BuildInspectorUI);
        }

        private static void BuildInspectorUI(DynamicVariableSpace space, UIBuilder ui)
        {
            CustomUILib.CustomUILib.BuildInspectorUI(space, ui);

            var outputField = ui.Current.AttachComponent<ValueField<string>>();

            if (Config.GetValue(EnableLinkedVariablesList))
                MakeButton(ui, "Output names of linked Variables", () => OutputVariableNames(space, outputField.Value));

            if (Config.GetValue(EnableVariableHierarchy))
                MakeButton(ui, "Output tree of linked Variable Hierarchy", () => OutputVariableHierarchy(space, outputField.Value));

            SyncMemberEditorBuilder.BuildField(outputField.Value, "Output", outputField.GetSyncMemberFieldInfo("Value"), ui);
        }

        private static void MakeButton(UIBuilder ui, string text, Action action)
        {
            var button = ui.Button(text);
            button.RequireLockInToPress.Value = true;

            var valueField = button.Slot.AttachComponent<ValueField<bool>>().Value;

            var toggle = button.Slot.AttachComponent<ButtonToggle>();
            toggle.TargetValue.Target = valueField;

            valueField.OnValueChange += field => action();
        }

        private static void OutputVariableHierarchy(DynamicVariableSpace space, Sync<string> target)
        {
            var hierarchy = new SpaceTree(space);

            if (hierarchy.Process())
                target.Value = hierarchy.ToString();
            else
                target.Value = "";
        }

        private static void OutputVariableNames(DynamicVariableSpace space, Sync<string> target)
        {
            var names = new StringBuilder("Variables linked to Namespace ");
            names.Append(space.SpaceName);
            names.AppendLine(":");

            foreach (var identity in space._dynamicValues.Keys)
            {
                names.Append(identity.name);
                names.Append(" (");
                names.AppendTypeName(identity.type);
                names.AppendLine(")");
            }

            names.Remove(names.Length - Environment.NewLine.Length, Environment.NewLine.Length);

            target.Value = names.ToString();
        }
    }
}