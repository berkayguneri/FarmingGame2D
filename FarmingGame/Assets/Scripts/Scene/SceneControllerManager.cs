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
        //"FadeAndSwitchScenes" coroutine'inin tekrar �a�r�lmamas� i�in isFadingi  true olarak ayarla
        isFading = true;

        //Make sure the "CanvasGroup" blocks raycast into the scene so no more input can be accepted
        faderCanvasGroup.blocksRaycasts = true;

        //Calculate how fast the "Canvasgroup" should be fade based on it's current alpha, it' final alpha and how long it has to change between the two
        //"Canvasgroup "un mevcut alfas�na, son alfas�na ve ikisi aras�nda ne kadar s�re
        //de�i�mesi gerekti�ine ba�l� olarak ne kadar h�zl� solmas� gerekti�ini hesaplay�n
        float fadeSpeed = Mathf.Abs(faderCanvasGroup.alpha - finalAlpha) / fadeDuration;


        while (!Mathf.Approximately(faderCanvasGroup.alpha,finalAlpha))
        {
            faderCanvasGroup.alpha = Mathf.MoveTowards(faderCanvasGroup.alpha, finalAlpha, fadeSpeed * Time.deltaTime);

            //Wait for  a frame then continue ---- Bir kare bekleyin, sonra devam edin
            yield return null;
        }

        //Set the flag to false since the fade has finished ---- Solma bitti�inden isFadingi yanl�� olarak ayarlay�n
        isFading = false;

        //Stop the "Canvasgroup" from blocking raycast so input it is no longer ignored ---- 
        //"Canvasgroup "un ���n yay�n�n� engellemesini durdurun, b�ylece girdi art�k g�z ard� edilmez
        faderCanvasGroup.blocksRaycasts = false;

        
    }    
    private IEnumerator FadeAndSwitchScenes(string sceneName, Vector3 spawnPosition)
    {
        //Call before scene unload fade out event ---- Sahne bo�altma solma olay�ndan �nce �a��r�n
        EventHandler.CallBeforeSceneUnloadFadeOutEvent();

        //Start fading to black and wait for it to finish before continuing ---- //Siyah� soldurmaya ba�la ve devam etmeden �nce bitmesini bekle
        yield return StartCoroutine(Fade(1f));

        //Store scene data ---- Sahne verilerini saklama
        SaveLoadManager.Instance.StoreCurrentSceneData();


        //Set player position
        Player.Instance.gameObject.transform.position = spawnPosition;

        //Call before scene unload event --- Sahne bo�altma olay�ndan �nce �a�r� 
        EventHandler.CallBeforeSceneUnloadEvent();

        //Unload the current active scene --- Ge�erli etkin sahneyi bo�alt�n
        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);

        //Start loading the given scene and wait for it to finish --- Verilen sahneyi y�klemeye ba�lay�n ve bitmesini bekleyin
        yield return StartCoroutine(LoadSceneAndSetActive(sceneName));

        //Call after scene load event --- Sahne y�kleme olay�ndan sonra �a�r�
        EventHandler.CallAfterSceneLoadEvent();

        //Restore new scene data ---- Yeni sahne verilerini geri y�kleme
        SaveLoadManager.Instance.RestoreCurrentSceneData();

        //Start fading back in and wait for it to finish before exiting the function --- Geri soldurmay� ba�lat ve i�levden ��kmadan �nce bitmesini bekle
        yield return StartCoroutine(Fade(0f));

        //Call after scene load fade in event
        EventHandler.CallAfterSceneLoadFadeInEvent();
    } 
    private IEnumerator LoadSceneAndSetActive(string sceneName)
    {
        //Allow the given scene to load over several frames and add it to the already loaded scenes (just the Persistent scene at this point)
        //Verilen sahnenin birka� kare boyunca y�klenmesine izin verin ve �nceden y�klenmi� sahnelere ekleyin (bu noktada sadece Persistent sahnesi)
        yield return SceneManager.LoadSceneAsync(sceneName,LoadSceneMode.Additive);

        //Find the scene that was most recently loaded (the one at the last index of the loaded scenes)
        //En son y�klenen sahneyi bulun (y�klenen sahnelerin son dizininde yer alan sahne)
        Scene newlyLoadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

        //Set the newly loaded scene as the active scene (this marks it as the one to be unloaded next)
        //"Yeni" y�klenen sahneyi aktif sahne olarak ayarlay�n (bu, onu bir sonraki bo�alt�lacak sahne olarak i�aretler)
        SceneManager.SetActiveScene(newlyLoadedScene);
    }

    private IEnumerator Start()
    {
        // Set the initial alpha to start off with a black screen --- Ba�lang�� alfas�n� siyah bir ekranla ba�layacak �ekilde ayarlay�n
        fadeImage.color = new Color(0f, 0f, 0f, 1f);
        faderCanvasGroup.alpha = 1f;

        //Start the first scene loading and wait for it to finish --- �lk sahneyi y�klemeye ba�lay�n ve bitmesini bekleyin
        yield return StartCoroutine(LoadSceneAndSetActive(startingSceneName.ToString()));

        //If this event any subscribers, call it
        EventHandler.CallAfterSceneLoadEvent();

        SaveLoadManager.Instance.RestoreCurrentSceneData();

        //Once the scene is finished loading,start fading in --- Sahne y�klemesi tamamland���nda, solmaya ba�lay�n 
        StartCoroutine(Fade(0));
    }

    //This is the main external point of contact and influence from the rest of the project
    //This will be called when the player wants to switch scenes --- Bu, oyuncu sahneleri de�i�tirmek istedi�inde �a�r�lacakt�r
    public void FadeAndLoadScene(string sceneName, Vector3 spawnPosition)
    {
        //If a fade isn't happening then start fading and switching scenes ---- //E�er bir solma ger�ekle�miyorsa, solmaya ve sahneleri de�i�tirmeye ba�lay�n
        if (!isFading)
        {
            StartCoroutine(FadeAndSwitchScenes(sceneName, spawnPosition));
        }
    }
}
