﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase;
using YagihataItems.YagiUtils;

namespace YagihataItems.RadialInventorySystemV4
{
    public static class GimmickBuilder
    {
        private class MaterialOverrideData
        {
            public IndexPair index;
            public Material material;
        }
        private class ExclusivePropData
        {
            public IndexPair index;
            public Material material;
        }
        private class IndexPair
        {
            public int group;
            public int prop;
        }
        private static Texture2D boxIcon = null;
        private static Texture2D groupIcon = null;
        private static Texture2D reloadIcon = null;
        private static Texture2D menuIcon = null;
        public static void RemoveFromAvatar(Avatar risAvatar)
        {
            var avatar = risAvatar.AvatarRoot.GetObject();
            if (avatar == null)
                return;
            var autoGeneratedFolder = RIS.AutoGeneratedFolderPath + risAvatar.UniqueID + "/";
            UnityUtils.DeleteFolder(autoGeneratedFolder + "Animations/");
            UnityUtils.DeleteFolder(autoGeneratedFolder + "SubMenus/");
            var fxLayer = avatar.GetFXLayer(autoGeneratedFolder + "AutoGeneratedFXLayer.controller", false);
            if(fxLayer != null)
            {
                foreach (var name in fxLayer.layers.Where(n => n.name.StartsWith("RIS")).Select(n => n.name))
                    fxLayer.TryRemoveLayer(name);
                foreach (var name in fxLayer.parameters.Where(n => n.name.StartsWith("RIS")).Select(n => n.name))
                    fxLayer.TryRemoveParameter(name);
            }
            //v3.1までのtypoを消すための処理
            if (avatar.expressionsMenu != null)
                avatar.expressionsMenu.controls.RemoveAll(n => n.name == "Radiai Inventory");
            //ここまで
            if (avatar.expressionsMenu != null)
                avatar.expressionsMenu.controls.RemoveAll(n => n.name == "Radial Inventory");
            if (avatar.expressionParameters != null)
                foreach (var name in avatar.expressionParameters.parameters.Where(n => n.name.StartsWith("RIS")).Select(n => n.name))
                    avatar.expressionParameters.TryRemoveParameter(name);

        }
        public static void ApplyToAvatar(Avatar risAvatar, EditorTab editorTab)
        {
            /*if (boxIcon == null)
                boxIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(RIS.WorkFolderPath + "Textures/box_icon.png");
            if (groupIcon == null)
                groupIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(RIS.WorkFolderPath + "Textures/group_icon.png");
            if (reloadIcon == null)
                reloadIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(RIS.WorkFolderPath + "Textures/reload_icon.png");
            if (menuIcon == null)
                menuIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(RIS.WorkFolderPath + "Textures/ris_icon.png");

            var autoGeneratedFolder = RIS.AutoGeneratedFolderPath + risAvatar.UniqueID + "/";
            if (!AssetDatabase.IsValidFolder(autoGeneratedFolder))
                UnityUtils.CreateFolderRecursively(autoGeneratedFolder);
            BuildExpressionParameters(risAvatar, autoGeneratedFolder);
            BuildExpressionsMenu(risAvatar, autoGeneratedFolder);
            editorTab.BuildFXLayer(ref risAvatar, autoGeneratedFolder);

            if(risAvatar.ApplyEnableDefault)
            {
                foreach(var group in risAvatar.Groups)
                {
                    foreach(var prop in group.Props)
                    {
                        if (risAvatar.MenuMode == RIS.MenuModeType.Simple || (risAvatar.MenuMode == RIS.MenuModeType.Advanced && prop.MaterialOverride == null))
                        {
                            foreach (var subProp in prop.TargetObjects.Select(n => n?.GetObject()))
                            {
                                if (subProp != null)
                                {
                                    subProp.SetActive(prop.IsDefaultEnabled);
                                    EditorUtility.SetDirty(subProp);
                                }
                            }
                        }
                    }
                }
            }
            */
            EditorUtility.DisplayDialog("Radial Inventory System", "ビルド成功！", "OK");
        }
        private static void CheckParam(VRCAvatarDescriptor avatar, AnimatorController controller, string paramName, bool defaultEnabled)
        {
            var param = controller.parameters.FirstOrDefault(n => n.name == paramName);
            if (param == null)
            {
                controller.AddParameter(paramName, AnimatorControllerParameterType.Bool);
                param = controller.parameters.FirstOrDefault(n => n.name == paramName);
            }
            param.type = AnimatorControllerParameterType.Bool;
            param.defaultBool = defaultEnabled;
        }

