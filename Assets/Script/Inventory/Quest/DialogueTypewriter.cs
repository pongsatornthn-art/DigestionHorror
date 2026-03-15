using System.Collections;
using UnityEngine;
using TMPro; // สำคัญมาก ต้องเรียกใช้ TextMeshPro

public class DialogueTypewriter : MonoBehaviour
{
    [Header("ตั้งค่า UI")]
    public TextMeshProUGUI dialogueText; // ลากช่อง Text ของคุณมาใส่ตรงนี้

    [Header("ความเร็วในการพิมพ์")]
    public float typingSpeed = 0.05f; // ยิ่งค่าน้อย ยิ่งพิมพ์เร็ว (0.05 คือกำลังดี)

    // ฟังก์ชันนี้เอาไว้เรียกใช้ตอนจะเริ่มส่งข้อความใหม่เข้าไป
    public void PlayDialogue(string textToType)
    {
        // หยุดคอร์รูทีนเก่าก่อน (เผื่อผู้เล่นกดข้ามหรือคุยประโยคใหม่รัวๆ)
        StopAllCoroutines();
        StartCoroutine(TypeText(textToType));
    }

    private IEnumerator TypeText(string text)
    {
        // 1. กำหนดข้อความทั้งหมดลงไปก่อน แต่สั่งให้ "ซ่อน" เอาไว้
        dialogueText.text = text;
        dialogueText.maxVisibleCharacters = 0;

        // 2. บังคับให้อัปเดตข้อมูลเพื่อใช้นับจำนวนตัวอักษรที่แท้จริง (ไม่นับพวกสระที่ซ้อนกัน)
        dialogueText.ForceMeshUpdate();
        int totalCharacters = dialogueText.textInfo.characterCount;
        int visibleCount = 0;

        // 3. วนลูปค่อยๆ เปิดให้เห็นทีละตัวอักษร
        while (visibleCount <= totalCharacters)
        {
            dialogueText.maxVisibleCharacters = visibleCount;
            visibleCount++;

            // รอเวลาเสี้ยววินาที ก่อนจะแสดงตัวถัดไป
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}