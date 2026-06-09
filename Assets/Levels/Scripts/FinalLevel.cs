using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class FinalLevel : MonoBehaviour
{

    private const string menuLoad = "MainMenu";
    private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(audioClip);
        StartCoroutine(AutoReturnToMenu());
    }


    IEnumerator AutoReturnToMenu()
    {
        yield return new WaitForSeconds(5);

        SceneManager.LoadScene(menuLoad);
    }

}
