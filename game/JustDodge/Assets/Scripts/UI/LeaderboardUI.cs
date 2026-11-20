using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] private Transform _entriesParent;
    [SerializeField] private float _refreshTime = 30.0f;

    private void Start()
    {
        StartCoroutine(RefreshCoroutine());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public async void RefreshUI()
    {
        FillRows(null, "...");
        List<LeaderboardEntry> entries = await LeaderboardService.Instance.GetTop10Scores();
        if (this == null) return;
        if (entries == null)
        {
            FillRows(null, "OFFLINE");
            return;
        }
        FillRows(entries, "XXX");
    }

    private void FillRows(List<LeaderboardEntry> entries, string emptyName)
    {
        for(int childIdx = 0; childIdx < _entriesParent.childCount; ++childIdx)
        {
            LeaderboardEntryUI uiEntry = _entriesParent.GetChild(childIdx).GetComponent<LeaderboardEntryUI>();
            bool hasEntry = entries != null && childIdx < entries.Count;
            uiEntry.NameText.text = hasEntry ? entries[childIdx].playerName : emptyName;
            uiEntry.ScoreText.text = hasEntry ? entries[childIdx].value.ToString() : "000";
            uiEntry.RankText.text = (childIdx + 1) + ".";
        }
    }

    private IEnumerator RefreshCoroutine()
    {
        while(true)
        {
            RefreshUI();
            yield return new WaitForSeconds(_refreshTime);
        }
    }
}
