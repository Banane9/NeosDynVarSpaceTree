using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;
using static FrooxEngine.DynamicVariableSpace;

namespace DynVarSpaceTree
{
    public class DynVarSpaceTree : ResoniteMod
    {
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> EnableLinkedVariablesList = new("EnableLinkedVariablesList", "Allow generating a list of dynamic variable definitions for a space.", () => true);

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> EnableVariableHierarchy = new("EnableVariableHierarchy", "Allow generating a hierarchy of dynamic variable components for a space.", () => true);

        private static ModConfiguration Config;

        public override string Author => "Banane9";
        public override string Link => "https://github.com/Banane9/ResoniteDynVarSpaceTree";
        public override string Name => "DynVarSpaceTree";
        public override string Version => "2.0.0";

        public override void OnEngineInit()
        {
            var harmony = new Harmony($"{Author}.{Name}");
            Config = GetConfiguration();
            Config.Save(true);
            harmony.PatchAll();
            CustomUILib.CustomUILib.AddCustomInspectorAfter<DynamicVariableSpace>(BuildInspectorUI);
        }

        private static void BuildInspectorUI(DynamicVariableSpace space, UIBuilder ui)
        {
            if (Config.GetValue(EnableLinkedVariablesList))
                MakeButton(ui, "Output names of linked Variables", () => OutputVariableNames(space));

            if (Config.GetValue(EnableVariableHierarchy))
                MakeButton(ui, "Output tree of linked Variable Hierarchy", () => OutputVariableHierarchy(space));
        }

        private static void MakeButton(UIBuilder ui, string text, Action action)
        {
            var button = ui.Button(text);
            button.LocalPressed += (b, e) => action();
        }

        private static void OutputVariableHierarchy(DynamicVariableSpace space)
        {
            var hierarchy = new SpaceTree(space);

            if (hierarchy.Process())
            {
                var outslot = space.LocalUserSpace.AddSlot("Variable Hierarchy");
                UniversalImporter.SpawnText(outslot, "Variable Hierarchy", hierarchy.ToString());
                outslot.PositionInFrontOfUser(float3.Backward);
            }
        }

        private static void OutputVariableNames(DynamicVariableSpace space)
        {
            var names = new StringBuilder("Variables linked to Namespace ");
            names.Append(space.SpaceName);
            names.AppendLine(":");

            var values = Traverse.Create(space).Field("_dynamicValues").GetValue() as IDictionary;

            foreach (var identity in values.Keys)
            {
                var name = Traverse.Create(identity).Field("name").GetValue() as string;
                var type = Traverse.Create(identity).Field("type").GetValue() as Type;
                names.AppendLine($"{name} ({type})");
            }

            names.Remove(names.Length - Environment.NewLine.Length, Environment.NewLine.Length);

            var outslot = space.LocalUserSpace.AddSlot("Variable Names");
            UniversalImporter.SpawnText(outslot, "Variable Names", names.ToString());
            outslot.PositionInFrontOfUser(float3.Backward);
        }
    }
}