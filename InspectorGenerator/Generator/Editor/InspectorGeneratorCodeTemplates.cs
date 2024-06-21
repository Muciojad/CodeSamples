namespace Core.Poseidon.InspectorGenerator.Generator.Editor
{
#if UNITY_EDITOR
    public static class InspectorGeneratorCodeTemplates
    {
        public static string PartialComponentCodeTemplate = @"namespace [NAMESPACE]
{
    [USINGS]

    public partial class [COMPONENT_NAME]
    {
        #region Inspector
        //[GENERATED_FIELDS]
        #endregion
    }
}";
        public static string NewComponentCodeTemplate = @"namespace [NAMESPACE]
{
    [USINGS]

    public partial class [COMPONENT_NAME] [: [BASE_CLASS]]
    {
        // todo: add your logic here
    }
}";

        public static string NamespaceSymbol = "[NAMESPACE]";
        public static string UsingsSymbol = "[USINGS]";
        public static string ComponentNameSymbol = "[COMPONENT_NAME]";
        public static string BaseClassSymbol = "[: [BASE_CLASS]]";
        public static string GeneratedFieldsSymbol = "//[GENERATED_FIELDS]";
    }
#endif
}