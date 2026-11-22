using TMPro;
using UnityEngine;

public class RoleBriefingUI : MonoBehaviour
{
    [SerializeField] private TMP_Text briefingText;
    [SerializeField] private RoleSelectionUI roleSelectionUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public void ShowBriefing(CourtroomRole role)
    {
        switch (role)
        {
            case CourtroomRole.Defence:
                briefingText.text =
                    "YOUR ROLE: DEFENCE\n\n" +
                    "You represent the defendant.\n\n" +
                    "Listen to the evidence and consider questions " +
                    "you would ask the witness.\n\n" +
                    "Prototype instructions — subject to review.";
                break;

                case CourtroomRole.Prosecution:
                briefingText.text =
                    "YOUR ROLE: PROSECUTION\n\n" +
                    "You present the case against the defendant.\n\n" +
                    "Consider how the witness's evidence supports " +
                    "your case and what you would ask them.\n\n" +
                    "Prototype instructions — subject to review.";
                break;
        }

        gameObject.SetActive(true);
    }

    public void ReturnToMenu()
    {
        roleSelectionUI.ShowMenu();
        gameObject.SetActive(false);
    }
}
