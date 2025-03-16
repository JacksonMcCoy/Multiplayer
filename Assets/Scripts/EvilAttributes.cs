using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvilAttributes : MonoBehaviour
{
    public Rigidbody2D evil;
    private Coroutine evilMoveControls;
    private float moveIncrement = 1f;
    private float evilMove = 2f;
    private int ymul;
    private int xmul;
    private int randomInt;
    private bool correctionMove = false;

    void Start()
    {
        evil = GetComponent<Rigidbody2D>();
        evil.freezeRotation = true;
        StartNewMovement();
    }

    void Update()
    {
        if (correctionMove == false) // check to make sure we aren't already correcting the location
        {
            if (evil.position.x > 2)
            {
                if (evilMoveControls != null)
                {
                    StopCoroutine(evilMoveControls);
                }
                correctionMove = true;
                evilMoveControls = StartCoroutine(MoveForTime(4));
            }
            else if (evil.position.x < -2)
            {
                if (evilMoveControls != null)
                {
                    StopCoroutine(evilMoveControls);
                }
                correctionMove = true;
                StopCoroutine(evilMoveControls);
                evilMoveControls = StartCoroutine(MoveForTime(2));
            }
            else if (evil.position.y > 5)
            {
                if (evilMoveControls != null)
                {
                    StopCoroutine(evilMoveControls);
                }
                correctionMove = true;
                evilMoveControls = StartCoroutine(MoveForTime(3));
            }
            else if (evil.position.y < -2)
            {
                if (evilMoveControls != null)
                {
                    StopCoroutine(evilMoveControls);
                }
                correctionMove = true;
                evilMoveControls = StartCoroutine(MoveForTime(1));
            }
        }
    }

    void StartNewMovement()
    {
        if (evilMoveControls == null)
        {
            randomInt = Random.Range(1, 5); // 1 up, 2 right, 3 down, 4 left
            evilMoveControls = StartCoroutine(MoveForTime(randomInt));
        }
    }

    IEnumerator MoveForTime(int direction)
    {
        if (direction == 1)
        {
            ymul = 1;
            xmul = 0;
        }
        else if (direction == 2)
        {
            ymul = 0;
            xmul = 1;
        }
        else if (direction == 3)
        {
            ymul = -1;
            xmul = 0;
        }
        else
        {
            ymul = 0;
            xmul = -1;
        }
        float timeElapsed = 0f;
        while (timeElapsed < moveIncrement)
        {
            timeElapsed += Time.deltaTime;
            evil.velocity = new Vector2(xmul * evilMove, ymul * evilMove);
            yield return null;
        }
        correctionMove = false;
        evilMoveControls = null;
        StartNewMovement();
    }
    // evil guy has no collision control because HE IS EVIL he doesn't respect the peppers
}