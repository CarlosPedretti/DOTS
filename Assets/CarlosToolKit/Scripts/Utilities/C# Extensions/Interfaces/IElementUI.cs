namespace Utilities.UI
{
    /// <summary>
    /// Defines the basic behavior that any UI element should implement.
    /// </summary>
    public interface IElementUI
    {
        /// <summary>
        /// Initializes the UI element.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Updates or refreshes the visual data of the UI element.
        /// </summary>
        void Refresh();

        /// <summary>
        /// Makes the UI element visible.
        /// </summary>
        void Show();

        /// <summary>
        /// Hides the UI element.
        /// </summary>
        void Hide();
    }

}