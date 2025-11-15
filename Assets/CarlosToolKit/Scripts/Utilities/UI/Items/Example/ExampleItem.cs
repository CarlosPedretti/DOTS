using UnityEngine;
using TMPro;

namespace Utilities.UI
{
    public class ExampleItem : UIItemBase<ExampleInfo>
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI scoreText;

        public override void Refresh()
        {
            nameText.text = currentData.Name;
            scoreText.text = currentData.Score.ToString();
        }
    }

    public class ExampleInfo
    {
        public string Name;
        public float Score;

        public ExampleInfo(string name, float score)
        {
            Name = name;
            Score = score;
        }
    }
}

