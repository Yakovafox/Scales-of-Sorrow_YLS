using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Ink.Runtime;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image profilePicture;
    [SerializeField] private Sprite dragonProfilePicture;
    [SerializeField] private Sprite playerProfilePicture;

    [SerializeField] private TextAsset startDialogue;
    [SerializeField] private TextAsset endDialgoue;
    private Story currentStory;

    public bool dialogueIsPlaying { get; private set; }

    private static DialogueManager instance;
    [SerializeField] private Management_GameMenus gameMenus;

    private const string SPEAKER_TAG = "speaker";
    private const string PORTRAIT_TAG = "portrait";

    public delegate void DragonBehaviour();
    public static event DragonBehaviour OnDragonBehaviour;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            Debug.LogWarning("Found more than one Dialogue Manager in the scene");
        }
        else { instance = this; }

    }

    private void Start()
    {
        EnterDialogueMode(true);
    }

    private void Update()
    {
        if (!dialogueIsPlaying) { return; }

        if (Input.GetKeyUp(KeyCode.Space) || Input.GetMouseButtonDown(0)) { ContinueStory(); }

    }


    public void EnterDialogueMode(bool intro)
    {
        if (intro) { currentStory = new Story(startDialogue.text);}
        else { currentStory = new Story(endDialgoue.text); }

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

        gameMenus.showGameOverScreen();
    }

    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            dialogueText.text = currentStory.Continue();

            HandleTags(currentStory.currentTags);
        }
        else
        {
            Debug.Log("Exiting Dialogue");
            StartCoroutine(ExitDialogueMode());
        }
    }

    private void HandleTags(List<string> currentTags)
    {
        foreach (string tag in currentTags)
        {
            string[] splitTag = tag.Split(':');

            if (splitTag.Length != 2) { Debug.LogError("Tag failed to parse: " + tag); }

            string tagKey = splitTag[0].Trim();
            string tagValue = splitTag[1].Trim();

            switch (tagKey)
            {
                case SPEAKER_TAG:
                    nameText.text = tagValue;
                    break;

                case PORTRAIT_TAG: 
                    if(tagValue == "Kelp") { profilePicture.sprite = dragonProfilePicture; }
                    else { profilePicture.sprite = playerProfilePicture; }
                    break;

                default:
                    Debug.LogWarning("Tag exists ");
                    break;
            }
        }
    }
}

