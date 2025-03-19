using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Ink.Runtime;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{

    [Header("Dialogue UI")] [SerializeField]
    private GameObject dialoguePanel;

    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image profilePicture;
    [SerializeField] private Sprite dragonProfilePicture;
    [SerializeField] private Sprite playerProfilePicture;

    [SerializeField] private TextAsset startDialogue;
    [SerializeField] private TextAsset endDialogue;

    PlayerControls playerControls;

    private Story currentStory;
    public bool dialogueIsPlaying { get; private set; }
    public bool canSkip { get; private set; }

    private static DialogueManager instance;
    [SerializeField] private Management_GameMenus gameMenus;

    bool isEnd;

    private const string SPEAKER_TAG = "speaker";
    private const string PORTRAIT_TAG = "portrait";



    public delegate void DragonBehaviour();

    public static event DragonBehaviour OnDragonBehaviour;

    public delegate void PlayerAttacking();

    public static event PlayerAttacking OnPlayerAttacking;
    
    public delegate void PlayerDashing();

    public static event PlayerDashing OnPlayerDashing;

    public delegate void endedDialogue();

    public static event endedDialogue OnEndedDialogue;


    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            Debug.LogWarning("Found more than one Dialogue Manager in the scene");
        }
        else
        {
            instance = this;
        }

        playerControls = new PlayerControls();

    }

    private void Start()
    {
        EnterDialogueMode(true);
    }


    private void Update()
    {
        if (!dialogueIsPlaying)
        {
            return;
        }

        bool isButtonDown = playerControls.Player.DialogueSkip.ReadValue<float>() > 0.1f;

    }
    
    public void EnterDialogueMode(bool start)
    {
        if (start) { currentStory = new Story(startDialogue.text); }
        else 
        {
            currentStory = new Story (endDialogue.text);
            isEnd = true;
        }
        dialogueIsPlaying = true;
        dialoguePanel.SetActive(true);

        OnPlayerDashing?.Invoke();
        
        ContinueStory();
    }

    public void skipStory()
    {
        if (canSkip)
        {
            StartCoroutine(canSkipDelay());
            ContinueStory();
        }
    }

    private IEnumerator canSkipDelay()
    {
        canSkip = false;
        yield return new WaitForSeconds(3f);
        canSkip = true;
    }

    private IEnumerator ExitDialogueMode()
    {
        yield return new WaitForSeconds(0.2f);

        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";

        if (isEnd) { gameMenus.showGameOverScreen(); }
        else 
        {
            OnDragonBehaviour();
            OnPlayerAttacking();
            OnPlayerDashing();
        }
        OnEndedDialogue();
    }

    public void ContinueStory()
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

