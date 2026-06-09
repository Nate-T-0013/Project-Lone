using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AdaptivePerformance;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static ProgressionItem;


public class LevelHandler : MonoBehaviour
{
    [Header("All child levels automatically assigned")]
    public Level[] levels;
    [HideInInspector] public CameraFollow cam;

    public GameObject player;
    Health playerHealth;
    //currently active level
    public Level activeLevel;

    //SAVE/LOAD
    private string saveFilePath;

    //temporary default level
    [SerializeField]private Level loadLevel;

    [SerializeField] private Level defaultLevel;

    //get all levels as an array, find the camera, find the player, disable all levels then load the correct one from save.
    private void Start()
    {

        saveFilePath = Path.Combine(Application.persistentDataPath, "save.txt");

        cam = GameObject.FindAnyObjectByType<CameraFollow>();
        player = GameObject.Find("Player");
        playerHealth = GameObject.Find("Player").GetComponent<Health>();
        levels = GetComponentsInChildren<Level>(true);
        foreach (Level level in levels)
        {
            if (level.gameObject.activeInHierarchy) DeactivateLevel(level);
        }
        LoadData();
        Debug.Log("Levels found: " + levels.Length);
    }

    //quickload
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F6)) LoadData();
        if (Input.GetKeyDown(KeyCode.Escape)) SceneManager.LoadScene("MainMenu");
    }

    //activate level in the scene and teleport player to the linked door
    public void ActivateLevel(Level level)
    {
        if (!level.gameObject.activeInHierarchy) level.gameObject.SetActive(true);
        activeLevel = level;
        disableProgressionItems();
        deleteProjectiles();
    }

    //make sure progression items are disabled. used in activateLevel
    private void disableProgressionItems()
    {
        ProgressionItem[] items = GetComponentsInChildren<ProgressionItem>(true);

        if (items.Length == 0) return;

        foreach (ProgressionItem item in items)
        {
            bool unlocked = false;
            switch (item.type)
            {
                case ProgressionType.DoubleJump:
                    unlocked = ProgressionGlobals.hasDoubleJump;
                    break;

                case ProgressionType.WaterSuit:
                    unlocked = ProgressionGlobals.hasWaterSuit;
                    break;
            }

            if (unlocked) item.gameObject.SetActive(false);
            else item.gameObject.SetActive(true);
        }
    }

    //delete all projectiles so they dont travel between levels
    private void deleteProjectiles()
    {
        Projectile[] projectiles = FindObjectsByType<Projectile>(FindObjectsSortMode.None);
        foreach (Projectile p in projectiles)
        {
            Destroy(p.gameObject);
        }
    }

    //deactivate level, nullify active level, and destroy all projectiles so we don't have projectiles overlap between levels since they aren't children of the level.
    public void DeactivateLevel(Level level)
    {
        if (level.gameObject.activeInHierarchy) level.gameObject.SetActive(false);
        activeLevel = null;
    }

    #region Save/Load

    //add more later.
    //also needs to save health, ammo, global progression bools for abilities
    public void SaveData()
    {
        string data = activeLevel.name + "\n"
            + "hasDoubleJump = " + ProgressionGlobals.hasDoubleJump + "\n"
            + "hasWaterSuit = " + ProgressionGlobals.hasWaterSuit + "\n"
            //+ any more progression stuff
            //+ health stuff
            ;

        //just reset health at save stations
        playerHealth.resetHealth();

        File.WriteAllText(saveFilePath, data);
        Debug.Log("Game saved to: " + saveFilePath);
    }


    //simple load that simply finds the savepoint in the saved level, enables that level, teleports the player to the given save point for that level, and will eventually apply saved values for health/ammo/global progression bools
    public void LoadData()
    {
        //try to load save file, but if we get any errors we just load the default level.
        try
        {
            loadHelper(saveFilePath);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Load failed: " + e.Message);
            string defaultPath = Path.Combine(Application.streamingAssetsPath, "default_save.txt");
            loadHelper(defaultPath);
            SaveData();
        }
    }

    //allows us to load the level and data
    public void loadHelper(string savePath)
    {
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        
        //read file
        string[] lines = File.ReadAllLines(savePath);

        //get level name to load
        string levelName = lines[0];

        //load progression data and level
        Level levelToLoad = System.Array.Find(levels, l => l.name == levelName);
        ProgressionGlobals.hasDoubleJump = parseBoolLine(lines[1]);
        ProgressionGlobals.hasWaterSuit = parseBoolLine(lines[2]);
        playerHealth.resetHealth();

        if (activeLevel != levelToLoad && activeLevel) DeactivateLevel(activeLevel);

        //reset player state to normal
        pm.playerStateMachine.currentState = pm.normalState;
        
        if (levelToLoad != null)
        {
            //teleport player to save point
            player.transform.position = levelToLoad.GetComponentInChildren<SavePoint>().transform.position + new Vector3(0, 0.005f, 0);
            pm.setVelocity(Vector2.zero);

            //activate level
            ActivateLevel(levelToLoad);

            //avoid camera jitter by clamping and resetting cam vel
            cam.clampCameraPosition(levelToLoad.minMaxCameraX, levelToLoad.minMaxCameraY);
            cam.setVelocity(Vector2.zero);
            cam.transform.position = levelToLoad.GetComponentInChildren<SavePoint>().transform.position;
            Debug.Log(levelName + " loaded");
        }

        //lock cursor on load
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    //parse progression related save data
    bool parseBoolLine(string line)
    {
        string value = line.Split('=')[1].Trim();
        return bool.Parse(value);
    }

    #endregion
}
