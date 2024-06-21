namespace Core.Poseidon.InspectorGenerator.Generator.Editor
{
#if UNITY_EDITOR
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Core.ViewManager;
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using UnityEditor;
    using UnityEngine;
    using Utils;
    using Object = UnityEngine.Object;

    public class InspectorGeneratorEditorWindow : OdinEditorWindow
    {
        #region Consts
        private const string INSPECTOR_WINDOW_TYPE = "UnityEditor.InspectorWindow,UnityEditor.dll";
        #endregion
        #region Window API

        [MenuItem("CONTEXT/Component/InspectorGenerator", false, -100)]
        public static void OpenWindow(MenuCommand command)
        {
            var window = GetWindow<InspectorGeneratorEditorWindow>(new[]
                { Type.GetType(INSPECTOR_WINDOW_TYPE) });
            window.CurrentObject = (command.context as Component)?.gameObject;
            window.FieldEntries = new List<FieldEntry>();
            window.CurrentField = default;
            window.Show();
        }

        protected override void Initialize()
        {
            Selection.selectionChanged += OnSelectionChangedHandler;
            base.Initialize();
        }

        protected override void OnDestroy()
        {
            Selection.selectionChanged -= OnSelectionChangedHandler;
            base.OnDestroy();
        }

        #endregion

        #region Window Content

        [BoxGroup("Current Object"), HideLabel, OdinSerialize] public GameObject CurrentObject;
        [BoxGroup("Settings")] public bool GenerateOnlyInspectorClass;

        [BoxGroup("Settings"), ShowIf(nameof(GenerateOnlyInspectorClass)), ValueDropdown(nameof(GetObjectComponents))]
        public Component BaseComponent;

        [BoxGroup("Settings"), HideIf(nameof(GenerateOnlyInspectorClass))]
        public string NewComponentName;

        [BoxGroup("Settings"), HideIf(nameof(GenerateOnlyInspectorClass))]
        public Type NewComponentBaseType;

        [BoxGroup("Settings"), HideIf(nameof(GenerateOnlyInspectorClass)), FolderPath, OnValueChanged(nameof(OnSavePathValueChanged))]
        public string ComponentSavePath;

        [BoxGroup("Settings"), HideIf(nameof(GenerateOnlyInspectorClass))]
        public string ComponentNamespace;
        

        [BoxGroup("Add field"), HideLabel, OdinSerialize]
        public FieldEntry CurrentField;

        [BoxGroup("Actions"), Button("Add", ButtonSizes.Medium)]
        private void AddField()
        {
            var fieldCopy = new FieldEntry(CurrentField);
            FieldEntries.Add(fieldCopy);
        }

        [BoxGroup("Actions"), Button("Generate View Inspector", ButtonSizes.Medium)]
        private void GenerateInspector()
        {
            if(GenerateOnlyInspectorClass)
            {
                var asset = GetScriptFileByObject(BaseComponent);
                if (asset == null)
                {
                    return;
                }

                var assetPath = AssetDatabase.GetAssetPath(asset);
                requestReferencesFill = true;
                InspectorGenerator.GenerateViewPartialInspector(FieldEntries, BaseComponent.GetType(), assetPath, BaseComponent.name);
                return;
            }
            
            requestReferencesFill = true;

            InspectorGenerator.GenerateComponentAndPartialInspector(FieldEntries, NewComponentBaseType, 
                NewComponentName, ComponentNamespace, ComponentSavePath);
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            if (!HasOpenInstances<InspectorGeneratorEditorWindow>())
            {
                return;
            }
            var window = GetWindow<InspectorGeneratorEditorWindow>(new[]
                { Type.GetType(INSPECTOR_WINDOW_TYPE) });
            window.FillReferencesInView();
        }

        private void OnSavePathValueChanged()
        {
            if(GenerateOnlyInspectorClass) return;
            ComponentNamespace = ComponentSavePath.Replace("Assets/Scripts/", "").Replace("/", ".");
        }
     
        private void FillReferencesInView()
        {
            if(!requestReferencesFill) return;
            requestReferencesFill = false;

            var targetType = GenerateOnlyInspectorClass ? BaseComponent.GetType() : GetType($"{ComponentNamespace}.{NewComponentName}");
            if(targetType == null)
            {
                Debug.LogError("no type");
                return;
            }
            object targetObj = GenerateOnlyInspectorClass ? BaseComponent : CurrentObject.AddComponent(targetType);
            if(targetObj == null)
            {
                Debug.LogError("no obj");
                return;
            }
            
            foreach (var fieldEntry in FieldEntries)
            {
                var field = targetType.GetField(fieldEntry.FieldName,
                    BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if(field == null) continue;

                var component = fieldEntry.ReferenceObject.GetComponent(fieldEntry.FieldType);
                if(component == null) continue;
                field.SetValue(targetObj, component);
            }
            foreach (var fieldEntry in FieldEntries)
            {
                var property = targetType.GetProperty(fieldEntry.FieldName,
                    BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if(property == null) continue;

                var component = fieldEntry.ReferenceObject.GetComponent(fieldEntry.FieldType);
                if(component == null) continue;
                property.SetValue(targetObj, component);
            }
            
            EditorUtility.SetDirty(CurrentObject);
        }

        [FoldoutGroup("Preview fields"), HideLabel, OdinSerialize]
        public List<FieldEntry> FieldEntries = new List<FieldEntry>();

        #endregion

        #region Helpers

        private GameObject currentSelection;
        private bool requestReferencesFill;
        
        public Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }
        
        private static TextAsset GetScriptFileByObject(Object contextObject)
        {
            var textAssets = AssetsUtils.FindAssets<TextAsset>();
            var matchingAsset = textAssets.FirstOrDefault(t => t.name == $"{contextObject.GetType().GetNiceName()}");
            return matchingAsset;
        }

        private void OnSelectionChangedHandler()
        {
            // analyze if current selection is child of current view
            var activeObject = Selection.activeObject;
            if (activeObject is not GameObject gameObject) return;
            if (!gameObject.transform.IsChildOf(CurrentObject.transform))
            {
                return;
            }

            CurrentField = new FieldEntry(gameObject);
        }
        

        private List<Component> GetObjectComponents()
        {
            return CurrentObject == null ? new List<Component>() : CurrentObject.GetComponents<Component>().ToList();
        }

        [Serializable]
        public sealed class FieldEntry
        {
            public GameObject ReferenceObject;
            public string FieldName;
            [ValueDropdown(nameof(GetAccessors)), OnValueChanged(nameof(OnValueChanged))]
            public string Accessor = "private";

            public string BoxGroup = "References";

            [ReadOnly]
            public string FieldAttribute;

            [ValueDropdown(nameof(GetComponents)), OnValueChanged(nameof(OnValueChanged))]
            [OdinSerialize] public Type FieldType;

            private List<Type> componentTypes;

            public FieldEntry(GameObject fieldObject)
            {
                ReferenceObject = fieldObject;
                var allComponents = fieldObject.GetComponents<Component>();
                componentTypes = allComponents.Select(c => c.GetType()).ToList();
                FieldName = ReferenceObject.name.FirstCharToLowerCase() ?? "null";
                FieldType = componentTypes[0];
                OnValueChanged();
            }

            public FieldEntry(FieldEntry otherEntry)
            {
                ReferenceObject = otherEntry.ReferenceObject;
                Accessor = $"{otherEntry.Accessor}";
                FieldAttribute = $"{otherEntry.FieldAttribute}";
                FieldName = $"{otherEntry.FieldName}";
                FieldType = otherEntry.FieldType;
                BoxGroup = $"{otherEntry.BoxGroup}";
            }

            public override string ToString()
            {
                var propertyGetSet = Accessor == "public" ? "{get; private set;}" : ";";
                return $"{MergeFieldAttributes()} {Accessor} {FieldType.GetNiceName()} {FieldName} {propertyGetSet}";
            }

            #region Helpers

            private string MergeFieldAttributes()
            {
                if (BoxGroup.IsNullOrEmpty()) return FieldAttribute;
                var attrEndIndex = FieldAttribute.LastIndexOf("]", StringComparison.Ordinal);
                if (attrEndIndex == -1) return "";
                var mergedAttr = FieldAttribute.Insert(attrEndIndex, $", BoxGroup(\"{BoxGroup}\")");
                return mergedAttr;
            }
            private void OnValueChanged()
            {
                FieldAttribute = Accessor is "private" or "protected" ? "[SerializeField]" : "[field: SerializeField]";
                FieldName = Accessor is "public" ? ReferenceObject.name.FirstCharToUpperCase()?.Replace(" ", string.Empty)
                    : ReferenceObject.name.FirstCharToLowerCase()?.Replace(" ", string.Empty);
            }

            #endregion

            #region Dropdowns

            private List<string> GetAccessors() => new List<string>() {"private", "protected", "public"};
            private List<Type> GetComponents() => componentTypes;

            #endregion
        }
        #endregion
    }
#endif
    
}