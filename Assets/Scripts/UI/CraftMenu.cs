using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftMenu : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private GameObject _closeButton;
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _jobCountText;
    [SerializeField] private Toggle _continuousToggle;
    private WorldTile Tile;

    public void Initialize(WorldTile tile)
    {
        // Store the tile we are querying
        Tile = tile;
        // Whenever the tile updates, we need to refresh the UI
        tile.OnWorldTileUpdated += Refresh;
        // Initialise the toggle and title
        _continuousToggle.isOn = Tile.Continuous;
        _title.text = Tile.Name;
        Refresh(tile);
        // Tweens the panel into view
        _panel.transform.localScale = Vector3.zero;
        _closeButton.SetActive(false);
        _panel.transform.DOScale(1, 0.25f).OnComplete(() => { _closeButton.SetActive(true); });
    }

    private void Refresh(WorldTile tile)
    {
        // Each time the tile is updated, we update the text
        Tile = tile;
        _jobCountText.text = $"Currently Queued Jobs: {tile.JobCount}";
    }
    public void Close()
    {
        Tile.OnWorldTileUpdated -= Refresh;
        // Scale out the panel and then destroy the dialog
        _closeButton.SetActive(false);
        _panel.transform.DOScale(0, 0.25f).OnComplete(() => { Destroy(gameObject); });
    }

    public void ClearQueue()
    {
        Tile.SetJobCount(0);
    }
    // The number of jobs is set on the button being clicked
    public void AddJobs(int jobs)
    {
        Tile.SetJobCount(Tile.JobCount + jobs);
    }
    // This updates the continuous nature based on the toggle
    public void SetContinuous()
    {
        Tile.SetContinuous(_continuousToggle.isOn);
    }
}
