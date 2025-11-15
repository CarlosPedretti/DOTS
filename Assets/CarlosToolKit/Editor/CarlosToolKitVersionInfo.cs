using UnityEngine;

[CreateAssetMenu(fileName = "CarlosToolKitVersion", menuName = "CarlosToolKit/VersionInfo")]
public class CarlosToolKitVersionInfo : ScriptableObject
{
    public string version;
    public string exportDate;
    public string hash;
}