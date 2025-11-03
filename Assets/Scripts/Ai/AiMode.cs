using UnityEngine;

namespace Ai
{
    public enum AiMode
    {
        Patrol, // เดินสุ่ม
        Chase, // ไล่ล่า
        Search, // หาตำแหน่งล่าสุด
        Rallying, // r
        Flocking, // เคลื่อนที่แบบฝูง
        AttackMelee, // โจมตีระยะประชิด
        AttackRange // โจมตีระยะไกล
    }
}