        private static void BuildExpressionParameters(Avatar risAvatar, string autoGeneratedFolder)
        {
            var avatar = risAvatar.AvatarRoot?.GetObject();

            if (avatar == null)
                return;

            var expParams = avatar.GetExpressionParameters(autoGeneratedFolder);
            if (risAvatar.OptimizeParameters)
                expParams.OptimizeParameter();
            foreach (var name in expParams.parameters.Where(n => n.name.StartsWith("RIS")).Select(n => n.name))
                expParams.TryRemoveParameter(name);
            foreach (var groupIndex in Enumerable.Range(0, risAvatar.Groups.Count))
            {
                var group = risAvatar.Groups[groupIndex];
                if (group.UseResetButton)
                    TryAddParam(risAvatar, $"RIS-G{groupIndex}RESET", 0f, false);
                foreach (var propIndex in Enumerable.Range(0, group.Props.Count))
                {
                    var prop = group.Props[propIndex];
                    var v2Mode = false;
                    if ((risAvatar.MenuMode == RIS.MenuModeType.Simple && risAvatar.GetExclusiveMode((RIS.ExclusiveGroupType)groupIndex) ==  RIS.ExclusiveModeType.ExclusiveV2) ||
                        (risAvatar.MenuMode == RIS.MenuModeType.Advanced && prop.ExclusiveGroup != RIS.ExclusiveGroupType.None))
                        v2Mode = true;
                    TryAddParam(risAvatar, $"RIS-G{groupIndex}P{propIndex}", prop.IsDefaultEnabled && !v2Mode ? 1f : 0f, prop.UseSaveParameter);
                }
            }
            avatar.expressionParameters = expParams;
            EditorUtility.SetDirty(avatar);
            EditorUtility.SetDirty(avatar.expressionParameters);
        }
        private static void BuildExpressionsMenu(Avatar risAvatar, string autoGeneratedFolder)
        {
            var avatar = risAvatar.AvatarRoot?.GetObject();

            if (avatar == null)
                return;

            VRCExpressionsMenu menu = null;
            avatar.customExpressions = true;
            var rootMenu = avatar.expressionsMenu;
            if (rootMenu == null)
                rootMenu = avatar.expressionsMenu = UnityUtils.TryGetAsset(autoGeneratedFolder + "AutoGeneratedMenu.asset", typeof(VRCExpressionsMenu)) as VRCExpressionsMenu;

            //v3.1までのtypoメニューを消す処理
            rootMenu.controls.RemoveAll(n => n.name == "Radiai Inventory");

            var risControl = rootMenu.controls.FirstOrDefault(n => n.name == "Radial Inventory");
            if (risControl == null)
                rootMenu.controls.Add(risControl = new VRCExpressionsMenu.Control() { name = "Radial Inventory" });
            risControl.icon = menuIcon;
            risControl.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
            risControl.subMenu = menu = UnityUtils.TryGetAsset(autoGeneratedFolder + $"RadInvMainMenu.asset", typeof(VRCExpressionsMenu)) as VRCExpressionsMenu;

            var subMenuFolder = autoGeneratedFolder + "SubMenus/";
            UnityUtils.ReCreateFolder(subMenuFolder);
            menu.controls.Clear();
            foreach(var groupIndex in Enumerable.Range(0, risAvatar.Groups.Count))
            {
                var group = risAvatar.Groups[groupIndex];
                var groupName = group.Name;
                if (string.IsNullOrWhiteSpace(groupName))
                    groupName = "Group" + groupIndex.ToString();

                VRCExpressionsMenu.Control control = new VRCExpressionsMenu.Control();
                control.name = groupName;
                control.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
                control.icon = group.Icon?.GetObject();
                if (control.icon == null)
                    control.icon = groupIcon;
                if (risAvatar.MenuMode == RIS.MenuModeType.Advanced && group.BaseMenu?.GetObject() != null)
                    AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(group.BaseMenu.GetObject()), subMenuFolder + $"Group{groupIndex}Menu.asset");
                VRCExpressionsMenu subMenu = control.subMenu = UnityUtils.TryGetAsset(subMenuFolder + $"Group{groupIndex}Menu.asset", typeof(VRCExpressionsMenu)) as VRCExpressionsMenu;
                if(group.UseResetButton)
                {
                    var propControl = new VRCExpressionsMenu.Control();
                    propControl.name = "Reset";
                    propControl.type = VRCExpressionsMenu.Control.ControlType.Toggle;
                    propControl.value = 1f;
                    propControl.icon = reloadIcon;
                    propControl.parameter = new VRCExpressionsMenu.Control.Parameter() { name = $"RIS-G{groupIndex}RESET" };
                    subMenu.controls.Add(propControl);
                }
                foreach (var propIndex in Enumerable.Range(0,group.Props.Count))
                {
                    var prop = group.Props[propIndex];
                    var propName = prop.GetPropName(risAvatar);
                    if (string.IsNullOrWhiteSpace(propName))
                        propName = "Prop" + propIndex.ToString();
                    var propControl = new VRCExpressionsMenu.Control();
                    propControl.name = propName;
                    propControl.type = VRCExpressionsMenu.Control.ControlType.Toggle;
                    propControl.value = 1f;
                    propControl.parameter = new VRCExpressionsMenu.Control.Parameter() { name = $"RIS-G{groupIndex}P{propIndex}"};
                    propControl.icon = prop.Icon?.GetObject();
                    if (propControl.icon == null)
                        propControl.icon = boxIcon;
                    subMenu.controls.Add(propControl);
                }
                control.subMenu = subMenu;
                menu.controls.Add(control);
                EditorUtility.SetDirty(control.subMenu);
            }
            avatar.expressionsMenu = rootMenu;
            EditorUtility.SetDirty(menu);
            EditorUtility.SetDirty(avatar.expressionsMenu);
            EditorUtility.SetDirty(avatar);
        }

        private static void TryAddParam(Avatar risAvatar, string name, float defaultValue, bool saved)
        {
            var avatar = risAvatar.AvatarRoot?.GetObject();

            if (avatar == null)
                return;
            var expParams = avatar.expressionParameters;
            if (risAvatar.OptimizeParameters)
            {
                var existParam = expParams.FindParameter(name);
                if(existParam != null)
                {
                    existParam.saved = saved;
                    existParam.valueType = VRCExpressionParameters.ValueType.Bool;
                    existParam.defaultValue = defaultValue;
                }
                else
                {
                    expParams.AddParameter(name, VRCExpressionParameters.ValueType.Bool, saved, defaultValue);
                }

            }
            else
                expParams.AddParameter(name, VRCExpressionParameters.ValueType.Bool, saved, defaultValue);

        }
    }
}