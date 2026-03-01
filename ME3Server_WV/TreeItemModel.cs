using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ME3Server_WV
{
    /// <summary>
    /// Cross-platform tree node model, replacing System.Windows.Forms.TreeNode.
    /// Used by Blaze.cs and the packet editor UI.
    /// </summary>
    public class TreeItemModel
    {
        public string Text { get; set; } = "";
        public string Name { get; set; } = "";
        public ObservableCollection<TreeItemModel> Children { get; set; } = new();

        public TreeItemModel() { }

        public TreeItemModel(string text)
        {
            Text = text;
        }
    }
}
