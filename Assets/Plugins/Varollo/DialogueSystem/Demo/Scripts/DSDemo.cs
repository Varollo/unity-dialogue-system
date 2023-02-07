using UnityEngine;
using UnityEngine.UI;
using DS.Data;

namespace DS.Demo
{
    public class DSDemo : MonoBehaviour
    {
        [SerializeField] private DSDialogueGraph dialogue;
        [SerializeField] private GameObject choicePrefab;
        [SerializeField] private Transform choiceParent;
        [SerializeField] private Text speakerText;
        [SerializeField] private Text dialogueText;

        private void Start()
        {
            LoadNextDialog(dialogue.GetDialogue());
        }

        private void LoadNextDialog(DSDialogue dialogue)
        {
            ClearChoices();

            if (dialogue == null)
            {
                gameObject.SetActive(false);
                return;
            }

            speakerText.text = dialogue.Speaker;
            dialogueText.text = dialogue.Text;

            foreach (var choice in dialogue.Choices)
            {
                GameObject instance = Instantiate(choicePrefab, choiceParent);

                instance.GetComponentInChildren<Button>().onClick.AddListener(() => LoadNextDialog(choice.Next));
                instance.GetComponentInChildren<Text>().text = choice.Text;

                instance.SetActive(true);
            }
        }

        private void ClearChoices()
        {
            foreach (Transform choice in choiceParent)
            {
                Destroy(choice.gameObject);
            }
        }
    }
}