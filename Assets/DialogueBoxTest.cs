using UnityEngine;
using UnityEngine.UI;
using DS.Data;

public class DialogueBoxTest : MonoBehaviour
{
    [SerializeField] private DSDialogue dialogue;
    [SerializeField] private GameObject choicePrefab;
    [SerializeField] private Transform choiceParent;
    [SerializeField] private Text dialogueText;

    private void Start()
    {
        LoadNextDialog(dialogue);
    }

    private void LoadNextDialog(DSDialogue dialogue)
    {
        ClearChoices();

        if (dialogue == null)
        {
            gameObject.SetActive(false);
            return;
        }

        //dialogueText.text = dialogue.Text;
        //foreach (var choice in dialogue.Choices)
        //{
        //    GameObject instance = Instantiate(choicePrefab, choiceParent);

        //    instance.GetComponentInChildren<Button>().onClick.AddListener(() => LoadNextDialog(choice.NextDialogue));
        //    instance.GetComponentInChildren<Text>().text = choice.Text;

        //    instance.SetActive(true);
        //}
    }

    private void ClearChoices()
    {
        foreach (Transform choice in choiceParent)
        {
            Destroy(choice.gameObject);
        }
    }
}
