using JsonViewer.Controls;
using JsonViewer.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace JsonViewer
{
    public static class Extensions
    {      
        public static bool ContainsIgnoreCase(this string value1, string value2)
        {
            if (value1 == null || value2 == null)
            {
                return false;
            }
            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(value1, value2, CompareOptions.IgnoreCase) >= 0;
        }

        public static JsonItem DeepCopy(this JsonItem original)
        {
            var clone = new JsonItem
            {
                Index = original.Index,
                IsExpanded = original.IsExpanded,
                IsSelected = original.IsSelected,
                IsVisible = original.IsVisible,
                ItemType = original.ItemType,
                Name = original.Name,
                Value = original.Value,
                Parent = original.Parent,
                IsMatch = original.IsMatch,
            };
            if (clone.Parent != null)
            {
                clone.Parent.Nodes.Add(clone);
            }
            foreach (var node in original.Nodes)
            {
                node.Parent = clone;
                DeepCopy(node);
            }
            return clone;
        }

        public static List<JsonItem> ToList(this JsonItem node)
        {
            return ToList(node, new List<JsonItem>());
        }         

        public static void GoNext(this ItemsControl container, JsonItem root)
        {
            SelectItem(container, root, true);
        }

        public static void GoPrev(this ItemsControl container, JsonItem root)
        {
            SelectItem(container, root, false);
        }

        public static void SetParentsState(this JsonItem node, Action<JsonItem> action)
        {
            action(node);
            var parent = node.Parent;
            while (parent != null)
            {
                action(parent);
                parent = parent.Parent;
            }
        }

        public static FilterResponse GetFilteredItem(this JsonItem original, JsonItem current, string filter, bool showAll)
        {
            var result = new FilterResponse();
            if (string.IsNullOrEmpty(filter))
            {
                var clone = original.DeepCopy();
                var currentList = current.ToList();
                var filteredList = currentList.Where(o => o.IsMatch);
                var selectedItem = currentList.FirstOrDefault(o => o.IsSelected);
                var oroginalsList = clone.ToList();
                foreach (var filteredItem in filteredList)
                {
                    var oItem = oroginalsList.FirstOrDefault(o => o.Index == filteredItem.Index);
                    if (oItem != null)
                    {
                        oItem.SetParentsState((o) =>
                        {
                            o.IsVisible = true;
                            o.IsExpanded = true;
                        });
                    }
                    oItem.IsExpanded = false;
                }
                result.Selected = selectedItem;
                result.Result = clone;
                return result;
            }
            else
            {
                var clone = original.DeepCopy();
                PrepareFilterItems(clone, filter, ref result);
                FilterItems(clone, showAll);
                result.Result = clone;
                return result;
            }
        }          

        public static void SelectItem(this ItemsControl container, JsonItem root, bool? next = default)
        {
            var items = root.ToList().Where(o => o.IsMatch).ToList();
            var itemsCount = items.Count;
            if (itemsCount == 0)
            {
                return;
            }

            var currentItem = items.FirstOrDefault(o => o.IsSelected == true);
            if (currentItem == null)
            {
                currentItem = items.FirstOrDefault(o => o.IsMatch == true);
            }
            if (currentItem.IsSelected)
            {
                var nextItem = next.HasValue ?  (next.Value ?
                    GetNextItem(items, currentItem) : 
                    GetPrevItem(items, currentItem)) : currentItem;

                if(nextItem != currentItem)
                {
                    currentItem.IsSelected = false;
                }
                nextItem.IsSelected = true;
                var treeViewItem = container.GetTreeViewItem(nextItem);
                if (treeViewItem != null)
                {
                    treeViewItem.IsSelected = true;
                    treeViewItem.BringIntoView();
                    return;
                }
            }
            else
            {
                currentItem.IsSelected = true;
                var treeViewItem = container.GetTreeViewItem(currentItem);
                if (treeViewItem != null)
                {
                    treeViewItem.BringIntoView();
                }
            }
        }

        private static void PrepareFilterItems(JsonItem root, string filter, ref FilterResponse result)
        {
            if (!string.IsNullOrEmpty(root.Name) && (root.Name.ContainsIgnoreCase(filter) || root.Value.ContainsIgnoreCase(filter)))
            {
                result.Matches.Add(root);
                root.IsMatch = true;
                root.SetParentsState((o) =>
                {
                    o.IsVisible = true;
                    o.IsExpanded = true;
                });
                root.IsExpanded = false;
            }
            else
            {
                root.IsVisible = false;
            }
            if (root.Name == "root")
            {
                root.IsVisible = true;
            }
            if (root.IsSelected)
            {
                result.Selected = root;
            }
            foreach (var node in root.Nodes)
            {
                if (node.IsSelected)
                {
                    result.Selected = node;
                }
                node.IsMatch = node.Name.ContainsIgnoreCase(filter) || root.Value.ContainsIgnoreCase(filter);
                if (node.IsMatch)
                {
                    node.SetParentsState((o) =>
                    {
                        o.IsVisible = true;
                        o.IsExpanded = true;
                    });
                    node.IsExpanded = false;
                }
                PrepareFilterItems(node, filter, ref result);
            }
        }

        private static void FilterItems(JsonItem root, bool showAll)
        {
            if (!root.IsVisible)
            {
                if (!showAll)
                {
                    root.Nodes.Clear();
                }
                return;
            }

            var nodes = showAll ? root.Nodes : root.Nodes.Where(o => o.IsVisible).ToList();
            foreach (var node in nodes)
            {
                FilterItems(node, showAll);
            }
            root.Nodes = nodes;
        }

        private static JsonItem GetPrevItem(IEnumerable<JsonItem> items, JsonItem current)
        {
            var ordered = items.OrderBy(o => o.Index).ToArray();
            for (int i = ordered.Count() - 1; i >= 0; i--)
            {
                var item = ordered[i];
                if (item.Index >= current.Index)
                {
                    continue;
                }
                return item;
            }
            var startItem = ordered.LastOrDefault();
            if (startItem != null)
            {
                return startItem;
            }
            return current;
        }

        private static JsonItem GetNextItem(IEnumerable<JsonItem> items, JsonItem current)
        {
            var ordered = items.OrderBy(o => o.Index).ToArray();
            for (int i = 0; i < ordered.Count(); i++)
            {
                var item = ordered[i];
                if (item.Index > current.Index)
                {
                    return item;
                }
            }
            var startItem = ordered.FirstOrDefault();
            if (startItem != null)
            {
                return startItem;
            }
            return current;
        }

        private static TreeViewItem GetTreeViewItem(this ItemsControl container, JsonItem item)
        {
            if(container == null)
            {
                return null;
            }
            if (container.DataContext == item)
            {
                return container as TreeViewItem;
            }

            container.ApplyTemplate();
            var itemsPresenter =
                (ItemsPresenter)container.Template.FindName("ItemsHost", container);

            if (itemsPresenter != null)
            {
                itemsPresenter.ApplyTemplate();
            }
            else
            {
                itemsPresenter = FindVisualChild<ItemsPresenter>(container);
                if (itemsPresenter == null)
                {
                    container.UpdateLayout();

                    itemsPresenter = FindVisualChild<ItemsPresenter>(container);
                }
            }

            var itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);

            var children = itemsHostPanel.Children;

            var virtualizingPanel =
                itemsHostPanel as VirtualizingStackPanelEx;

            var treeViewItems = new List<TreeViewItem>();

            for (int i = 0, count = container.Items.Count; i < count; i++)
            {
                if (virtualizingPanel != null)
                {
                    virtualizingPanel.BringIntoView(i);
                }
                var subContainer =
                        (TreeViewItem)container.ItemContainerGenerator.ContainerFromIndex(i);

                if (subContainer == null)
                {
                    continue;
                }

                if (virtualizingPanel == null)
                {
                    subContainer.BringIntoView();
                }                            

                if (subContainer.DataContext is JsonItem model)
                {
                    if (model.Index == item.Index)
                    {
                        subContainer.IsExpanded = false;
                        return subContainer;
                    }
                    if (model.Index < item.Index)
                    {
                        treeViewItems.Add(subContainer);
                    }
                }
            }

            if (!treeViewItems.Any())
            {
                return null;
            }

            var conainer =
                treeViewItems.OrderByDescending(o => ((JsonItem)o.DataContext).Index).First();

            conainer.IsExpanded = true;

            return GetTreeViewItem(conainer, item);
        }         

        private static T FindVisualChild<T>(Visual visual) where T : Visual
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
            {
                Visual child = (Visual)VisualTreeHelper.GetChild(visual, i);
                if (child != null)
                {
                    T correctlyTyped = child as T;
                    if (correctlyTyped != null)
                    {
                        return correctlyTyped;
                    }

                    T descendent = FindVisualChild<T>(child);
                    if (descendent != null)
                    {
                        return descendent;
                    }
                }
            }
            return null;
        }

        private static List<JsonItem> ToList(JsonItem node, List<JsonItem> nodes)
        {
            nodes.Add(node);
            foreach (var item in node.Nodes)
            {
                ToList(item, nodes);
            }
            return nodes;
        }
    }
}
