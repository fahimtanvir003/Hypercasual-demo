using UnityEngine;
using DG.Tweening;
using System.Collections;

public class PlayerCollisions : MonoBehaviour
{
    [SerializeField] private GameObject bloodParticles;
    [SerializeField] private GameObject collectParticles;
    private Animator playerAnim;

    private AudioSource source;

    private void Awake()
    {
        playerAnim = GetComponent<Animator>();
        bloodParticles.SetActive(false);
        collectParticles.SetActive(false);

        source = gameObject.AddComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Size")
        {
            source.PlayOneShot(GameEvents.instance.sfxWaterCollected);

            GameEvents.instance.playerSize.Value += 1;

            if (!GameEvents.instance.canSlide.Value)
            {
                GameEvents.instance.collectedWater.Value += 1;
                if (!collectParticles.activeInHierarchy)
                {
                    collectParticles.SetActive(true);
                }
                else
                {
                    collectParticles.SetActive(false);
                    collectParticles.SetActive(true);
                }
                StartCoroutine(TurnOffObject(collectParticles, 0.2f));
            }

            other.GetComponent<Collider>().enabled = false;
            other.transform.DOScale(Vector3.zero, 0.5f).OnComplete(()=>
            {
                Destroy(other.gameObject);
            });
        }
        if (other.tag == "Obstacle")
        {
            source.PlayOneShot(GameEvents.instance.sfxBirckBreaker);

            playerAnim.SetTrigger("kick");
            other.GetComponent<Block>().CheckHit();
        }
        if (other.tag == "Gate")
            other.GetComponent<Gate>().ExecuteOperation();

        if (other.tag == "Saw")
        {
            Handheld.Vibrate();

            source.PlayOneShot(GameEvents.instance.sfxFleshHit);

            GameEvents.instance.gameLost.SetValueAndForceNotify(true);
            bloodParticles.SetActive(true);
            GetComponent<Collider>().enabled = false;

            Invoke(nameof(PlayGameOverSfx), 0.7f);
        }
        if (other.tag == "Finish")
        {
            GameEvents.instance.gameWon.SetValueAndForceNotify(true);
            source.PlayOneShot(GameEvents.instance.sfxGameWon);
        }

        if (other.CompareTag("PunchingObstacle"))
        {
            Handheld.Vibrate();

            source.PlayOneShot(GameEvents.instance.sfxFleshHit);

            other.GetComponentInParent<PunchingObstacle>().SimulateHit(other);
            GameEvents.instance.gameLost.SetValueAndForceNotify(true);

            GetComponent<Collider>().enabled = false;

            Invoke(nameof(PlayGameOverSfx), 0.7f);
        }
    }

    void PlayGameOverSfx()
    {
        source.PlayOneShot(GameEvents.instance.sfxGameOver);
    }

    private IEnumerator TurnOffObject(GameObject obj, float waitTimer)
    {
        yield return new WaitForSeconds(waitTimer);
        obj.SetActive(false);
    }
}