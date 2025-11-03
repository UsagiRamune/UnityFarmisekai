using UnityEngine;
using System.Collections.Generic;

namespace Ai
{
    /// <summary>
    /// จัดการ Event ที่ทำให้ AI เข้าโหมด Rallying
    /// แทนที่ FlockingManager เพื่อความเรียบง่าย
    /// </summary>
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        void Awake()
        {
            if (Instance && Instance != this) 
            { 
                Destroy(gameObject); 
                return; 
            }
            Instance = this;
        }
        
        [Header("Event Settings")]
        public bool eventActive = false;
        
        [Header("Rally Point")]
        public Transform rallyPoint;          // จุดรวมพลที่ AI จะไป
        
        [SerializeField, Tooltip("Event Status for Inspector")]
        private bool isEventActiveForInspector;
        
        // เก็บ AI ที่ไปถึง rally point แล้ว
        private HashSet<AiController> agentsReachedRally = new HashSet<AiController>();

        void Update()
        {
            // อัปเดต Inspector
            isEventActiveForInspector = eventActive;
            
            // // Debug: กด E เพื่อเปิด/ปิด Event
            // if (Input.GetKeyDown(KeyCode.G))
            // {
            //     ToggleEvent();
            // }
        }
        
        public void ToggleEvent()
        {
            eventActive = !eventActive;
            Debug.Log($"Event Active: {eventActive}");
            
            // ถ้าเริ่ม event ใหม่ ให้ clear รายชื่อที่เคยไปถึงแล้ว
            if (eventActive)
            {
                agentsReachedRally.Clear();
            }
        }
        
        public bool IsEventActive()
        {
            return eventActive;
        }
        
        public Transform GetRallyPoint()
        {
            return rallyPoint;
        }
        
        public void OnAgentReachedRallyPoint(AiController agent)
        {
            if (agentsReachedRally.Add(agent))
            {
                Debug.Log($"Agent {agent.name} reached rally point. Total reached: {agentsReachedRally.Count}");
                /*agent.Mode = AiMode.Patrol;*/

            }
        }
        
        // เช็คว่า AI ตัวนี้เคยไปถึง rally point ในรอบนี้แล้วหรือยัง
        public bool HasAgentReachedRally(AiController agent)
        {
            return agentsReachedRally.Contains(agent);
        }
        
    }
}