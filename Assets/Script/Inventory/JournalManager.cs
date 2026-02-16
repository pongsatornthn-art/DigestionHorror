using UnityEngine;

public class JournalManager : MonoBehaviour
{
    public static JournalManager instance;
    public GameObject journalUI;
    public GameObject[] notePages;
    private bool isOpen = false;

    void Awake() => instance = this;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J)) { isOpen = !isOpen; journalUI.SetActive(isOpen); }
    }

    public void UnlockPage(int index)
    {
        if (index >= 0 && index < notePages.Length) notePages[index].SetActive(true);
    }
}