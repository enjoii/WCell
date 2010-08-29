using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.AI
{
    public class AIEventMap
    {
        private int m_time, m_phase;
        private Dictionary<int, int> dic;
        public AIEventMap() 
        {
            m_time = 0;
            m_phase = 0;
            dic = new Dictionary<int, int>();
        }

        public int GetTimer { get {return m_time;} }

        public void Reset() { dic.Clear(); m_time = 0; m_phase = 0;}

        public void Update(int time) { m_time += time; }

        public int GetPhaseMask { get { return (m_phase >> 24) & 0xFF; } }

        public void SetPhase(int phase)
        {
            if ((phase != 0) && phase < 9)
                m_phase = (1 << (phase + 24));
        }

        void ScheduleEvent(int eventId, int time, int gcd = 0, int phase = 0)
        {
            time += m_time;
            if ((gcd != 0) && gcd < 9)
                eventId |= (1 << (gcd + 16));
            if ((phase != 0) && phase < 9)
                eventId |= (1 << (phase + 24));
            int value;
            while(dic.TryGetValue(time, out value))
                ++time;

            dic.Add(time, eventId);
        }

        void RescheduleEvent(int eventId, int time, int gcd = 0, int phase = 0)
        {
            CancelEvent(eventId);
            ScheduleEvent(eventId, time, gcd, phase);
        }

        void RepeatEvent(int time)
        {
            if (dic.Count == 0)
                return;
            int eventId;
            if (dic.TryGetValue(time, out eventId))
            {
                dic.Remove(time);
                time += m_time;
                int value;
                while (dic.TryGetValue(time, out value))
                    ++time;
                dic.Add(time, eventId);
            }
        }
        /* Not sure how to implement this yet or how it will be used.
        void PopEvent()
        {
            erase(begin());
        } */

        int ExecuteEvent()
        {
            foreach (KeyValuePair<int, int> kvp in dic)
            {
                if (kvp.Key > m_time)
                    return 0;
                else if ((m_phase != 0) && ((kvp.Value & 0xFF000000) != 0) && ((kvp.Value & m_phase) == 0))
                    dic.Remove(kvp.Key);
                else
                {
                    int eventId = (kvp.Value & 0x0000FFFF);
                    dic.Remove(kvp.Key);
                    return eventId;
                }
            }
            return 0;
        }

        int GetEvent()
        {
            foreach(KeyValuePair<int, int> kvp in dic)
            {
                if (kvp.Key > m_time)
                    return 0;
                else if ((m_phase != 0) && ((kvp.Value & 0xFF000000) != 0) && ((kvp.Value & m_phase) == 0))
                    dic.Remove(kvp.Key);
                else
                    return (kvp.Value & 0x0000FFFF);
            }
            return 0;
        }

        // Delay all events
        void DelayEvents(int delay)
        {
            if (delay < m_time)
                m_time -= delay;
            else
                m_time = 0;
        }

        // Delay all events having the specified Global Cooldown.
        void DelayEvents(int delay, int gcd)
        {
            int nextTime = m_time + delay;
            gcd = (1 << (gcd + 16));
            foreach(KeyValuePair<int, int> kvp in dic)
            {
                if (kvp.Key < nextTime)
                    break;
                if ((kvp.Value & gcd) != 0)
                {
                    ScheduleEvent(kvp.Value, kvp.Key-m_time+delay);
                    dic.Remove(kvp.Key);
                }
            }
        }

        void CancelEvent(int eventId)
        {
            foreach(KeyValuePair<int, int> kvp in dic)
            {
                if (eventId == (kvp.Value & 0x0000FFFF))
                {
                    dic.Remove(kvp.Key);
                }
            }
        }

        void CancelEventsByGCD(int gcd)
        {
            gcd = (1 << (gcd + 16));

            foreach(KeyValuePair<int, int> kvp in dic)
            {
                if ((kvp.Value & gcd) != 0)
                {
                    dic.Remove(kvp.Key);
                }
            }
        }
    }
}
