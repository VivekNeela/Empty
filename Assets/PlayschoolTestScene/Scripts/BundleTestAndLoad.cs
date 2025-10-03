using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BundleTestandLoad : MonoBehaviour
{
    public string bundleName;
    // public bool isAndroid;
    public GameObject uiCategorySelectionPrefab;
    public Transform selectionCategoryHolder;
    public GameObject loadingObject;

    private AssetBundle assetBundle;
    private AsyncOperation asyncOperation;
    private string filePathAssetBundle;


    private void Awake()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        // Check and create StreamingAssets folder (only in Editor or Standalone)
#if UNITY_EDITOR || UNITY_STANDALONE
        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath);
            Debug.Log("Created StreamingAssets directory.");
        }
#endif

        // if (isAndroid)
        //     filePathAssetBundle = Path.Combine(Application.streamingAssetsPath + "/android", bundleName);
        // else
        //     filePathAssetBundle = Path.Combine(Application.streamingAssetsPath, bundleName);

        //lets just keep all bundles in streaming assets
        filePathAssetBundle = Path.Combine(Application.streamingAssetsPath, bundleName);
        
    }


    private void Start()
    {
        loadingObject.SetActive(true);
        ReadAssetBundleSceneAndSpawnCategory();
    }

    private void ReadAssetBundleSceneAndSpawnCategory()
    {
        StartCoroutine(GetSceneNameFromBundle());
    }


    private IEnumerator GetSceneNameFromBundle()
    {
        AssetBundle.UnloadAllAssetBundles(true);
        string filePath = filePathAssetBundle;

        /* if (!System.IO.File.Exists(filePath))
          {
              Debug.LogWarning(" Abe Asset  bundle kaa naam sahi daal.   Please check the file path.");
                Debug.Log(" Abe Asset  bundle kaa naam sahi daal.   Please check the file path.");
              yield break;
          }*/

        var assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(filePath);

        yield return assetBundleCreateRequest;

        string[] scenePaths = assetBundleCreateRequest.assetBundle.GetAllScenePaths();

        for (int i = 0; i < scenePaths.Length; i++)
        {
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePaths[i]);

            GameObject spawnedUIObject = Instantiate(uiCategorySelectionPrefab, selectionCategoryHolder);

            int index = i;
            spawnedUIObject.GetComponent<Button>().onClick.AddListener(() => LoadGameScene(index));
            spawnedUIObject.transform.GetChild(1).GetComponent<TMP_Text>().text = sceneName;

            spawnedUIObject.name = sceneName;
        }
        loadingObject.SetActive(false);
    }


    public void LoadGameScene(int _sceneIndex)
    {
        loadingObject.SetActive(true);
        StartCoroutine(LoadSceneFromBundle(_sceneIndex));
        //LoadSceneFromBundle(bundleName);
    }


    private IEnumerator LoadSceneFromBundle(int sceneIndex)
    {
        AssetBundle.UnloadAllAssetBundles(true);

        string filePath = filePathAssetBundle;

        var assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(filePath);

        yield return assetBundleCreateRequest;

        assetBundle = assetBundleCreateRequest.assetBundle;
        string[] scenePaths = assetBundle.GetAllScenePaths();
        if (scenePaths.Length > 0)
        {
            //asyncOperation = SceneManager.LoadSceneAsync(scenePaths[0]);
            asyncOperation = SceneManager.LoadSceneAsync(scenePaths[sceneIndex]);
            // while (!asyncOperation.isDone)
            // {
            //     yield return null;
            // }
        }
        else
        {
            Debug.LogError("No scene found in the loaded AssetBundle.");
        }
        loadingObject.SetActive(false);
        //  loadingObject.SetActive(false);
    }

    public void CloseApplication()
    {
        Application.Quit();
    }

    //this function goes on the lang btns....
    public void SelectLangugae(string language)
    {
        PlayerPrefs.SetString("PlayschoolLanguageAudio", language);
    }



}
