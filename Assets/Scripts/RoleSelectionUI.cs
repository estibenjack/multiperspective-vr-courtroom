using UnityEngine;

public class RoleSelectionUI : MonoBehaviour
{
    [SerializeField] private CourtroomSession courtroomSession;
    [SerializeField] private RoleBriefingUI roleBriefingUI;

    private CourtroomRole selectedRole;
    private bool hasSelectedRole;

    public void SelectDefence()
    {
        selectedRole = CourtroomRole.Defence;
        hasSelectedRole = true;
        Debug.Log("Selected Defence");
    }

    public void SelectProsecution()
    {
        selectedRole = CourtroomRole.Prosecution;
        hasSelectedRole = true;
        Debug.Log("Selected Prosecution");
    }

    public void EnterCourtroom()
    {
        if (!hasSelectedRole)
        {
            Debug.Log("Please select a role first.");
            return;
        }

        courtroomSession.StartSession(selectedRole);
        roleBriefingUI.ShowBriefing(selectedRole);
        gameObject.SetActive(false);
    }

    public void ShowMenu()
    {
        hasSelectedRole = false;
        gameObject.SetActive(true);
    }
}
