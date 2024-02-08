using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneControllerManager : SingletonMonobehaviour<SceneControllerManager>
{
    private bool isFading;

    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private CanvasGroup faderCanvasGroup = null;
    [SerializeField] private Image fadeImage = null;

    public SceneName startingSceneName;

    //This is the coroutine where the 'building blocks' of the script are put together.
    IEnumerator Fade(float finalAlpha)
    {
        //set the fading flag to true so the "FadeAndSwitchScenes" coroutine won't be called again
        //"FadeAndSwitchScenes" coroutine'inin tekrar çaðrýlmamasý için isFadingi  true olarak ayarla
        isFading = true;

        //Make sure the "CanvasGroup" blocks raycast into the scene so no more input can be accepted
        faderCanvasGroup.blocksRaycasts = true;

        //Calculate how fast the "Canvasgroup" should be fade based on it's current alpha, it' final alpha and how long it has to change between the two
        //"Canvasgroup "un mevcut alfasýna, son alfasýna ve ikisi arasýnda ne kadar süre
        //deðiþmesi gerektiðine baðlý olarak ne kadar hýzlý solmasý gerektiðini hesaplayýn
        float fadeSpeed = Mathf.Abs(faderCanvasGroup.alpha - finalAlpha) / fadeDuration;


        while (!Mathf.Approximately(faderCanvasGroup.alpha,finalAlpha))
        {
            faderCanvasGroup.alpha = Mathf.MoveTowards(faderCanvasGroup.alpha, finalAlpha, fadeSpeed * Time.deltaTime);

            //Wait for  a frame then continue ---- Bir kare bekleyin, sonra devam edin
            yield return null;
        }

        //Set the flag to false since the fade has finished ---- Solma bittiðinden isFadingi yanlýþ olarak ayarlayýn
        isFading = false;

        //Stop the "Canvasgroup" from blocking raycast so input it is no longer ignored ---- 
        //"Canvasgroup "un ýþýn yayýnýný engellemesini durdurun, böylece girdi artýk göz ardý edilmez
        faderCanvasGroup.blocksRaycasts = false;

        
    }    
    private IEnumerator FadeAndSwitchScenes(string sceneName, Vector3 spawnPosition)
    {
        //Call before scene unload fade out event ---- Sahne boþaltma solma olayýndan önce çaðýrýn
        EventHandler.CallBeforeSceneUnloadFadeOutEvent();

        //Start fading to black and wait for it to finish before continuing ---- //Siyahý soldurmaya baþla ve devam etmeden önce bitmesini bekle
        yield return StartCoroutine(Fade(1f));

        //Store scene data ---- Sahne verilerini saklama
        SaveLoadManager.Instance.StoreCurrentSceneData();


        //Set player position
        Player.Instance.gameObject.transform.position = spawnPosition;

        //Call before scene unload event --- Sahne boþaltma olayýndan önce çaðrý 
        EventHandler.CallBeforeSceneUnloadEvent();

        //Unload the current active scene --- Geçerli etkin sahneyi boþaltýn
        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);

        //Start loading the given scene and wait for it to finish --- Verilen sahneyi yüklemeye baþlayýn ve bitmesini bekleyin
        yield return StartCoroutine(LoadSceneAndSetActive(sceneName));

        //Call after scene load event --- Sahne yükleme olayýndan sonra çaðrý
        EventHandler.CallAfterSceneLoadEvent();

        //Restore new scene data ---- Yeni sahne verilerini geri yükleme
        SaveLoadManager.Instance.RestoreCurrentSceneData();

        //Start fading back in and wait for it to finish before exiting the function --- Geri soldurmayý baþlat ve iþlevden çýkmadan önce bitmesini bekle
        yield return StartCoroutine(Fade(0f));

        //Call after scene load fade in event
        EventHandler.CallAfterSceneLoadFadeInEvent();
    } 
    private IEnumerator LoadSceneAndSetActive(string sceneName)
    {
        //Allow the given scene to load over several frames and add it to the already loaded scenes (just the Persistent scene at this point)
        //Verilen sahnenin birkaç kare boyunca yüklenmesine izin verin ve önceden yüklenmiþ sahnelere ekleyin (bu noktada sadece Persistent sahnesi)
        yield return SceneManager.LoadSceneAsync(sceneName,LoadSceneMode.Additive);

        //Find the scene that was most recently loaded (the one at the last index of the loaded scenes)
        //En son yüklenen sahneyi bulun (yüklenen sahnelerin son dizininde yer alan sahne)
        Scene newlyLoadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

        //Set the newly loaded scene as the active scene (this marks it as the one to be unloaded next)
        //"Yeni" yüklenen sahneyi aktif sahne olarak ayarlayýn (bu, onu bir sonraki boþaltýlacak sahne olarak iþaretler)
        SceneManager.SetActiveScene(newlyLoadedScene);
    }

    private IEnumerator Start()
    {
        // Set the initial alpha to start off with a black screen --- Baþlangýç alfasýný siyah bir ekranla baþlayacak þekilde ayarlayýn
        fadeImage.color = new Color(0f, 0f, 0f, 1f);
        faderCanvasGroup.alpha = 1f;

        //Start the first scene loading and wait for it to finish --- Ýlk sahneyi yüklemeye baþlayýn ve bitmesini bekleyin
        yield return StartCoroutine(LoadSceneAndSetActive(startingSceneName.ToString()));

        //If this event any subscribers, call it
        EventHandler.CallAfterSceneLoadEvent();

        SaveLoadManager.Instance.RestoreCurrentSceneData();

        //Once the scene is finished loading,start fading in --- Sahne yüklemesi tamamlandýðýnda, solmaya baþlayýn 
        StartCoroutine(Fade(0));
    }

    //This is the main external point of contact and influence from the rest of the project
    //This will be called when the player wants to switch scenes --- Bu, oyuncu sahneleri deðiþtirmek istediðinde çaðrýlacaktýr
    public void FadeAndLoadScene(string sceneName, Vector3 spawnPosition)
    {
        //If a fade isn't happening then start fading and switching scenes ---- //Eðer bir solma gerçekleþmiyorsa, solmaya ve sahneleri deðiþtirmeye baþlayýn
        if (!isFading)
        {
            StartCoroutine(FadeAndSwitchScenes(sceneName, spawnPosition));
        }
    }
}
