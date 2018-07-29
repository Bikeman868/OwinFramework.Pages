using System;
using System.Collections.Generic;
using System.Linq;

namespace OwinFramework.Pages.Framework.Utility
{
    public class DependencyListSorter<T>
    {
        public List<T> Sort(List<T> list, Func<T, T, bool> isDependentOn)
        {
            var expandedList = new List<ListItem>();

            for (var i = 0; i < list.Count; i++)
            {
                var listItem = new ListItem
                { 
                    Index = i,
                    DependsOn = new List<int>()
                };
                for (var j = 0; j < list.Count; j++)
                {
                    if (i != j && isDependentOn(list[i], list[j]))
                        listItem.DependsOn.Add(j);
                }
                expandedList.Add(listItem);
            }

            var expanded = true;
            while (expanded)
            {
                expanded = false;
                for (var i = 0; i < expandedList.Count; i++)
                {
                    if (Expand(expandedList, expandedList[i]))
                        expanded = true;
                }
            }

            var orderedList = new List<ListItem>();

            while (expandedList.Count > 0)
            {
                for (var i = 0; i < expandedList.Count; )
                {
                    var listItem = expandedList[i];
                    if (CanAdd(listItem, orderedList))
                    {
                        orderedList.Add(listItem);
                        expandedList.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            return orderedList.Select(l => list[l.Index]).ToList();
        }

        private bool Expand(List<ListItem> list, ListItem item)
        {
            var result = false;
            for (var dependsOnIndex = 0; dependsOnIndex < item.DependsOn.Count; dependsOnIndex++)
            {
                var dependsOn = item.DependsOn[dependsOnIndex];
                var dependent = list[dependsOn];
                foreach (var d in dependent.DependsOn)
                {
                    var inheritedDependency = d;
                    if (item.DependsOn.All(i => i != inheritedDependency))
                    {
                        item.DependsOn.Add(inheritedDependency);
                        result = true;
                    }
                }
            }
            return result;
        }

        private bool CanAdd(ListItem item, List<ListItem> orderedItems)
        {
            if (item.DependsOn.Count == 0) return true;
            return item.DependsOn.All(d => orderedItems.Any(i => i.Index == d));
        }

        private class ListItem
        {
            public int Index;
            public List<int> DependsOn;

            public override string ToString()
            {
                return Index + " [" + string.Join(",", DependsOn) + "]";
            }
        }
    }
}
