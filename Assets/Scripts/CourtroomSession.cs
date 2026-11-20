using UnityEngine;

public class CourtroomSession : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform defenceSpawn;
    [SerializeField] private Transform prosecutionSpawn;

    // [SerializeField] private CourtroomRole testRole;

    // private void Start()
    // {
    //     StartSession(testRole);
    // }   

    public void StartSession(CourtroomRole role)
    {
        Transform spawn;

        switch (role)
        {
            case CourtroomRole.Defence:
                spawn = defenceSpawn;
                break;

            case CourtroomRole.Prosecution:
                spawn = prosecutionSpawn;
                break;

            default:
                Debug.LogError("Unsupported courtroom role");
                return;
        }

        player.SetPositionAndRotation(spawn.position, spawn.rotation);

        Debug.Log("Session started as " + role);
    }
}
