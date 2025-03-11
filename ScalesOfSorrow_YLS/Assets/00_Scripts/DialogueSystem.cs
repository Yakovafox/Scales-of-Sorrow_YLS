using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Ink.Runtime;

public class DialogueManager : MonoBehaviour
{

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [SerializeField] private TextAsset startDialogue;
    [SerializeField] private TextAsset finalDialogue;
    private Story currentStory;
    public bool dialogueIsPlaying { get; private set; }

    private static DialogueManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            Debug.LogWarning("Found more than one Dialogue Manager in the scene");
        }
        else { instance = this; }

        Time.timeScale = 0;
    }

    private void Start()
    {
        dialogueIsPlaying = true;
        EnterDialogueMode(startDialogue);
    }

    private void Update()
    {
        if (!dialogueIsPlaying) { return; }

        if (Input.GetKeyUp(KeyCode.Space) || Input.GetMouseButtonDown(0)) { ContinueStory(); }

    }


    public void EnterDialogueMode(TextAsset inkJSON)
    {
        currentStory = new Story(inkJSON.text);
        dialogueIsPlaying = true;
        dialoguePanel.SetActive(true);

        ContinueStory();
    }

    private IEnumerator ExitDialogueMode()
    {
        yield return new WaitForSeconds(0.2f);

        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
    }

    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            // set text for the current dialogue line
            dialogueText.text = currentStory.Continue();
        }
        else
        {
            StartCoroutine(ExitDialogueMode());
            Time.timeScale = 1f;
        }
    }
}

