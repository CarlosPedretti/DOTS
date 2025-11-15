using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities.UI
{
    /// <summary>
    /// Base class for managing a list of UI items. 
    /// Handles filtering, sorting, and pooling of UI elements.
    /// </summary>
    /// <typeparam name="TData">The type of data represented by each item.</typeparam>
    /// <typeparam name="TItem">The type of UI item component.</typeparam>
    public abstract class UIItemManagerBase<TData, TItem> : MonoBehaviour
        where TItem : MonoBehaviour, IUIItem<TData>
    {
        [SerializeField] private Transform contentParent;
        [SerializeField] private TItem itemPrefab;

        private readonly List<TItem> itemPool = new();

        private Func<TData, bool> filterPredicate = null;
        private Comparison<TData> sortComparison = null;

        /// <summary>
        /// Initializes all items in the pool with the current data list.
        /// This will create items if necessary, set their data, and call Initialize().
        /// </summary>
        public void InitializeAllItems()
        {
            var dataList = GetDataList();

            for (int i = 0; i < dataList.Count; i++)
            {
                TItem item;
                if (i < itemPool.Count)
                {
                    item = itemPool[i];
                }
                else
                {
                    item = Instantiate(itemPrefab, contentParent);
                    itemPool.Add(item);
                }

                item.SetData(dataList[i]);
                item.Initialize();
                item.Show();
            }

            for (int i = dataList.Count; i < itemPool.Count; i++)
            {
                itemPool[i].Hide();
            }
        }

        /// <summary>
        /// Sets the filter predicate used to determine which items should be shown.
        /// </summary>
        /// <param name="predicate">A function that returns true for items that should be included.</param>
        public void SetFilter(Func<TData, bool> predicate)
        {
            filterPredicate = predicate;
        }

        /// <summary>
        /// Sets the sorting method to determine the order of displayed items.
        /// </summary>
        /// <param name="comparison">A comparison function to sort the data list.</param>
        public void SetSorter(Comparison<TData> comparison)
        {
            sortComparison = comparison;
        }

        /// <summary>
        /// Clears any applied filters and sorting rules.
        /// </summary>
        public void ClearFilterAndSorter()
        {
            filterPredicate = null;
            sortComparison = null;
        }

        /// <summary>
        /// Refreshes the UI item list using the current filter and sorting rules.
        /// </summary>
        public virtual void RefreshList()
        {
            var dataList = GetDataList();

            if (filterPredicate != null)
                dataList = dataList.FindAll(new Predicate<TData>(filterPredicate));

            if (sortComparison != null)
                dataList.Sort(sortComparison);

            RefreshList(dataList);
        }

        /// <summary>
        /// Returns the original data list to be visualized in the UI.
        /// Must be implemented by the inheriting class.
        /// </summary>
        /// <returns>A list of data items.</returns>
        protected abstract List<TData> GetDataList();

        /// <summary>
        /// Refreshes the UI item list with a specific list of data.
        /// Instantiates or reuses UI items from the pool.
        /// </summary>
        /// <param name="dataList">The list of data to display in the UI.</param>
        public virtual void RefreshList(List<TData> dataList)
        {
            for (int i = 0; i < dataList.Count; i++)
            {
                TItem item;
                if (i < itemPool.Count)
                {
                    item = itemPool[i];
                }
                else
                {
                    item = Instantiate(itemPrefab, contentParent);
                    itemPool.Add(item);
                }

                item.SetData(dataList[i]);
                item.Show();
            }

            for (int i = dataList.Count; i < itemPool.Count; i++)
            {
                itemPool[i].Hide();
            }
        }
    }
}

