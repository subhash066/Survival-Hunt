using UnityEngine;

public class Boar : MonoBehaviour
{
    public enum State { Idle, Eat, RunAway, Death, Sleep }
    public State currentState = State.Idle;

    public Transform player;
    public int bulletsHit = 0;

    Animator anim;

    void Start()
    {
        // Only get Animator if it exists
        anim = GetComponent<Animator>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        switch (currentState)
        {
            case State.Idle:
                PlayAnim("Idle");
                break;

            case State.Eat:
                PlayAnim("Eat");
                break;

            case State.RunAway:
                PlayAnim("RunBack");
                break;

            case State.Death:
                PlayAnim("Death");
                Invoke("GoToSleep", 3f);
                break;

            case State.Sleep:
                PlayAnim("Sleep");
                break;
        }
    }

    public void OnBulletHit()
    {
        bulletsHit++;
        currentState = State.RunAway;

        if (bulletsHit >= 5)
        {
            currentState = State.Death;
        }
    }

    void GoToSleep()
    {
        currentState = State.Sleep;
    }

    void PlayAnim(string triggerName)
    {
        if (anim == null) return; // skip if no Animator
        anim.SetTrigger(triggerName);
    }
}
