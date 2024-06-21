namespace Core.Poseidon.InspectorGenerator.Generator.Editor
{
#if UNITY_EDITOR
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Sirenix.OdinInspector;
    using Sirenix.Utilities;
    using UnityEditor;
    using UnityEngine;
    using Utils;

    public static class InspectorGenerator
    {
        #region Private Variables
        private static List<string> usingsMap = new List<string>();
        #endregion
        #region Public API

        public static void GenerateViewPartialInspector(List<InspectorGeneratorEditorWindow.FieldEntry> fields, Type baseComponentType,
            string viewScriptFilePath, string componentName)
        {
            var code = GeneratePartialCode(fields, baseComponentType, mainComponentName: componentName);
            var fileName = $"{baseComponentType.GetNiceName()}.Inspector.cs";
            var path = viewScriptFilePath.Replace($"{baseComponentType.GetNiceName()}.cs", fileName);
            File.WriteAllText(path, code);
            AssetDatabase.Refresh();
        }

        public static void GenerateComponentAndPartialInspector(List<InspectorGeneratorEditorWindow.FieldEntry> fields,
            Type baseComponentType, string componentName, string componentNamespace, string scriptFilePath)
        {
            var mainComponentCode = GenerateMainComponentCode(componentName, baseComponentType, componentNamespace);
            
            var mainComponentPath = $"{scriptFilePath}/{componentName}.cs";
            
            var partialCode = GeneratePartialCode(fields, null, componentNamespace, componentName);
            var partialFileName = $"{componentName}.Inspector.cs";
            
            var partialPath = mainComponentPath.Replace($"{componentName}.cs", partialFileName);
            
            File.WriteAllText(mainComponentPath, mainComponentCode);
            File.WriteAllText(partialPath, partialCode);
            AssetDatabase.Refresh();
        }
        #endregion

        #region Private Methods

        private static string GeneratePartialCode(List<InspectorGeneratorEditorWindow.FieldEntry> fieldEntries, Type baseType, 
            string optionalNamespace = "", string mainComponentName = "")
        {
            var template = InspectorGeneratorCodeTemplates.PartialComponentCodeTemplate;

            var componentNamespace =
                baseType == null
                    ? optionalNamespace.IsNullOrEmpty() ? "Default" : optionalNamespace
                    : baseType.Namespace;
            var componentName = baseType == null ? mainComponentName : baseType.GetNiceName();
            var componentBaseClass = baseType == null ? string.Empty : baseType.BaseType.GetNiceName();
            var usings = GetUsings(fieldEntries, baseType);

            var fields = GenerateFields(fieldEntries);

            template = template.Replace(InspectorGeneratorCodeTemplates.NamespaceSymbol, componentNamespace);
            template = template.Replace(InspectorGeneratorCodeTemplates.UsingsSymbol, usings);
            template = template.Replace(InspectorGeneratorCodeTemplates.ComponentNameSymbol, componentName);
            template = template.Replace(InspectorGeneratorCodeTemplates.BaseClassSymbol, componentBaseClass.IsNullOrEmpty() ? "" 
                : $" : {componentBaseClass}");
            template = template.Replace(InspectorGeneratorCodeTemplates.GeneratedFieldsSymbol, fields);

            return template;
        }

        private static string GenerateMainComponentCode(string componentName, Type baseType, string componentNamespace)
        {
            var template = InspectorGeneratorCodeTemplates.NewComponentCodeTemplate;
            var usings = baseType == null ? "" : $"using {baseType.Namespace};\r";
            
            template = template.Replace(InspectorGeneratorCodeTemplates.NamespaceSymbol, componentNamespace);
            template = template.Replace(InspectorGeneratorCodeTemplates.UsingsSymbol, usings);
            template = template.Replace(InspectorGeneratorCodeTemplates.ComponentNameSymbol, componentName);
            template = template.Replace(InspectorGeneratorCodeTemplates.BaseClassSymbol, baseType == null ? "" 
                : $" : {baseType.GetNiceName()}");
            template = template.Replace(InspectorGeneratorCodeTemplates.GeneratedFieldsSymbol, "");
            
            return template;
        }

        private static string GetUsings(List<InspectorGeneratorEditorWindow.FieldEntry> fieldEntries, Type baseType)
        {
            var output = "";
            var usingClause = "using [namespace];\n    ";
            usingsMap.Clear();
            output += usingClause.Replace("[namespace]", typeof(SerializeField).Namespace);
            usingsMap.Add(usingClause.Replace("[namespace]", typeof(SerializeField).Namespace));
            
            output += usingClause.Replace("[namespace]", typeof(BoxGroupAttribute).Namespace);
            usingsMap.Add(usingClause.Replace("[namespace]", typeof(BoxGroupAttribute).Namespace));

            foreach (var fieldEntry in fieldEntries)
            {
                var clause = usingClause.Replace("[namespace]", fieldEntry.FieldType.Namespace);
                if (usingsMap.Contains(clause))
                {
                    continue;
                }
                output += $"{clause}";
            }

            if (baseType == null) return output;
            var baseTypeClause =
                usingClause.Replace("[namespace]",
                    baseType.BaseType == null ? baseType.Namespace : baseType.BaseType.Namespace);

            if (!usingsMap.Contains(baseTypeClause))
            {
                output += $"{baseTypeClause}";
            }

            return output;
        }

        private static string GenerateFields(List<InspectorGeneratorEditorWindow.FieldEntry> fieldEntries)
        {
            return fieldEntries.Aggregate("", (current, fieldEntry) => current + $"{fieldEntry}\r	    ");
        }
        
        #endregion
    }
#endif
}