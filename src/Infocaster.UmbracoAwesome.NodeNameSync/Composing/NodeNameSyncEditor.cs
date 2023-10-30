using Umbraco.Cms.Core.PropertyEditors;

namespace NodeNameSync.Composing
{

    [DataEditor(
    alias: "Infocaster.NodeNameSync",
    EditorType.PropertyValue,
    name: "Node name sync",
    view: "/App_Plugins/NodeNameSync/nodenamesync.html",
    Icon = "icon-nodes",
    HideLabel = false,
    ValueType = "STRING")]
    public class NodeNameSyncEditor : DataEditor
    {
        public NodeNameSyncEditor(IDataValueEditorFactory dataValueEditorFactory, EditorType type = EditorType.PropertyValue)
            : base(dataValueEditorFactory, type)
        {
        }
    }
}
