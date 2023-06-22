using System.Collections;
using UnityEngine;
using UniRx;
using PathCreation.Examples;
using UniRx.Triggers;

public class GameFlow : MonoBehaviour
{
    public CompositeDisposable subscriptions = new CompositeDisposable();

    [SerializeField] private GameObject[] thingsToTurnOffDuringSlide;
    [SerializeField] private GameObject[] thingsToTurnOnDuringSlide;

    public float elapsedTime;

    [SerializeField] private GameObject speedingVfx;

    [SerializeField] private Animator anim;

    [SerializeField] private RoadMeshCreator roadScript;

    [SerializeField] private Material defaultMat;
    [SerializeField] private Material iceMat;
    [SerializeField] private Material sideIceMat;

    private void Start()
    {
        //roadScript = FindObjectOfType<RoadMeshCreator>();

        //roadScript.roadMaterial = defaultMat;
        //roadScript.undersideMaterial = defaultMat;

        if (speedingVfx != null)
        {
            speedingVfx.SetActive(false);
        }
    }
    private void OnEnable()
    {
        StartCoroutine(Subscribe());
    }

    private IEnumerator Subscribe()
    {
        yield return new WaitUntil(() => GameEvents.instance != null);

        GameEvents.instance.canSlide.ObserveEveryValueChanged(x => x.Value)
            .Subscribe(value =>
            {
                if (value)
                {
                    SetNightSettings();
                }
                else
                {
                    SetDaySettings();
                }
            })
            .AddTo(subscriptions);

        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                RenderSettings.skybox.SetFloat("_CubemapTransition", elapsedTime);
            })
            .AddTo(subscriptions);
        
    }

    void SetNightSettings()
    {
        if(roadScript != null)
        {
            roadScript.roadMaterial = iceMat;
            roadScript.undersideMaterial = sideIceMat;
            roadScript.AssignMaterials();
        }

        anim.SetBool("NighTime", true);

        speedingVfx.SetActive(true);

        ManipulateObjetcsScale(thingsToTurnOnDuringSlide, true);

        ManipulateObjetcsScale(thingsToTurnOffDuringSlide, false);
    }
    void SetDaySettings()
    {
        if(roadScript != null)
        {
            roadScript.roadMaterial = defaultMat;
            roadScript.undersideMaterial = defaultMat;
            roadScript.AssignMaterials();
        }

        anim.SetBool("NighTime", false);

        speedingVfx.SetActive(false);

        ManipulateObjetcsScale(thingsToTurnOnDuringSlide, false);

        ManipulateObjetcsScale(thingsToTurnOffDuringSlide, true);
    }

    private void ManipulateObjetcsScale(GameObject[] objectArray, bool state)
    {
        foreach (GameObject objs in objectArray)
        {
            if (!GameEvents.instance.gameWon.Value && !GameEvents.instance.gameLost.Value)
            {
                //objs.transform.DOScale(size, 1f)
                //.SetEase(Ease.InBounce);
                objs.SetActive(state);
            }
        }
    }

    private void OnDisable()
    {
        subscriptions.Clear();
    }
}
