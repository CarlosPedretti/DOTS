using UnityEngine;

public interface IConfiguration
{
    string PrefsKey { get; }
    bool IsModified { get; }

    void Initialize();
    void SaveConfiguration();
    void RevertToSaved();
    void SetDefaultValue(object newValue);
    void SetToDefault();
    void InvokeEvent();
}
