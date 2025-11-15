using UnityEngine;

namespace Utilities.UI
{
    /// <summary>
    /// Base class for individual UI items that display data of type T.
    /// Provides methods for initialization, data assignment, refresh, and visibility control.
    /// </summary>
    /// <typeparam name="T">The type of data this UI item displays.</typeparam>
    public abstract class UIItemBase<T> : MonoBehaviour, IUIItem<T>
    {
        /// <summary>
        /// The currently assigned data displayed by this UI item.
        /// </summary>
        protected T currentData;

        /// <summary>
        /// Assigns new data to this UI item and updates the UI accordingly.
        /// Calls <see cref="Refresh"/> to reflect the changes in the UI.
        /// </summary>
        /// <param name="data">The data to display.</param>
        public virtual void SetData(T data)
        {
            currentData = data;
            Refresh();
        }

        /// <summary>
        /// Initializes the UI item.
        /// Can be overridden to perform setup tasks before data is assigned.
        /// </summary>
        public virtual void Initialize()
        {

        }

        /// <summary>
        /// Refreshes the UI to reflect the current data.
        /// Should be overridden in derived classes to update UI elements.
        /// </summary>
        public virtual void Refresh()
        {

        }

        /// <summary>
        /// Shows the UI item by enabling its GameObject.
        /// </summary>
        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Hides the UI item by disabling its GameObject.
        /// </summary>
        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
