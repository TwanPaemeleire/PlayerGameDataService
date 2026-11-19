using UnityEngine;

public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] private Transform _entriesParent;
    public void RefreshUI()
    {
        for(int childIdx = 0; childIdx < _entriesParent.childCount; ++childIdx)
        {
            Destroy(_entriesParent.GetChild(childIdx));
        }
    }
}
