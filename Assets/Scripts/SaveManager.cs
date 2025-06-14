using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    #region SINGLETON

    public static SaveManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion


    [SerializeField] private InventorySO inventory;

    private string savePath = Application.persistentDataPath + "/savefile.json";

    public void SaveGame()
    {
        SaveData saveData = new SaveData();
        saveData.waveNumber = WaveManager.Instance.CurrentWave;

        for (int i = 0; i < inventory.ItemCount; i++)
        {
            var item = inventory.GetItemFromIndex(i);
            saveData.items.Add(new SavedInventoryItem(item));
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);

        Debug.Log($"Game saved to {savePath}");
    }

    public SaveData LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("No save file found!");
            return null;
        }

        string json = File.ReadAllText(savePath);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);
        WaveManager.Instance.StartWave(saveData.waveNumber);
        inventory.Load(saveData.items);

        Debug.Log("Game loaded successfully.");
        return saveData;
    }

    public void Restart()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Save file deleted.");
        }

        WaveManager.Instance.StartWave(0);
        for (int i = 0; i < inventory.ItemCount; i++)
        {
            var item = inventory.GetItemFromIndex(i);
            inventory.RemoveItem(item);
        }

        SaveGame();
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    private IEnumerator Start()
    {
        // Load save game and update inventory
        var curSave = LoadGame();

        yield return null;

        if (curSave != null)
        {
            WaveManager.Instance.StartWave(curSave.waveNumber);
        }
        else
        {
            WaveManager.Instance.StartWave(0);
        }
    }

    private void OnDestroy()
    {
        SaveGame();
    }
}

[System.Serializable]
public class SaveData
{
    public int waveNumber;
    public List<SavedInventoryItem> items = new();
}

[System.Serializable]
public class SavedInventoryItem
{
    public string itemID; // ID can be the name of the scriptable object for now. TODO: Make this bulletproof
    public int quantity;

    public SavedInventoryItem(ItemSO item)
    {
        itemID = item.name;
        quantity = item.TotalAmount;
    }
}
