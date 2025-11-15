using QFSW.QC;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities.UI
{
    public class ExampleItemManager : UIItemManagerBase<ExampleInfo, ExampleItem>
    {
        private List<ExampleInfo> data = new();

        private void Start()
        {
            RefreshList();
        }

        [Command]
        public void LoadDummyData(int quantity = 3)
        {
            for (int i = 0; i < quantity; i++)
            {
                var playerInfo = new ExampleInfo(Utils.GetRandomName(), Random.Range(1, 5000));
                data.Add(playerInfo);
            }

            RefreshList();
        }

        protected override List<ExampleInfo> GetDataList() => data;

        [Command]
        public void ApplyFilter_OnlyHighScores()
        {
            SetFilter(player => player.Score >= 1000);
            RefreshList();
        }

        [Command]
        public void ApplySort_ByScoreDescending()
        {
            SetSorter((a, b) => b.Score.CompareTo(a.Score));
            RefreshList();
        }

        [Command]
        public void ClearFilterAndSorting()
        {
            ClearFilterAndSorter();
            RefreshList();
        }
    }
}

