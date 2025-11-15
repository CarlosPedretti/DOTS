public interface IUIItem<T>
{
    /// <summary>
    /// Sets the UIItem with new data
    /// </summary>
    void SetData(T data);

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
